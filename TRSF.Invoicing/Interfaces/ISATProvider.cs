using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Interfaces
{
    public interface ISATProvider
    {
        String Timbrar(String RFC, String Comprobante, long transactionId);

        byte[] ObtenerQR(string RFC,  String UUID, long transactionId);
    }
}
