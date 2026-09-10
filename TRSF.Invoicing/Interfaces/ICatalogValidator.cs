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

    public interface ICatalogValidator
    {
        bool Existe(CatalogoGrande catalogo, string codigo);

        string ObtenerDescripcion(CatalogoGrande catalogo, string codigo);
    }
}
