using System;
using System.Collections.Generic;

namespace TRSF.Invoicing.ConstanciaFiscal
{
    /// <summary>
    /// La Constancia de Situacion Fiscal lista regimenes por su descripcion en texto
    /// (p.ej. "Regimen de Sueldos y Salarios e Ingresos Asimilados a Salarios"), no por
    /// su codigo de catalogo (c_RegimenFiscal, p.ej. "605"). Esta tabla traduce entre
    /// ambos - son los mismos 23 codigos de <see cref="cfdi40.c_RegimenFiscal"/>, con su
    /// texto oficial del catalogo del SAT.
    /// </summary>
    public static class CatalogoRegimenFiscalTexto
    {
        private static readonly Dictionary<string, string> DescripcionACodigo = new(StringComparer.OrdinalIgnoreCase)
        {
            ["General de Ley Personas Morales"] = "601",
            ["Personas Morales con Fines no Lucrativos"] = "603",
            ["Sueldos y Salarios e Ingresos Asimilados a Salarios"] = "605",
            ["Arrendamiento"] = "606",
            ["Regimen de Enajenacion o Adquisicion de Bienes"] = "607",
            ["Demas ingresos"] = "608",
            ["Consolidacion"] = "609",
            ["Residentes en el Extranjero sin Establecimiento Permanente en Mexico"] = "610",
            ["Ingresos por Dividendos (socios y accionistas)"] = "611",
            ["Personas Fisicas con Actividades Empresariales y Profesionales"] = "612",
            ["Ingresos por intereses"] = "614",
            ["Regimen de los ingresos por obtencion de premios"] = "615",
            ["Sin obligaciones fiscales"] = "616",
            ["Sociedades Cooperativas de Produccion que optan por diferir sus ingresos"] = "620",
            ["Incorporacion Fiscal"] = "621",
            ["Actividades Agricolas, Ganaderas, Silvicolas y Pesqueras"] = "622",
            ["Opcional para Grupos de Sociedades"] = "623",
            ["Coordinados"] = "624",
            ["Regimen de las Actividades Empresariales con ingresos a traves de Plataformas Tecnologicas"] = "625",
            ["Regimen Simplificado de Confianza"] = "626",
            ["Hidrocarburos"] = "628",
            ["De los Regimenes Fiscales Preferentes y de las Empresas Multinacionales"] = "629",
            ["Enajenacion de acciones en bolsa de valores"] = "630",
        };

        /// <summary>
        /// Busca el codigo de catalogo para una descripcion de regimen tal como aparece
        /// en una Constancia de Situacion Fiscal (con o sin el prefijo "Regimen de").
        /// Devuelve null si no se reconoce - no lanza excepcion, ya que el texto exacto
        /// puede variar entre versiones del formato del SAT.
        /// </summary>
        public static string BuscarCodigo(string descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                return null;

            var normalizada = NormalizarPrefijo(descripcion);

            foreach (var kvp in DescripcionACodigo)
            {
                if (string.Equals(NormalizarPrefijo(kvp.Key), normalizada, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }

            // coincidencia parcial como respaldo (acentos/redaccion pueden variar)
            foreach (var kvp in DescripcionACodigo)
            {
                var claveNormalizada = NormalizarPrefijo(kvp.Key);
                if (normalizada.Contains(claveNormalizada, StringComparison.OrdinalIgnoreCase)
                    || claveNormalizada.Contains(normalizada, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        private static string NormalizarPrefijo(string texto)
        {
            texto = texto.Trim();
            if (texto.StartsWith("Regimen de ", StringComparison.OrdinalIgnoreCase))
                texto = texto.Substring("Regimen de ".Length);
            else if (texto.StartsWith("Regimen ", StringComparison.OrdinalIgnoreCase))
                texto = texto.Substring("Regimen ".Length);
            return texto.Trim();
        }
    }
}
