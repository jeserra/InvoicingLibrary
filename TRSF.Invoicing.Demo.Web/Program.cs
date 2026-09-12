using System.Text.Json;
using System.Text.Json.Serialization;
using GenerateCuratedCatalogDb;
using TRSF.Invoicing.BindingModels;
using TRSF.Invoicing.Catalogs.Sqlite;
using TRSF.Invoicing.CFDI;
using TRSF.Invoicing.ConstanciaFiscal;
using TRSF.Invoicing.Demo.Web;
using TRSF.Invoicing.Interfaces;
using TRSF.Invoicing.PDF;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DemoSessionStore>();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var manifestJsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

// --- Paso 1: generar catalogo curado -------------------------------------------------

app.MapPost("/api/catalog/generate", (GenerateCatalogRequest req, DemoSessionStore store) =>
{
    var masterDbPath = Path.Combine(AppContext.BaseDirectory, "Data", "catalogs.sqlite");
    if (!File.Exists(masterDbPath))
        return Results.Problem("No se encontro el catalogo maestro catalogs.sqlite.", statusCode: 500);

    CurationManifest manifest;
    if (string.IsNullOrEmpty(req.Industria) || req.Industria == "completo")
    {
        manifest = new CurationManifest();
    }
    else
    {
        var manifestPath = Path.Combine(AppContext.BaseDirectory, "Samples", $"{req.Industria}.json");
        if (!File.Exists(manifestPath))
            return Results.NotFound($"No existe el manifiesto de industria '{req.Industria}'.");
        manifest = JsonSerializer.Deserialize<CurationManifest>(File.ReadAllText(manifestPath), manifestJsonOptions)
            ?? new CurationManifest();
    }

    var outputDbPath = Path.Combine(Path.GetTempPath(), $"trsf-demo-{Guid.NewGuid():N}.sqlite");
    var summaries = CuratedCatalogGenerator.Generate(masterDbPath, manifest, outputDbPath);
    var validator = new SqliteCatalogValidator(outputDbPath);
    var session = store.Create(outputDbPath, validator);

    return Results.Ok(new
    {
        sessionId = session.Id,
        resumen = summaries.Select(s => new { catalogo = s.Table, conservadas = s.Kept, total = s.Total }),
    });
});

app.MapGet("/api/catalog/search", (string sessionId, string catalogo, string? prefix, DemoSessionStore store) =>
{
    var session = store.Get(sessionId);
    if (session is null)
        return Results.NotFound("Sesion no encontrada. Repita el paso 1.");

    if (!Enum.TryParse<CatalogoGrande>(catalogo, ignoreCase: true, out var catalogoEnum))
        return Results.BadRequest($"Catalogo desconocido: {catalogo}");

    var matches = CatalogBrowser.Buscar(session.CuratedDbPath, catalogoEnum, prefix ?? "");
    return Results.Ok(matches);
});

// --- Paso 2: emisor + certificado ------------------------------------------------------

