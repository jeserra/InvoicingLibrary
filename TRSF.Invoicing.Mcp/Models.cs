using System.ComponentModel;

namespace TRSF.Invoicing.Mcp;

[Description("Certificado de Sello Digital (CSD) del emisor. Use 'rfc' si ya se registro el CSD con register_certificate (Azure Key Vault), o cerPath/keyPath/password para leerlo de archivos locales. Proporcione exactamente una de las dos formas.")]
public class CertificadoInput
{
    [Description("RFC del emisor cuyo CSD ya fue guardado en el vault con register_certificate. Alternativa a cerPath/keyPath/password.")]
    public string? rfc { get; set; }

    [Description("Ruta local al archivo .cer del CSD.")]
    public string? cerPath { get; set; }

    [Description("Ruta local al archivo .key del CSD.")]
    public string? keyPath { get; set; }

    [Description("Contrasena de la llave privada. Omita y use passwordEnvVar si prefiere no incluirla en la conversacion.")]
    public string? password { get; set; }

    [Description("Nombre de una variable de entorno de la que leer la contrasena, en vez de 'password'.")]
    public string? passwordEnvVar { get; set; }
}

[Description("Datos del emisor de la factura.")]
public class EmisorInput
{
    [Description("RFC del emisor.")]
    public string rfc { get; set; } = "";

    [Description("Razon social o nombre del emisor, tal como esta en su Constancia de Situacion Fiscal.")]
    public string nombre { get; set; } = "";

    [Description("Codigo de regimen fiscal del emisor (catalogo c_RegimenFiscal).")]
    public string regimenFiscal { get; set; } = "";
}

[Description("Datos del receptor de la factura. Puede obtenerse con read_constancia_fiscal.")]
public class ReceptorInput
{
    [Description("RFC del receptor.")]
    public string rfc { get; set; } = "";

    [Description("Razon social o nombre del receptor.")]
    public string nombre { get; set; } = "";

    [Description("Codigo postal del domicilio fiscal del receptor.")]
    public string domicilioFiscalReceptor { get; set; } = "";

    [Description("Codigo de regimen fiscal del receptor (catalogo c_RegimenFiscal).")]
    public string regimenFiscalReceptor { get; set; } = "";

    [Description("Codigo de uso del CFDI (catalogo c_UsoCFDI), p.ej. 'G03'.")]
    public string usoCFDI { get; set; } = "";
}

[Description("Una linea/concepto de la factura. El impuesto se calcula asumiendo IVA 16% trasladado.")]
public class ConceptoInput
{
    [Description("Clave de producto/servicio (catalogo c_ClaveProdServ) - use search_catalog para encontrarla.")]
    public string claveProdServ { get; set; } = "";

    [Description("Clave de unidad (catalogo c_ClaveUnidad) - use search_catalog para encontrarla.")]
    public string claveUnidad { get; set; } = "";

    [Description("Descripcion del concepto.")]
    public string descripcion { get; set; } = "";

    [Description("Cantidad.")]
    public decimal cantidad { get; set; }

    [Description("Valor unitario, sin impuestos.")]
    public decimal valorUnitario { get; set; }
}

public record CatalogMatch(string Codigo, string? Descripcion);
