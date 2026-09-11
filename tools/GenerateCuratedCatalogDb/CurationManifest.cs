namespace GenerateCuratedCatalogDb
{
    /// <summary>
    /// Una sección de catalogo omitida significa "no curar este catalogo - copiarlo
    /// completo" (ver README.md). <see cref="Segmentos"/> solo aplica a ClaveProdServ:
    /// coincide por prefijo de 2 digitos contra el codigo de 8 digitos tipo UNSPSC que
    /// usa SAT (confirmado contra el catalogo real: 52,747 codigos, todos de 8 digitos,
    /// 58 prefijos de 2 digitos distintos - ver MIGRATION.md).
    /// </summary>
    public class CatalogSection
    {
        public string[]? Segmentos { get; set; }
        public string[]? Codigos { get; set; }
        public string[]? CodigosAdicionales { get; set; }
    }

    public class CurationManifest
    {
        public CatalogSection? ClaveProdServ { get; set; }
        public CatalogSection? ClaveUnidad { get; set; }
        public CatalogSection? CodigoPostal { get; set; }
        public CatalogSection? Colonia { get; set; }
        public CatalogSection? Municipio { get; set; }
    }
}
