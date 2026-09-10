using System;
using System.Security.Cryptography;

namespace TRSF.Invoicing.Utils
{
    public class ValidateCertificate
    {
        public static bool Validate(string PassKey, string KeyFile)
        {
            try
            {
                byte[] privateKey = Convert.FromBase64String(KeyFile);
                using RSA rsa = RSA.Create();
                rsa.ImportEncryptedPkcs8PrivateKey(PassKey, privateKey, out _);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
