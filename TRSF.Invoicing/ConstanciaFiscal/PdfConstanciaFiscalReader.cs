using System.IO;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace TRSF.Invoicing.ConstanciaFiscal
{
    /// <summary>
    /// Lee una Constancia de Situacion Fiscal (CSF) del SAT en PDF. Usa iText7 solo para
    /// extraer el texto plano; todo el parseo real vive en <see cref="ConstanciaFiscalTextParser"/>,
    /// que se puede probar sin un PDF. La escritura de PDFs (representacion impresa del CFDI)
    /// vive por separado en TRSF.Invoicing.PDF via IPDFProvider - este archivo solo lee.
    /// </summary>
    public static class PdfConstanciaFiscalReader
    {
        public static ConstanciaSituacionFiscal Leer(string rutaPdf)
        {
            using var stream = File.OpenRead(rutaPdf);
            return Leer(stream);
        }

        public static ConstanciaSituacionFiscal Leer(Stream pdfStream)
        {
            var texto = ExtraerTexto(pdfStream);
            return ConstanciaFiscalTextParser.Parse(texto);
        }

        private static string ExtraerTexto(Stream pdfStream)
        {
            using var pdf = new PdfDocument(new PdfReader(pdfStream));
            var sb = new StringBuilder();
            var estrategia = new SimpleTextExtractionStrategy();
            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                sb.AppendLine(PdfTextExtractor.GetTextFromPage(pdf.GetPage(i), estrategia));
            }
            return sb.ToString();
        }
    }
}
