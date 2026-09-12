using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Mcp;

/// <summary>
/// Guarda documentos de factura en disco local, bajo {basePath}/{rfcEmisor}/{yyyy}/{MM}/{fileName}.
/// Es el respaldo sin dependencias cuando no hay un backend en la nube configurado (p.ej.
/// AzureBlobInvoiceStorageProvider) - funciona sin ninguna configuracion previa.
/// </summary>
public class LocalFileInvoiceStorageProvider : IInvoiceStorageProvider
{
    private static readonly string DefaultBasePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TRSF.Invoicing", "Invoices");

    public static string BasePath =>
        Environment.GetEnvironmentVariable("TRSF_STORAGE_LOCAL_DIR") is { Length: > 0 } dir ? dir : DefaultBasePath;

    public string Save(string rfcEmisor, DateTime fecha, string fileName, byte[] contenido, string contentType)
    {
        var directory = Path.Combine(BasePath, rfcEmisor, fecha.ToString("yyyy"), fecha.ToString("MM"));
        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, fileName);
        File.WriteAllBytes(filePath, contenido);

        return filePath;
    }
}
