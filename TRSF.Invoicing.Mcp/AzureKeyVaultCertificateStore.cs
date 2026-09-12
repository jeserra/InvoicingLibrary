using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace TRSF.Invoicing.Mcp;

/// <summary>
/// Almacena/recupera el CSD completo (cer+key+password) de un RFC en Azure Key Vault, como
/// alternativa a pasar rutas de archivo locales en cada llamada. No implementa
/// ICertificatesRepository directamente - esa interfaz esta pensada para un servicio de
/// lookup multi-certificado (GetCertificate(noCertificado) / GetCertificate(accountId, rfc)),
/// mientras que aqui solo necesitamos "resolver un certificado para esta llamada" y luego
/// envolverlo en FileCertificateRepository igual que en el flujo de archivo local.
/// </summary>
public static class AzureKeyVaultCertificateStore
{
    private static SecretClient CreateClient()
    {
        var vaultUri = Environment.GetEnvironmentVariable("TRSF_KEYVAULT_URI");
        if (string.IsNullOrWhiteSpace(vaultUri))
            throw new InvalidOperationException(
                "TRSF_KEYVAULT_URI no esta configurado. Defina esa variable de entorno con la URL del " +
                "Key Vault, o use cer.cerPath/cer.keyPath en vez de cer.rfc.");

        return new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
    }

    public static void Save(string rfc, FileCertificate certificate)
    {
        var client = CreateClient();
        client.SetSecret(rfc, certificate.ToVaultJson());
    }

    public static FileCertificate Load(string rfc)
    {
        var client = CreateClient();
        KeyVaultSecret secret;
        try
        {
            secret = client.GetSecret(rfc);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException(
                $"No hay un certificado guardado en el vault para el RFC '{rfc}'. Use register_certificate primero.");
        }

        return FileCertificate.FromVaultJson(secret.Value);
    }
}
