using System;
using System.Linq;
using Xunit;
using TRSF.Invoicing.ConstanciaFiscal;

namespace TRSF.Invoicing.Test.ConstanciaFiscal
{
    // Todos los datos de identidad en este archivo son inventados para la prueba -
    // ninguno corresponde a una persona o RFC reales. El texto reproduce la estructura
    // exacta que produce iText7 al extraer una Constancia de Situacion Fiscal real
    // (confirmado contra un PDF de muestra durante el desarrollo de esta funcionalidad,
    // sin commitear ese PDF ni sus datos).
    public class ConstanciaFiscalTextParserTest
    {
        private const string TextoPersonaFisica = @"
Página  [1] de [3]
CÉDULA DE IDENTIFICACIÓN FISCAL
XAXX010101AA1
Registro Federal de Contribuyentes
MARIA FERNANDA LOPEZ
HERNANDEZ
Nombre, denominación o razón
social
idCIF: 99999999999
VALIDA TU INFORMACIÓN
FISCAL
CONSTANCIA DE SITUACIÓN FISCAL
Lugar y Fecha de Emisión
CIUDAD DE MEXICO A 01 DE ENERO DE
2026
XAXX010101AA1
Datos de Identificación del Contribuyente:
RFC: XAXX010101AA1
CURP: LOHM850101MDFPRR05
Nombre (s): MARIA FERNANDA
Primer Apellido: LOPEZ
Segundo Apellido: HERNANDEZ
Fecha inicio de operaciones: 01 DE ENERO DE 2020
Estatus en el padrón: ACTIVO
Fecha de último cambio de estado: 01 DE ENERO DE 2020
Nombre Comercial: LOPEZ MARIA
Datos del domicilio registrado
Código Postal:01000 Tipo de Vialidad: CALLE
Nombre de Vialidad: REFORMA Número Exterior: 100
Número Interior: Nombre de la Colonia: CENTRO
Página  [2] de [3]
Actividades Económicas:
Orden Actividad Económica Porcentaje Fecha Inicio Fecha Fin
1 Asalariado 60 01/01/2020
Regímenes:
Régimen Fecha Inicio Fecha Fin
Régimen de Sueldos y Salarios e Ingresos Asimilados a Salarios 01/01/2020
Régimen de las Personas Físicas con Actividades Empresariales y Profesionales 01/06/2022
Obligaciones:
Declaración anual de ISR. Personas Físicas. A más tardar el 30 de abril del ejercicio
siguiente.
01/06/2022
";

        private const string TextoPersonaMoral = @"
Datos de Identificación del Contribuyente:
RFC: ACM010101AA1
Denominación o Razón Social: ACME COMERCIALIZADORA SA DE CV
Fecha inicio de operaciones: 01 DE ENERO DE 2010
Datos del domicilio registrado
Código Postal:64000 Tipo de Vialidad: AVENIDA
Regímenes:
Régimen Fecha Inicio Fecha Fin
Régimen General de Ley Personas Morales 01/01/2010
Obligaciones:
";

        [Fact]
        public void Parse_extrae_RFC_CURP_y_nombre_de_persona_fisica()
        {
            var resultado = ConstanciaFiscalTextParser.Parse(TextoPersonaFisica);

            Assert.Equal("XAXX010101AA1", resultado.RFC);
            Assert.Equal("LOHM850101MDFPRR05", resultado.CURP);
            Assert.Equal("MARIA FERNANDA LOPEZ HERNANDEZ", resultado.NombreCompleto);
            Assert.Equal("01000", resultado.CodigoPostal);
        }

        [Fact]
        public void Parse_extrae_los_dos_regimenes_con_su_codigo()
        {
            var resultado = ConstanciaFiscalTextParser.Parse(TextoPersonaFisica);

            Assert.Equal(2, resultado.Regimenes.Count);

            var sueldos = resultado.Regimenes[0];
            Assert.Equal("605", sueldos.Codigo);
            Assert.Equal(new DateTime(2020, 1, 1), sueldos.FechaInicio);
            Assert.Null(sueldos.FechaFin);
            Assert.True(sueldos.Vigente);

            var actividadEmpresarial = resultado.Regimenes[1];
            Assert.Equal("612", actividadEmpresarial.Codigo);
            Assert.Equal(new DateTime(2022, 6, 1), actividadEmpresarial.FechaInicio);
        }

        [Fact]
        public void RegimenPrincipal_devuelve_el_vigente_mas_reciente()
        {
            var resultado = ConstanciaFiscalTextParser.Parse(TextoPersonaFisica);

            var principal = resultado.RegimenPrincipal();

            Assert.NotNull(principal);
            Assert.Equal("612", principal.Codigo);
        }

        [Fact]
        public void ToReceptor_arma_un_Receptor_listo_para_CFDI_40()
        {
            var resultado = ConstanciaFiscalTextParser.Parse(TextoPersonaFisica);

            var receptor = resultado.ToReceptor(usoCFDI: "G03");

            Assert.Equal("XAXX010101AA1", receptor.RFC);
            Assert.Equal("MARIA FERNANDA LOPEZ HERNANDEZ", receptor.Nombre);
            Assert.Equal("01000", receptor.DomicilioFiscalReceptor);
            Assert.Equal("612", receptor.RegimenFiscalReceptor);
            Assert.Equal("G03", receptor.UsoCFDI);
        }

        [Fact]
        public void Parse_reconoce_razon_social_de_persona_moral()
        {
            var resultado = ConstanciaFiscalTextParser.Parse(TextoPersonaMoral);

            Assert.Equal("ACM010101AA1", resultado.RFC);
            Assert.Equal("ACME COMERCIALIZADORA SA DE CV", resultado.NombreCompleto);
            Assert.Equal("64000", resultado.CodigoPostal);
            Assert.Single(resultado.Regimenes);
            Assert.Equal("601", resultado.Regimenes[0].Codigo);
        }

        [Fact]
        public void Parse_no_lanza_con_texto_vacio_y_deja_los_campos_en_null()
        {
            var resultado = ConstanciaFiscalTextParser.Parse("texto irreconocible sin ninguna etiqueta");

            Assert.Null(resultado.RFC);
            Assert.Null(resultado.CURP);
            Assert.Null(resultado.NombreCompleto);
            Assert.Null(resultado.CodigoPostal);
            Assert.Empty(resultado.Regimenes);
            Assert.Null(resultado.RegimenPrincipal());
        }
    }
}
