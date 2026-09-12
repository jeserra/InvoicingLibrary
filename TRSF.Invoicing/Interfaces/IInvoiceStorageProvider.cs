using System;

namespace TRSF.Invoicing.Interfaces
{
    public interface IInvoiceStorageProvider
    {
        /// <summary>Guarda un documento de la factura y devuelve su ubicacion (URI o ruta, segun el proveedor).</summary>
        string Save(string rfcEmisor, DateTime fecha, string fileName, byte[] contenido, string contentType);
    }
}