app.MapPost("/api/emisor", async (HttpRequest request, DemoSessionStore store) =>
{
    var form = await request.ReadFormAsync();
    var sessionId = form["sessionId"].ToString();
    var session = store.Get(sessionId);
    if (session is null)
        return Results.NotFound("Sesion no encontrada. Repita el paso 1.");

    var rfc = form["rfc"].ToString();
    var nombre = form["nombre"].ToString();
    var regimenFiscal = form["regimenFiscal"].ToString();
    var lugarExpedicion = form["lugarExpedicion"].ToString();
    var useTestCsd = form["useTestCsd"] == "true";

    byte[] cerBytes;
    byte[] keyBytes;
    string password;

    if (useTestCsd)
    {
        var testCsdDir = Path.Combine(AppContext.BaseDirectory, "TestCsd");
        cerBytes = await File.ReadAllBytesAsync(Path.Combine(testCsdDir, "CSD_Pruebas_CFDI_LAN7008173R5.cer"));
        keyBytes = await File.ReadAllBytesAsync(Path.Combine(testCsdDir, "CSD_Pruebas_CFDI_LAN7008173R5.key"));
        password = "12345678a";
    }
    else
    {
        var cerFile = form.Files["cer"];
        var keyFile = form.Files["key"];
        if (cerFile is null || keyFile is null)
            return Results.BadRequest("Suba el archivo .cer y .key, o marque 'usar CSD de pruebas'.");

        using var cerStream = new MemoryStream();
        await cerFile.CopyToAsync(cerStream);
        cerBytes = cerStream.ToArray();

        using var keyStream = new MemoryStream();
        await keyFile.CopyToAsync(keyStream);
        keyBytes = keyStream.ToArray();

        password = form["pwd"].ToString();
    }

    DemoCertificate certificate;
    try
    {
        certificate = DemoCertificate.FromBytes(cerBytes, keyBytes, password);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"No se pudo leer el certificado: {ex.Message}");
    }

    session.CertificatesRepository = new DemoCertificateRepository(certificate);
    session.NoCertificado = certificate.NoCertificate;
    session.Emisor = new Emisor { RFC = rfc, Nombre = nombre, RegimenFiscal = regimenFiscal };

    return Results.Ok(new
    {
        noCertificado = certificate.NoCertificate,
        validoDesde = certificate.ValidFrom,
        validoHasta = certificate.ValidUntil,
        lugarExpedicion,
    });
});

// --- Paso 3: receptor via Constancia de Situacion Fiscal -------------------------------

app.MapPost("/api/receptor/csf", async (HttpRequest request, DemoSessionStore store) =>
{
    var form = await request.ReadFormAsync();
    var sessionId = form["sessionId"].ToString();
    var session = store.Get(sessionId);
    if (session is null)
        return Results.NotFound("Sesion no encontrada. Repita el paso 1.");

    var usoCFDI = form["usoCFDI"].ToString();
    var pdfFile = form.Files["pdf"];
    if (pdfFile is null)
        return Results.BadRequest("Suba el PDF de la Constancia de Situacion Fiscal.");

    ConstanciaSituacionFiscal constancia;
    try
    {
        using var pdfStream = pdfFile.OpenReadStream();
        constancia = PdfConstanciaFiscalReader.Leer(pdfStream);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"No se pudo leer el PDF: {ex.Message}");
    }

    var receptor = constancia.ToReceptor(usoCFDI);

    return Results.Ok(new
    {
        rfc = receptor.RFC,
        nombre = receptor.Nombre,
        domicilioFiscalReceptor = receptor.DomicilioFiscalReceptor,
        regimenFiscalReceptor = receptor.RegimenFiscalReceptor,
        usoCFDI = receptor.UsoCFDI,
        regimenesEncontrados = constancia.Regimenes.Select(r => new { r.Descripcion, r.Codigo, r.Vigente }),
        advertencia = receptor.RegimenFiscalReceptor is null
            ? "No se reconocio ningun regimen fiscal en el PDF - revise/complete el campo antes de sellar."
            : null,
    });
});

// --- Paso 4: generar factura sellada ---------------------------------------------------

