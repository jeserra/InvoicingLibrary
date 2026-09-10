using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TRSF.Invoicing.ConstanciaFiscal
{
    /// <summary>
    /// Parsea el texto ya extraido de una Constancia de Situacion Fiscal (CSF) del SAT.
    /// Separado de la extraccion de PDF (<see cref="PdfConstanciaFiscalReader"/>) para
    /// poder probar la logica de parseo con texto plano, sin necesitar un PDF real.
    ///
    /// El layout de la CSF no es un formato estable/versionado publicamente - esto es
    /// best-effort sobre la plantilla actual del SAT. Los campos no reconocidos quedan
    /// en null en vez de lanzar, para que un cambio de formato degrade en vez de romper.
    /// </summary>
    public static class ConstanciaFiscalTextParser
    {
        public static ConstanciaSituacionFiscal Parse(string texto)
        {
            if (texto == null)
                throw new ArgumentNullException(nameof(texto));

            var normalizado = QuitarAcentos(texto);

            var resultado = new ConstanciaSituacionFiscal
            {
                RFC = Capturar(normalizado, @"(?m)^RFC:\s*([A-Z&NÑ]{3,4}\d{6}[A-Z0-9]{3})\s*$"),
                CURP = Capturar(normalizado, @"(?m)^CURP:\s*([A-Z0-9]{18})\s*$"),
                CodigoPostal = Capturar(normalizado, @"Codigo Postal:\s*(\d{5})"),
                NombreCompleto = ExtraerNombre(normalizado),
            };

            resultado.Regimenes = ExtraerRegimenes(normalizado);

            return resultado;
        }

        private static string ExtraerNombre(string texto)
        {
            // Persona moral: "Denominacion o Razon Social: ACME SA DE CV"
            var razonSocial = Capturar(texto, @"(?m)^Denominacion o Razon Social:\s*(.+?)\s*$");
            if (!string.IsNullOrWhiteSpace(razonSocial))
                return razonSocial;

            // Persona fisica: "Nombre (s): X" + "Primer Apellido: Y" + "Segundo Apellido: Z" (puede faltar)
            var nombres = Capturar(texto, @"(?m)^Nombre\s*\(s\):\s*(.+?)\s*$");
            var primerApellido = Capturar(texto, @"(?m)^Primer Apellido:\s*(.+?)\s*$");
            var segundoApellido = Capturar(texto, @"(?m)^Segundo Apellido:\s*(.+?)\s*$");

            var partes = new[] { nombres, primerApellido, segundoApellido }
                .Where(p => !string.IsNullOrWhiteSpace(p));
            var nombreCompleto = string.Join(" ", partes);
            return string.IsNullOrWhiteSpace(nombreCompleto) ? null : nombreCompleto;
        }

        private static List<RegimenFiscalInfo> ExtraerRegimenes(string texto)
        {
            var regimenes = new List<RegimenFiscalInfo>();

            var seccion = ExtraerSeccion(texto, "Regimenes:", "Obligaciones:");
            if (seccion == null)
                return regimenes;

            var lineaRegex = new Regex(
                @"^(?<desc>.+?)\s+(?<f1>\d{2}/\d{2}/\d{4})(?:\s+(?<f2>\d{2}/\d{2}/\d{4}))?\s*$");

            foreach (var linea in seccion.Split('\n'))
            {
                var l = linea.Trim();
                if (l.Length == 0)
                    continue;
                // salta la fila de encabezado de la tabla
                if (l.Equals("Regimen Fecha Inicio Fecha Fin", StringComparison.OrdinalIgnoreCase))
                    continue;

                var m = lineaRegex.Match(l);
                if (!m.Success)
                    continue;

                var descripcion = m.Groups["desc"].Value.Trim();
                regimenes.Add(new RegimenFiscalInfo
                {
                    Descripcion = descripcion,
                    Codigo = CatalogoRegimenFiscalTexto.BuscarCodigo(descripcion),
                    FechaInicio = ParseFecha(m.Groups["f1"].Value),
                    FechaFin = m.Groups["f2"].Success ? ParseFecha(m.Groups["f2"].Value) : null,
                });
            }

            return regimenes;
        }

        private static string ExtraerSeccion(string texto, string inicioMarcador, string finMarcador)
        {
            var inicio = texto.IndexOf(inicioMarcador, StringComparison.OrdinalIgnoreCase);
            if (inicio < 0)
                return null;
            inicio += inicioMarcador.Length;

            var fin = texto.IndexOf(finMarcador, inicio, StringComparison.OrdinalIgnoreCase);
            if (fin < 0)
                fin = texto.Length;

            return texto.Substring(inicio, fin - inicio);
        }

        private static DateTime? ParseFecha(string valor)
        {
            if (DateTime.TryParseExact(valor, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
                return fecha;
            return null;
        }

        private static string Capturar(string texto, string patron)
        {
            var m = Regex.Match(texto, patron);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }

        private static string QuitarAcentos(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalizado)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
