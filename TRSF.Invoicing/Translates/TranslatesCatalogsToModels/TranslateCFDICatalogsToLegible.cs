using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Translates
{
    public class TranslateCFDICatalogsToLegible
    {
        public static string TranslateUSOCFDIToLegible(cfdi33.c_UsoCFDI c_UsoCFDI)
        {
            switch (c_UsoCFDI)
            {
                case cfdi33.c_UsoCFDI.G01: { return "Adquisición de mercancias"; }
                case cfdi33.c_UsoCFDI.G02: { return "Devoluciones, descuentos o bonificaciones"; }
                case cfdi33.c_UsoCFDI.G03: { return "Gastos en general"; }
                default: { throw new NotSupportedException("Tipo de uso de cfdi no soportado"); }
            }
        }

        public static string TranslateFormaPagoToLegible(cfdi33.c_FormaPago c_FormaPago)
        {

            /* 01	Efectivo
                02	Cheque nominativo
                03	Transferencia electrónica de fondos
                04	Tarjeta de crédito
                05	Monedero electrónico
                06	Dinero electrónico
                08	Vales de despensa
                12	Dación en pago
                13	Pago por subrogación
                14	Pago por consignación
                15	Condonación
                17	Compensación
                23	Novación
                24	Confusión
                25	Remisión de deuda
                26	Prescripción o caducidad
                27	A satisfacción del acreedor
                28	Tarjeta de débito
                29	Tarjeta de servicios
                99	Por definir */
            switch (c_FormaPago)
            {
                case cfdi33.c_FormaPago.Item01: { return "Efectivo"; }
                case cfdi33.c_FormaPago.Item02: { return "Transferencia electrónica de fondos"; }
                case cfdi33.c_FormaPago.Item03: { return "Tarjeta de crédito"; }
                case cfdi33.c_FormaPago.Item04: { return "Tarjeta de crédito"; }
                default: { throw new NotSupportedException("Tipo de forma de pago no soportado"); }
            }
        }

        public static string TranslateRegimenesFiscalesToLegible(cfdi33.c_RegimenFiscal c_RegimenFiscal)
        {
            switch (c_RegimenFiscal)
            {
                case cfdi33.c_RegimenFiscal.Item601: { return "General de Ley Personas Morales"; }
                case cfdi33.c_RegimenFiscal.Item603: { return "Personas Morales con Fines no Lucrativos"; }
                case cfdi33.c_RegimenFiscal.Item605: { return "General de Ley Personas Morales"; }
                case cfdi33.c_RegimenFiscal.Item606: { return "General de Ley Personas Morales"; }
                case cfdi33.c_RegimenFiscal.Item607: { return "Régimen de Enajenación o Adquisición de Bienes"; }
                case cfdi33.c_RegimenFiscal.Item608: { return "General de Ley Personas Morales"; }
                case cfdi33.c_RegimenFiscal.Item609: { return "Consolidación"; }
                case cfdi33.c_RegimenFiscal.Item610: { return "Residentes en el Extranjero sin Establecimiento Permanente en México"; }
                case cfdi33.c_RegimenFiscal.Item620: { return "Sociedades Cooperativas de Producción que optan por diferir sus ingresos"; }
                case cfdi33.c_RegimenFiscal.Item622: { return "622 Actividades Agrícolas, Ganaderas, Silvícolas y Pesqueras"; }
                case cfdi33.c_RegimenFiscal.Item623: { return "Opcional para Grupos de Sociedades"; }
                case cfdi33.c_RegimenFiscal.Item624: { return "Coordinados"; }
                case cfdi33.c_RegimenFiscal.Item628: { return "Hidrocarburos"; }
                case cfdi33.c_RegimenFiscal.Item630: { return "Enajenación de acciones en bolsa de valores"; }
                default: { throw new NotSupportedException("Régimen fiscal no soportado"); }

            }
        }

        public static string TranslateMetodoPagoToLegible(cfdi33.c_MetodoPago c_MetodoPago)
        {
            if (c_MetodoPago == cfdi33.c_MetodoPago.PPD)
            {
                return "Pago en parcialidades o diferido";
            }
            else if (c_MetodoPago == cfdi33.c_MetodoPago.PUE)
            {
                return "Pago en una sola exhibición";
            }
            else return String.Empty;
        }

        public static string TranslateMonedasToLegible(cfdi33.c_Moneda c_Moneda)
        {
            if (c_Moneda == cfdi33.c_Moneda.MXN)
            {
                return "Pesos";
            }
            else if (c_Moneda == cfdi33.c_Moneda.USD)
            {
                return "Dólares";
            }
            else if (c_Moneda == cfdi33.c_Moneda.EUR)
            {
                return "Euros";
            }
            else if (c_Moneda == cfdi33.c_Moneda.BRL)
            {
                return "Libras Esterinas";
            }
            else
                return c_Moneda.ToString();


        }

        public static string TranslateTipoComproabanteToLegible (cfdi33.c_TipoDeComprobante c_TipoDeComprobante)
        {
            switch(c_TipoDeComprobante)
            {
                case cfdi33.c_TipoDeComprobante.I: { return "Ingreso"; }
                case cfdi33.c_TipoDeComprobante.E: { return "Egreso"; }
                case cfdi33.c_TipoDeComprobante.T: { return "Translado"; }
                case cfdi33.c_TipoDeComprobante.P: { return "Pago"; }
                default: { return String.Empty; }
            }
        }
    }
}