app.MapPost("/api/invoice/seal", (SealInvoiceRequest req, DemoSessionStore store) =>
{
    var session = store.Get(req.SessionId);
    if (session is null)
        return Results.NotFound("Sesion no encontrada. Repita el paso 1.");
    if (session.Emisor is null || session.CertificatesRepository is null)
        return Results.BadRequest("Complete el paso 2 (emisor y certificado) antes de sellar.");

    if (!session.CatalogValidator.Existe(CatalogoGrande.ClaveProdServ, req.Concepto.ClaveProdServ))
        return Results.BadRequest($"'{req.Concepto.ClaveProdServ}' no es una clave de producto/servicio valida en el catalogo curado de esta sesion.");
    if (!session.CatalogValidator.Existe(CatalogoGrande.ClaveUnidad, req.Concepto.ClaveUnidad))
        return Results.BadRequest($"'{req.Concepto.ClaveUnidad}' no es una clave de unidad valida en el catalogo curado de esta sesion.");

    var importe = Math.Round(req.Concepto.Cantidad * req.Concepto.ValorUnitario, 2);
    var ivaImporte = Math.Round(importe * 0.16m, 2);

    var concepto = new Concepto
    {
        ClaveProductoServicio = req.Concepto.ClaveProdServ,
        ClaveUnidad = req.Concepto.ClaveUnidad,
        Descripcion = req.Concepto.Descripcion,
        Cantidad = req.Concepto.Cantidad,
        ValorUnitario = req.Concepto.ValorUnitario,
        Importe = importe,
        ObjetoImp = "02",
        ConceptosImpuestos = new List<ConceptoImpuestos>
        {
            new()
            {
                BaseImpuesto = importe,
                Importe = ivaImporte,
                Impuesto = "IVA",
                TipoFactor = "Tasa",
                TasaOCuota = "0.160000",
                RetencionOTraslado = "Traslado",
            },
        },
    };

    var comprobante = new TRSF.Invoicing.BindingModels.Comprobante
    {
        Version = "4.0",
        Emisor = session.Emisor,
        Receptor = new TRSF.Invoicing.BindingModels.Receptor
        {
            RFC = req.Receptor.Rfc,
            Nombre = req.Receptor.Nombre,
            DomicilioFiscalReceptor = req.Receptor.DomicilioFiscalReceptor,
            RegimenFiscalReceptor = req.Receptor.RegimenFiscalReceptor,
            UsoCFDI = req.Receptor.UsoCFDI,
        },
        Conceptos = new List<TRSF.Invoicing.BindingModels.Concepto> { concepto },
        LugarExpedicion = req.LugarExpedicion,
        FormaPago = "01",
        MetodoPago = "PUE",
        TipoComprobante = "I",
        Moneda = "MXN",
        SubTotal = importe,
        Total = importe + ivaImporte,
        Fecha = DateTime.Now,
        noCertificado = session.NoCertificado,
    };

    try
    {
        var cfdi = new CFDIv40(session.CertificatesRepository, DemoSatProvider.Instance);
        var xml = cfdi.CreateCFDI(comprobante, Timbrado: false);
        session.SelloXml = xml;
        return Results.Ok(new { xml });
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"No se pudo sellar el comprobante: {ex.Message}");
    }
});

app.MapGet("/api/invoice/download", (string sessionId, DemoSessionStore store) =>
{
    var session = store.Get(sessionId);
    if (session?.SelloXml is null)
        return Results.NotFound("No hay una factura sellada para esta sesion todavia.");

    var bytes = System.Text.Encoding.UTF8.GetBytes(session.SelloXml);
    return Results.File(bytes, "application/xml", "factura-demo.xml");
});

app.MapGet("/api/invoice/pdf", (string sessionId, DemoSessionStore store) =>
{
    var session = store.Get(sessionId);
    if (session?.SelloXml is null)
        return Results.NotFound("No hay una factura sellada para esta sesion todavia.");

    var cfdi = new CFDIv40(session.CertificatesRepository!, DemoSatProvider.Instance);
    var comprobante = cfdi.DeserializeXML(session.SelloXml);

    using var stream = new MemoryStream();
    new ItextPDFProvider().EscribirCFDIPDF(comprobante, stream, imageQR: null);

    return Results.File(stream.ToArray(), "application/pdf", "factura-demo.pdf");
});

app.Run();

record GenerateCatalogRequest(string? Industria);

record SealInvoiceRequest(string SessionId, string LugarExpedicion, ReceptorInput Receptor, ConceptoInput Concepto);

record ReceptorInput(string Rfc, string Nombre, string DomicilioFiscalReceptor, string RegimenFiscalReceptor, string UsoCFDI);

record ConceptoInput(string ClaveProdServ, string ClaveUnidad, string Descripcion, decimal Cantidad, decimal ValorUnitario);
