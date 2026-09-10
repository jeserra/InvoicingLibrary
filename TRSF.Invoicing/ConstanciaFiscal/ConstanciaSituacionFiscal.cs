using System;
using System.Collections.Generic;
using System.Linq;

namespace TRSF.Invoicing.ConstanciaFiscal
{
    /// <summary>
    /// Un regimen fiscal listado en la Constancia de Situacion Fiscal del contribuyente.
    /// Un contribuyente puede tener varios (p.ej. Sueldos y Salarios + Actividad
    /// Empresarial al mismo tiempo); el que no tiene FechaFin es el vigente.
    /// </summary>
    public class RegimenFiscalInfo
    {
        /// <summary>Descripcion tal como aparece en la constancia (p.ej. "Regimen de Sueldos y Salarios e Ingresos Asimilados a Salarios").</summary>
        public string Descripcion { get; set; }

        /// <summary>Codigo del catalogo c_RegimenFiscal (p.ej. "605"), o null si la descripcion no se reconocio.</summary>
        public string Codigo { get; set; }

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public bool Vigente => FechaFin == null;
    }

    /// <summary>
    /// Datos extraidos de una Constancia de Situacion Fiscal (CSF) emitida por el SAT -
    /// suficientes para poblar un <see cref="BindingModels.Receptor"/> o
    /// <see cref="BindingModels.Emisor"/> de CFDI 4.0, que requiere exactamente estos
    /// datos (RFC, regimen fiscal, domicilio fiscal) del receptor.
    ///
    /// La extraccion de texto de PDF es best-effort: el layout de la CSF puede variar
    /// entre persona fisica/moral y entre versiones del formato del SAT. Los campos que
    /// no se pudieron reconocer quedan en null en lugar de lanzar una excepcion - revise
    /// el resultado antes de usarlo para timbrar.
    /// </summary>
    public class ConstanciaSituacionFiscal
    {
        public string RFC { get; set; }
        public string CURP { get; set; }

        /// <summary>
        /// Para persona fisica: "Nombre(s) Primer Apellido Segundo Apellido" concatenado.
        /// Para persona moral: la denominacion o razon social.
        /// </summary>
        public string NombreCompleto { get; set; }

        /// <summary>Codigo postal del domicilio fiscal registrado - va directo a DomicilioFiscalReceptor.</summary>
        public string CodigoPostal { get; set; }

        public List<RegimenFiscalInfo> Regimenes { get; set; } = new List<RegimenFiscalInfo>();

        /// <summary>
        /// El regimen vigente mas reciente (sin FechaFin); si ninguno esta vigente,
        /// el de FechaInicio mas reciente. Null si no se reconocio ningun regimen.
        /// </summary>
        public RegimenFiscalInfo RegimenPrincipal()
        {
            return Regimenes.Where(r => r.Vigente).OrderByDescending(r => r.FechaInicio).FirstOrDefault()
                ?? Regimenes.OrderByDescending(r => r.FechaInicio).FirstOrDefault();
        }

        /// <summary>
        /// Construye un Receptor listo para usar en CFDI 4.0. Si el contribuyente tiene
        /// mas de un regimen vigente y el que corresponde a esta factura no es
        /// <see cref="RegimenPrincipal"/>, pase su codigo explicitamente en
        /// <paramref name="regimenFiscalCodigo"/>.
        /// </summary>
        public BindingModels.Receptor ToReceptor(string usoCFDI, string regimenFiscalCodigo = null)
        {
            return new BindingModels.Receptor
            {
                RFC = RFC,
                Nombre = NombreCompleto,
                DomicilioFiscalReceptor = CodigoPostal,
                RegimenFiscalReceptor = regimenFiscalCodigo ?? RegimenPrincipal()?.Codigo,
                UsoCFDI = usoCFDI,
            };
        }
    }
}
