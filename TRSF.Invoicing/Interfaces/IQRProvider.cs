using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Interfaces
{
    /// <summary>
    /// Genera el QR de verificacion pidiendolo a un servicio remoto (a diferencia de
    /// <see cref="ILocalQRProvider"/>, que lo arma localmente sin red). La unica
    /// implementacion en este repo es TRSF.Invoicing.QRProviders.EcodexQRProvider, que
    /// nada mas llama - se conserva como referencia de un cliente REST, no como parte de
    /// un flujo activo.
    /// </summary>
    public interface IQRProvider
    {
        Task<byte[]> GenerateQR(string rfc, string UUID);
    }
}
