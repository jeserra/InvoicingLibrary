using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Mcp;

/// <summary>
/// ICertificate loaded from .cer/.key files on disk, resolved fresh per tool call - never
/// persisted. Same CerFile/KeyFile shape as TRSF.Invoicing.Demo.Web.DemoCertificate (base64
/// of the full .cer, as cfdi:Comprobante/@Certificado expects), just sourced from file paths
/// instead of an upload stream.
/// </summary>
public class FileCertificate : ICertificate
{
    public string CerFile { get; set; } = "";
    public string KeyFile { get; set; } = "";
    public string Pwd { get; set; } = "";
    public string NoCertificate { get; set; } = "";
    public int idCertificate { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }

    public static FileCertificate Load(string cerPath, string keyPath, string password)
    {
        var cerBytes = File.ReadAllBytes(cerPath);
        var keyBytes = File.ReadAllBytes(keyPath);

        var x509 = X509CertificateLoader.LoadCertificate(cerBytes);
        byte[] serialNumber = x509.GetSerialNumber();
        Array.Reverse(serialNumber);

        return new FileCertificate
        {
            CerFile = Convert.ToBase64String(cerBytes),
            KeyFile = Convert.ToBase64String(keyBytes),
            Pwd = password,
            NoCertificate = Encoding.UTF8.GetString(serialNumber),
            ValidFrom = x509.NotBefore,
            ValidUntil = x509.NotAfter,
        };
    }

    /// <summary>Rehidrata un certificado guardado por AzureKeyVaultCertificateStore.Save.</summary>
    public static FileCertificate FromVaultJson(string json)
    {
        return JsonSerializer.Deserialize<FileCertificate>(json)
            ?? throw new InvalidOperationException("El secreto del vault no contiene un certificado valido.");
    }

    public string ToVaultJson() => JsonSerializer.Serialize(this);
}

/// <summary>Repositorio de un solo certificado, en memoria, para la duracion de una llamada.</summary>
public class FileCertificateRepository : ICertificatesRepository
{
    private readonly FileCertificate certificate;

    public FileCertificateRepository(FileCertificate certificate)
    {
        this.certificate = certificate;
    }

    public ICertificate GetCertificate(string noCertificado) => certificate;

    public ICertificate GetCertificate(string accountId, string rfc) => certificate;

    public bool SaveCertificate(int accountId, ICertificate certificate) =>
        throw new NotSupportedException("Este servidor MCP no persiste certificados.");
}

/// <summary>
/// Este servidor solo sella (CreateCFDI con Timbrado:false) - nunca contacta un PAC, asi que
/// esta implementacion nunca se invoca realmente. Existe solo porque el constructor de
/// CFDIv40 requiere un ISATProvider.
/// </summary>
public class NotConnectedSatProvider : ISATProvider
{
    public static readonly NotConnectedSatProvider Instance = new();

    public string Timbrar(string RFC, string Comprobante, long transactionId) =>
        throw new NotSupportedException("Este servidor solo sella (Timbrado: false); no esta conectado a ningun PAC.");

    public byte[] ObtenerQR(string RFC, string UUID, long transactionId) =>
        throw new NotSupportedException("Este servidor solo sella (Timbrado: false); no esta conectado a ningun PAC.");
}
