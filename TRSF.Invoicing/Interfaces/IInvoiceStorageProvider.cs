using System;

namespace TRSF.Invoicing.Interfaces
{
    /// <summary>
    /// Persiste el XML (sellado o timbrado) despues de CreateCFDI. No es requerido por
    /// CFDIBase/CFDIv33/CFDIv40 - es responsabilidad del caller decidir donde y cuando
    /// guardar. Sin implementacion de referencia en este repo (los ejemplos de
    /// archivo-local/Azure Blob viven en el servidor MCP que consume esta libreria, no aqui);
    /// implemente contra el contrato de abajo.
    /// </summary>
    public interface IInvoiceStorageProvider
    {
        /// <summary>Guarda un documento de la factura y devuelve su ubicacion (URI o ruta, segun el proveedor).</summary>
        string Save(string rfcEmisor, DateTime fecha, string fileName, byte[] contenido, string contentType);
    }
}
