using System.ComponentModel;
using System.Text;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TRSF.Invoicing.BindingModels;
using TRSF.Invoicing.CFDI;
using TRSF.Invoicing.Catalogs.Sqlite;
using TRSF.Invoicing.ConstanciaFiscal;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Mcp.Tools;

/// <summary>
/// Adaptadores MCP sobre el mismo flujo que TRSF.Invoicing.Demo.Web/Program.cs implementa como
/// endpoints HTTP: leer un CSF, buscar claves de catalogo, y sellar un CFDI 4.0 (sin timbrar -
/// CreateCFDI con Timbrado:false, igual que el demo). No hay logica de facturacion nueva aqui.
/// </summary>
[McpServerToolType]
public static class InvoiceTools
{
    private static readonly string CatalogDbPath = Path.Combine(AppContext.BaseDirectory, "Data", "catalogs.sqlite");

    [McpServerTool(Name = "read_constancia_fiscal")]
    [Description("Lee una Constancia de Situacion Fiscal (CSF) del SAT en PDF y arma los datos de Receptor (RFC, nombre, domicilio fiscal, regimen fiscal) para usarlos en create_invoice.")]
    public static object ReadConstanciaFiscal(
        [Description("Ruta local al PDF de la Constancia de Situacion Fiscal.")] string pdfPath,
        [Description("Codigo de uso del CFDI a asignar (catalogo c_UsoCFDI), p.ej. 'G03'.")] string usoCFDI,
        [Description("Codigo de regimen fiscal a usar si el contribuyente tiene mas de uno activo. Si se omite, se usa el mas reciente sin fecha de baja.")] string? regimenFiscalCodigo = null)
    {
        if (!File.Exists(pdfPath))
            throw new McpException($"No se encontro el archivo: {pdfPath}");

        ConstanciaSituacionFiscal constancia;
        try
        {
            constancia = PdfConstanciaFiscalReader.Leer(pdfPath);
        }
        catch (Exception ex)
        {
            throw new McpException($"No se pudo leer el PDF: {ex.Message}");
        }

        var receptor = regimenFiscalCodigo is null
            ? constancia.ToReceptor(usoCFDI)
            : constancia.ToReceptor(usoCFDI, regimenFiscalCodigo);

        return new
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
        };
    }

    [McpServerTool(Name = "search_catalog")]
    [Description("Busca codigos validos en un catalogo grande del CFDI (ClaveProdServ, ClaveUnidad, CodigoPostal, Colonia, Municipio) por prefijo de codigo o texto en la descripcion.")]
    public static List<CatalogMatch> SearchCatalog(
        [Description("Catalogo a buscar: ClaveProdServ, ClaveUnidad, CodigoPostal, Colonia o Municipio.")] string catalogo,
        [Description("Texto a buscar.")] string query)
    {
        if (!Enum.TryParse<CatalogoGrande>(catalogo, ignoreCase: true, out var catalogoEnum))
            throw new McpException($"Catalogo desconocido: '{catalogo}'. Valores validos: {string.Join(", ", Enum.GetNames<CatalogoGrande>())}");

        if (!File.Exists(CatalogDbPath))
            throw new McpException("No se encontro el catalogo maestro catalogs.sqlite.");

        return CatalogLookup.Buscar(CatalogDbPath, catalogoEnum, query ?? "");
    }

    [McpServerTool(Name = "create_invoice")]
    [Description("Sella un CFDI 4.0 (Comprobante Fiscal Digital) mexicano con el CSD del emisor y devuelve el XML sellado. No timbra ante el SAT/PAC - el XML resultante todavia necesita timbrarse antes de ser fiscalmente valido para enviarse a un cliente. Impuesto asumido en cada concepto: IVA 16% trasladado. El XML se guarda automaticamente: en Azure Blob Storage si TRSF_STORAGE_CONTAINER_URI esta configurado, o en disco local (TRSF_STORAGE_LOCAL_DIR, o %LocalAppData%/TRSF.Invoicing/Invoices por default) si no. La respuesta incluye 'savedTo' con la ubicacion resultante.")]
    public static object CreateInvoice(
        [Description("Certificado de Sello Digital del emisor.")] CertificadoInput cer,
        [Description("Datos del emisor.")] EmisorInput emisor,
        [Description("Datos del receptor. Puede obtenerse con read_constancia_fiscal.")] ReceptorInput receptor,
        [Description("Conceptos (lineas) de la factura. Al menos uno.")] List<ConceptoInput> conceptos,
        [Description("Codigo postal del domicilio del emisor (lugar de expedicion).")] string lugarExpedicion,
        [Description("Catalogo c_FormaPago. Default '01' (Efectivo).")] string formaPago = "01",
        [Description("'PUE' (pago en una sola exhibicion) o 'PPD' (pago en parcialidades o diferido).")] string metodoPago = "PUE",
        [Description("Catalogo c_TipoDeComprobante. Default 'I' (Ingreso).")] string tipoComprobante = "I",
        [Description("Catalogo c_Moneda. Default 'MXN'.")] string moneda = "MXN",
        [Description("Si se da, ruta local donde escribir el XML sellado en vez de devolverlo inline.")] string? outputPath = null)
    {
        if (conceptos is null || conceptos.Count == 0)
            throw new McpException("Se requiere al menos un concepto.");

        var certificate = ResolveCertificate(cer);

        if (!File.Exists(CatalogDbPath))
            throw new McpException("No se encontro el catalogo maestro catalogs.sqlite.");

        var conceptosModel = new List<Concepto>();
        decimal subtotal = 0m, totalImpuestos = 0m;

        using (var validator = new SqliteCatalogValidator(CatalogDbPath))
        {
            foreach (var c in conceptos)
            {
                if (!validator.Existe(CatalogoGrande.ClaveProdServ, c.claveProdServ))
                    throw new McpException($"'{c.claveProdServ}' no es una clave de producto/servicio valida.");
                if (!validator.Existe(CatalogoGrande.ClaveUnidad, c.claveUnidad))
                    throw new McpException($"'{c.claveUnidad}' no es una clave de unidad valida.");

                var importe = Math.Round(c.cantidad * c.valorUnitario, 2);
                var ivaImporte = Math.Round(importe * 0.16m, 2);
                subtotal += importe;
                totalImpuestos += ivaImporte;

                conceptosModel.Add(new Concepto
                {
                    ClaveProductoServicio = c.claveProdServ,
                    ClaveUnidad = c.claveUnidad,
                    Descripcion = c.descripcion,
                    Cantidad = c.cantidad,
                    ValorUnitario = c.valorUnitario,
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
                });
            }
        }

        var comprobante = new TRSF.Invoicing.BindingModels.Comprobante
        {
            Version = "4.0",
            Emisor = new Emisor { RFC = emisor.rfc, Nombre = emisor.nombre, RegimenFiscal = emisor.regimenFiscal },
            Receptor = new TRSF.Invoicing.BindingModels.Receptor
            {
                RFC = receptor.rfc,
                Nombre = receptor.nombre,
                DomicilioFiscalReceptor = receptor.domicilioFiscalReceptor,
                RegimenFiscalReceptor = receptor.regimenFiscalReceptor,
                UsoCFDI = receptor.usoCFDI,
            },
            Conceptos = conceptosModel,
            LugarExpedicion = lugarExpedicion,
            FormaPago = formaPago,
            MetodoPago = metodoPago,
            TipoComprobante = tipoComprobante,
            Moneda = moneda,
            SubTotal = subtotal,
            Total = subtotal + totalImpuestos,
            Fecha = DateTime.Now,
            noCertificado = certificate.NoCertificate,
        };

        string xml;
        try
        {
            var certRepo = new FileCertificateRepository(certificate);
            var cfdi = new CFDIv40(certRepo, NotConnectedSatProvider.Instance);
            xml = cfdi.CreateCFDI(comprobante, Timbrado: false);
        }
        catch (Exception ex)
        {
            throw new McpException($"No se pudo sellar el comprobante: {ex.Message}");
        }

        string savedTo;
        try
        {
            var fileName = $"{comprobante.Fecha:yyyyMMdd-HHmmss}-{receptor.rfc}.xml";
            IInvoiceStorageProvider storage = AzureBlobInvoiceStorageProvider.IsConfigured
                ? new AzureBlobInvoiceStorageProvider()
                : new LocalFileInvoiceStorageProvider();
            savedTo = storage.Save(emisor.rfc, comprobante.Fecha, fileName, Encoding.UTF8.GetBytes(xml), "application/xml");
        }
        catch (Exception ex)
        {
            throw new McpException($"El comprobante se sello correctamente pero no se pudo guardar en storage: {ex.Message}");
        }

        if (outputPath is not null)
        {
            File.WriteAllText(outputPath, xml);
            return new { outputPath, savedTo, subtotal, total = comprobante.Total };
        }

        return new { xml, savedTo, subtotal, total = comprobante.Total };
    }

    [McpServerTool(Name = "register_certificate")]
    [Description("Guarda un CSD (.cer + .key + contrasena) en Azure Key Vault bajo el RFC del emisor, para que create_invoice pueda referenciarlo despues con cer.rfc en vez de rutas de archivo. Requiere TRSF_KEYVAULT_URI configurado en el servidor.")]
    public static object RegisterCertificate(
        [Description("RFC del emisor bajo el cual guardar el certificado.")] string rfc,
        [Description("Ruta local al archivo .cer del CSD.")] string cerPath,
        [Description("Ruta local al archivo .key del CSD.")] string keyPath,
        [Description("Contrasena de la llave privada. Omita y use passwordEnvVar si prefiere no incluirla en la conversacion.")] string? password = null,
        [Description("Nombre de una variable de entorno de la que leer la contrasena, en vez de 'password'.")] string? passwordEnvVar = null)
    {
        var resolvedPassword = password
            ?? (passwordEnvVar is not null ? Environment.GetEnvironmentVariable(passwordEnvVar) : null);
        if (string.IsNullOrEmpty(resolvedPassword))
            throw new McpException("Proporcione 'password' o 'passwordEnvVar' para la llave del CSD.");

        if (!File.Exists(cerPath))
            throw new McpException($"No se encontro el archivo .cer: {cerPath}");
        if (!File.Exists(keyPath))
            throw new McpException($"No se encontro el archivo .key: {keyPath}");

        FileCertificate certificate;
        try
        {
            certificate = FileCertificate.Load(cerPath, keyPath, resolvedPassword);
        }
        catch (Exception ex)
        {
            throw new McpException($"No se pudo leer el certificado: {ex.Message}");
        }

        try
        {
            AzureKeyVaultCertificateStore.Save(rfc, certificate);
        }
        catch (Exception ex)
        {
            throw new McpException($"No se pudo guardar el certificado en el vault: {ex.Message}");
        }

        return new { rfc, noCertificado = certificate.NoCertificate, validoDesde = certificate.ValidFrom, validoHasta = certificate.ValidUntil };
    }

    private static FileCertificate ResolveCertificate(CertificadoInput cer)
    {
        var hasRfc = !string.IsNullOrWhiteSpace(cer.rfc);
        var hasPaths = !string.IsNullOrWhiteSpace(cer.cerPath) || !string.IsNullOrWhiteSpace(cer.keyPath);

        if (hasRfc && hasPaths)
            throw new McpException("Proporcione 'cer.rfc' o 'cer.cerPath'/'cer.keyPath', no ambos.");

        if (hasRfc)
        {
            try
            {
                return AzureKeyVaultCertificateStore.Load(cer.rfc!);
            }
            catch (Exception ex)
            {
                throw new McpException($"No se pudo leer el certificado del vault: {ex.Message}");
            }
        }

        if (string.IsNullOrWhiteSpace(cer.cerPath) || string.IsNullOrWhiteSpace(cer.keyPath))
            throw new McpException("Proporcione 'cer.rfc', o ambos 'cer.cerPath' y 'cer.keyPath'.");

        var password = cer.password
            ?? (cer.passwordEnvVar is not null ? Environment.GetEnvironmentVariable(cer.passwordEnvVar) : null);
        if (string.IsNullOrEmpty(password))
            throw new McpException("Proporcione 'cer.password' o 'cer.passwordEnvVar' para la llave del CSD.");

        if (!File.Exists(cer.cerPath))
            throw new McpException($"No se encontro el archivo .cer: {cer.cerPath}");
        if (!File.Exists(cer.keyPath))
            throw new McpException($"No se encontro el archivo .key: {cer.keyPath}");

        try
        {
            return FileCertificate.Load(cer.cerPath, cer.keyPath, password);
        }
        catch (Exception ex)
        {
            throw new McpException($"No se pudo leer el certificado: {ex.Message}");
        }
    }
}
