using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Mcp;

/// <summary>
/// Guarda documentos de factura (XML sellado) en Azure Blob Storage, bajo
/// {rfcEmisor}/{yyyy}/{MM}/{fileName}. Misma autenticacion que AzureKeyVaultCertificateStore
/// (DefaultAzureCredential) y mismo patron de configuracion por variable de entorno.
/// </summary>
public class AzureBlobInvoiceStorageProvider : IInvoiceStorageProvider
{
    public static bool IsConfigured => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TRSF_STORAGE_CONTAINER_URI"));

    private static BlobContainerClient CreateClient()
    {
        var containerUri = Environment.GetEnvironmentVariable("TRSF_STORAGE_CONTAINER_URI");
        if (string.IsNullOrWhiteSpace(containerUri))
            throw new InvalidOperationException(
                "TRSF_STORAGE_CONTAINER_URI no esta configurado. Defina esa variable de entorno con la URL del contenedor de blobs.");

        return new BlobContainerClient(new Uri(containerUri), new DefaultAzureCredential());
    }

    public string Save(string rfcEmisor, DateTime fecha, string fileName, byte[] contenido, string contentType)
    {
        var container = CreateClient();
        var blobPath = $"{rfcEmisor}/{fecha:yyyy}/{fecha:MM}/{fileName}";
        var blob = container.GetBlobClient(blobPath);

        using var stream = new MemoryStream(contenido);
        blob.Upload(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
        });

        return blob.Uri.ToString();
    }
}
