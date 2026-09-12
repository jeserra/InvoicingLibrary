namespace TRSF.Invoicing.Interfaces
{
    /// <summary>
    /// Catalogos de CFDI 4.0 demasiado grandes/volatiles para representarse como enum en
    /// C# (ver Schemas40/xsd/README.md) - se validan en tiempo de ejecucion en su lugar.
    /// </summary>
    public enum CatalogoGrande
    {
        ClaveProdServ,
        ClaveUnidad,
        CodigoPostal,
        Colonia,
        Municipio
    }

    /// <summary>
    /// Valida codigos de los catalogos grandes del CFDI antes de sellar. No es requerido
    /// por CFDIBase/CFDIv33/CFDIv40 - es responsabilidad del caller invocarlo antes de
    /// construir el Comprobante (ver TRSF.Invoicing.Catalogs.Sqlite.SqliteCatalogValidator
    /// para una implementacion de referencia sobre SQLite).
    /// </summary>
    public interface ICatalogValidator
    {
        /// <summary>True si <paramref name="codigo"/> existe en <paramref name="catalogo"/>.</summary>
        bool Existe(CatalogoGrande catalogo, string codigo);

        /// <summary>
        /// Descripcion legible del codigo, si el catalogo la tiene disponible; cadena vacia
        /// o null en otro caso (catCFDI.xsd no trae descripciones - ver
        /// Schemas40/xsd/README.md y tools/ImportCatalogDescriptions).
        /// </summary>
        string ObtenerDescripcion(CatalogoGrande catalogo, string codigo);
    }
}
