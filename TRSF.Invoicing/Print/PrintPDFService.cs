using System;
using System.Text;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Properties;
using iText.Layout.Element;
using System.IO;

using iText.Layout.Borders;
using TRSF.Invoicing.Translates;
using iText.IO.Image;
using TRSF.Invoicing.Utils;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using iText.IO.Font;
using iText.Layout.Renderer;
using iText.Layout.Layout;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;

namespace TRSF.Invoicing.Print
{
    public class PrintPDFService
    {
        public static void PrintCFDIPDF( cfdi33.Comprobante comprobante, ref  MemoryStream stream, byte[] imageQR, string emisorDireccionLinea1 = null, string emisorDireccionLinea2 = null )
        {
            
            var datosTimbrado = new cfdi33.TimbreFiscalDigital();

            if(comprobante.Complemento.Items !=null)
            {
                foreach (var item in comprobante.Complemento.Items)
                {
                    if (item.GetType() == typeof(cfdi33.TimbreFiscalDigital))
                    {
                        datosTimbrado = ((cfdi33.TimbreFiscalDigital)item);
                    }
                }
            }
            else
            {
                datosTimbrado = new cfdi33.TimbreFiscalDigital()
                {
                    FechaTimbrado = DateTime.Now,
                    Leyenda = "Comprobante no timbrado",
                    NoCertificadoSAT = "00000000000000000000",
                    RfcProvCertif = "XAXX000000AAA",
                    SelloCFD = string.Empty,
                    SelloSAT = string.Empty,
                    UUID = Guid.Empty.ToString(),

                };
            }

          
             
            var writer = new PdfWriter(stream);
            
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document doc = new Document(pdfDoc);

            Table tableDocto = new Table(new float[] { 4 }).UseAllAvailableWidth();

            Table tableImage = new Table(new float[] { 4, 4 } );
            Table tableEmisorReceptor = new Table(new float[] { 4,6 })
                .SetBorder(Border.NO_BORDER);
            Table tableDatosReceptor = new Table(new float[] { 4, 6 })
                .UseAllAvailableWidth();
            Table tableEmisor = new Table(new float[] { 4 , 2   })
                .SetBorder(Border.NO_BORDER); ;


            tableImage.SetWidth(UnitValue.CreatePercentValue(50));
            tableImage.SetTextAlignment(TextAlignment.RIGHT);

            tableImage.AddCell(getNormalCell("Factura", 24).SetTextAlignment(TextAlignment.CENTER));
             

            tableEmisor.AddCell(getNormalCell("Factura expedida por cuenta y orden de", 9, 1,2).SetTextAlignment(TextAlignment.CENTER));
 
            tableEmisor.AddCell(getNormalCell(comprobante.Emisor.Rfc, 9));

            tableEmisor.AddCell(getNormalCell(comprobante.Emisor.Nombre, 9,1,2));


            tableEmisor.AddCell(getNormalCell("Régimen Fiscal: ", 9));
            tableEmisor.AddCell(getNormalCell(comprobante.Emisor.RegimenFiscal.ToString().Replace("Item", ""), 9));
            //tableEmisor.AddCell(getNormalCell(TranslateCFDICatalogsToLegible.TranslateRegimenesFiscalesToLegible(comprobante.Emisor.RegimenFiscal), 9));



            tableEmisor.AddCell(getNormalCell(emisorDireccionLinea1 ?? String.Empty, 9, 1,2).SetTextAlignment(TextAlignment.CENTER));
            tableEmisor.AddCell(getNormalCell(emisorDireccionLinea2 ?? String.Empty, 9,1,2).SetTextAlignment(TextAlignment.CENTER));

         
            tableEmisorReceptor.AddCell(tableImage).SetBorder(Border.NO_BORDER);
            tableEmisorReceptor.AddCell(tableEmisor).SetBorder(Border.NO_BORDER);

            Table tableReceptor = new Table(new float[] { 3, 5 });

            tableReceptor.AddCell(getNormalCell("Facturado a: ", 12,1,2));

            tableReceptor.AddCell(getNormalCell("RFC: ",9));
            tableReceptor.AddCell(getRoundCell(comprobante.Receptor.Rfc, 9));

            tableReceptor.AddCell(getNormalCell("Razón Social: ", 9));
            tableReceptor.AddCell(getRoundCell(comprobante.Receptor.Nombre, 9));


            tableReceptor.AddCell(getNormalCell("Uso CFDI: ", 9));
            tableReceptor.AddCell(getRoundCell(TranslateCFDICatalogsToLegible.TranslateUSOCFDIToLegible(comprobante.Receptor.UsoCFDI), 9));



            Table tableDatosComprobante = new Table(new float[] { 1 });
            Table tableDatos = new Table(new float[] { 4, 2, 4, 2});
            //  tableDatos.AddCell(new Cell().Add("Fecha:").SetBorder(Border.NO_BORDER));

            tableDatos.AddCell(getNormalCell("Folio Fiscal:", 9, 1, 2));
            tableDatos.AddCell(getRoundCell(datosTimbrado.UUID, 9, 1, 2));

            tableDatos.AddCell(getNormalCell("Tipo de Comprobante:", 9));
            tableDatos.AddCell(getRoundCell(comprobante.TipoDeComprobante.ToString(), 9));
            // tableDatos.AddCell(getNormalCell(TranslateCFDICatalogsToLegible.TranslateTipoComproabanteToLegible(comprobante.TipoDeComprobante), 9));

            tableDatos.AddCell(getNormalCell("Código Postal:", 9));
            tableDatos.AddCell(getRoundCell(comprobante.LugarExpedicion, 9));

            tableDatos.AddCell(getNormalCell("Fecha Emisión:", 9));
            tableDatos.AddCell(getRoundCell( comprobante.Fecha.ToString(), 9));

            tableDatos.AddCell(getNormalCell("Fecha Certificación:", 9));
            tableDatos.AddCell(getRoundCell(datosTimbrado.FechaTimbrado.ToString(), 9));

            tableDatos.AddCell(getNormalCell("Serie:", 9 ));
            tableDatos.AddCell(getRoundCell(comprobante.Serie??String.Empty, 9));

            tableDatos.AddCell(getNormalCell("Folio:", 9));
            tableDatos.AddCell(getRoundCell(comprobante.Folio??String.Empty, 9));

            tableDatos.AddCell(getNormalCell("CSD del Emisor: ", 9));
            tableDatos.AddCell(getRoundCell(comprobante.NoCertificado ?? String.Empty, 9));

            tableDatos.AddCell(getNormalCell("CSD del SAT: ", 9));
            tableDatos.AddCell(getRoundCell(datosTimbrado.NoCertificadoSAT ?? String.Empty, 9));
            tableDatosComprobante.AddCell(tableDatos);
             

            tableDatosReceptor.AddCell(tableReceptor).SetBorder(Border.NO_BORDER);
            tableDatosReceptor.AddCell(tableDatosComprobante);
             
            tableDocto.AddCell(tableEmisorReceptor).SetBorder(Border.NO_BORDER);
           
            tableDocto.AddCell(tableDatosReceptor); 
 
            Table tableConceptos = new Table(new float[] { 5, 12,3,4,4,4 });

            tableConceptos.AddCell(getHeaderCell("Cantidad ",10 ));
            tableConceptos.AddCell(getHeaderCell("ClaveProdServ", 10));
            tableConceptos.AddCell(getHeaderCell("Unidad", 10));
            tableConceptos.AddCell(getHeaderCell("Concepto ", 10));
            tableConceptos.AddCell(getHeaderCell("Valor unitario",10));
            tableConceptos.AddCell(getHeaderCell("Importe", 10));

            foreach (var item in comprobante.Conceptos)
            { 
                tableConceptos.AddCell(getGridCell(item.Cantidad.ToString(), 10));
                tableConceptos.AddCell(getGridCell(item.ClaveProdServ.Replace("Item",""), 10));
                tableConceptos.AddCell(getGridCell(item.ClaveUnidad.ToString().Replace("Item", ""), 10));
                tableConceptos.AddCell(getGridCell(item.Descripcion.ToString(), 10));
                tableConceptos.AddCell(getGridCell(item.ValorUnitario.ToString(),10));
                tableConceptos.AddCell(getGridCell(item.Importe.ToString(),10));

                if (item.Impuestos != null)
                {
                    if (item.Impuestos.Traslados.Length > 0)
                    {
                        tableConceptos.AddCell(getHeaderCell("Impuestos Trasladados", 9));
                        tableConceptos.AddCell(getHeaderCell("Base", 9));
                        tableConceptos.AddCell(getHeaderCell("TipoFactor ", 9));
                        tableConceptos.AddCell(getHeaderCell("Tasa o Cuota", 9));
                        tableConceptos.AddCell(getHeaderCell("Impuesto", 9));
                        tableConceptos.AddCell(getHeaderCell("Importe", 9));

                        foreach (var taxitem in item.Impuestos.Traslados)
                        {
                            tableConceptos.AddCell(getGridCell(string.Empty, 9));
                            tableConceptos.AddCell(getGridCell(taxitem.Base.ToString(), 9));
                            tableConceptos.AddCell(getGridCell(taxitem.TipoFactor.ToString(), 9));
                            tableConceptos.AddCell(getGridCell(taxitem.TasaOCuota.ToString().Replace("Item", ""), 9));
                            tableConceptos.AddCell(getGridCell(taxitem.Impuesto.ToString().Replace("Item", ""), 9));
                            tableConceptos.AddCell(getGridCell(taxitem.Importe.ToString(), 9));

                        }
                    }
                }
            }


            tableConceptos.AddCell(getNormalCell(String.Empty, 10, 3, 6)); // Empty Row 
            tableConceptos.AddCell(getNormalCell(Convertir.EnLetras(comprobante.Total.ToString()), 9, 1, 4));
            tableConceptos.AddCell(getGridCell("SubTotal", 10));
            tableConceptos.AddCell(getRoundCell(comprobante.SubTotal.ToString(), 10));
            

            tableConceptos.AddCell(getNormalCell("Observaciones:", 9, 1, 4));
            tableConceptos.AddCell(getGridCell("IVA", 10));
            tableConceptos.AddCell(getRoundCell((comprobante.Total - comprobante.SubTotal).ToString(), 10));

            tableConceptos.AddCell(getNormalCell("Método de Pago", 9));
            tableConceptos.AddCell(getRoundCell(comprobante.MetodoPago.ToString(), 9));

            tableConceptos.AddCell(getNormalCell("Forma de Pago", 9));
            tableConceptos.AddCell(getRoundCell(comprobante.FormaPago.ToString().Replace("Item", ""), 9));

            tableConceptos.AddCell(getGridCell("Total", 10));
            tableConceptos.AddCell(getRoundCell(comprobante.Total.ToString(), 10));
 
            tableDocto.AddCell(tableConceptos);

            Table datosSATandQR = new Table(new float[] { 6, 2 });
            Table datosSAT = new Table(new float[] { 5   });

            
            datosSAT.AddCell(getHeaderCell("Este documento es una representacion impresa de un cfdi", 8).SetTextAlignment(TextAlignment.CENTER));
            datosSAT.AddCell(getNormalCell("Sello Digital del Emisor", 7, 1, 1));

            int sizeRow = 120;
            int maxRows = comprobante.Sello.Length / sizeRow;

            int i = 0;
            for (int x = 0; x <= maxRows; x++)
            {
                if ((i + sizeRow) < comprobante.Sello.Length)
                {
                    datosSAT.AddCell(getNormalCell(comprobante.Sello.Substring(i, sizeRow), 6).SetTextAlignment(TextAlignment.CENTER));
                }
                else
                {
                    var last = comprobante.Sello.Length - i;
                    datosSAT.AddCell(getNormalCell(comprobante.Sello.Substring(i, last), 6).SetTextAlignment(TextAlignment.CENTER));
                }
                i += sizeRow;
            }
         
             datosSAT.AddCell(getNormalCell("Sello Original del SAT", 7, 1, 1));
            //Marrano

            i = 0;
            maxRows = datosTimbrado.SelloSAT.Length / sizeRow;
            for (int x = 0; x <= maxRows; x++)
            {
                if ((i + sizeRow) < datosTimbrado.SelloSAT.Length)
                {
                    datosSAT.AddCell(getNormalCell(datosTimbrado.SelloSAT.Substring(i, sizeRow), 6).SetTextAlignment(TextAlignment.CENTER));
                }
                else
                {
                    var last = datosTimbrado.SelloSAT.Length - i;
                    datosSAT.AddCell(getNormalCell(datosTimbrado.SelloSAT.Substring(i, last), 6).SetTextAlignment(TextAlignment.CENTER));
                }
                i += sizeRow;
            }

            datosSAT.AddCell(getHeaderCell("Cadena Original del complemento de certificado digital del SAT", 7).SetTextAlignment(TextAlignment.CENTER));

            i = 0;
            maxRows = datosTimbrado.CadenaOriginal.Length / sizeRow;

            for (int x = 0; x <= maxRows; x++)
            {
                if ((i + 100) < datosTimbrado.CadenaOriginal.Length)
                {
                    datosSAT.AddCell(getNormalCell(datosTimbrado.CadenaOriginal.Substring(i, sizeRow), 6).SetTextAlignment(TextAlignment.CENTER));
                }
                else
                {
                    var last = datosTimbrado.CadenaOriginal.Length - i;
                    datosSAT.AddCell(getNormalCell(datosTimbrado.CadenaOriginal.Substring(i, last), 6).SetTextAlignment(TextAlignment.CENTER));
                }
                i += sizeRow;
            }

            datosSATandQR.AddCell(datosSAT);
            datosSATandQR.AddCell(createImageCell(imageQR));
            tableDocto.AddCell(datosSATandQR);
            doc.Add(tableDocto);
            doc.Close();             
        }

        public static Cell createImageCell(String path)  
        {
            if (!String.IsNullOrEmpty(path))
            {
                Image img = new Image(ImageDataFactory.Create(path));
                Cell cell = new Cell().Add(img.SetAutoScale(true));
                cell.SetBorder(null);
                return cell;
            }
            else
            { 
                Cell emptyCell = new Cell();
                return emptyCell;
            }
        }

        public static Cell createImageCell(byte[] bytearray)
        {
            try
            {
                Image img = new Image(ImageDataFactory.Create(bytearray));
                Cell cell = new Cell().Add(img.SetAutoScale(true));
                cell.SetBorder(null);
                return cell;
            }
            catch(Exception ex)
            {
                
                Cell emptyCell = new Cell();
                return emptyCell;
            }
        }

       

        public static Cell getNormalCell(String input, float size, int rowSpan = 1, int colSpan = 1)
        {
            if (String.IsNullOrEmpty(input))
            {
                return new Cell(rowSpan, colSpan);
            }

            PdfFont f = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            Cell cell = new Cell(rowSpan, colSpan).Add(new Paragraph(input).SetFont(f));
            cell.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            if (size > 0)
            {
                //size = -size;
                cell.SetFontSize(size);
                cell.SetFontColor(ColorConstants.BLACK);
            }
           // cell.SetNextRenderer(new RoundedCornersCellRenderer(cell));
            cell.SetBorder(Border.NO_BORDER);
            return cell;
        }

        public static Cell getHeaderCell(String input, float size, int rowSpan = 1, int colSpan = 1)
        {
            if (String.IsNullOrEmpty(input))
            {
                return new Cell(rowSpan, colSpan);
            }

            PdfFont f = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            Cell cell = new Cell(rowSpan, colSpan).Add(new Paragraph(input).SetFont(f));
            cell.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            if (size > 0)
            {
                //size = -size;
                cell.SetFontSize(size);
                cell.SetFontColor(ColorConstants.WHITE);
                cell.SetBold();
            }
            // cell.SetNextRenderer(new RoundedCornersCellRenderer(cell));
            cell.SetBorder(Border.NO_BORDER);
            cell.SetBackgroundColor(ColorConstants.BLACK);
            return cell;
        }

        public static Cell getRoundCell(String input, float size, int rowSpan = 1, int colSpan = 1)
        {
            if (String.IsNullOrEmpty(input))
            {
                return new Cell(rowSpan, colSpan);
            }

            PdfFont f = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            Cell cell = new Cell(rowSpan, colSpan).Add(new Paragraph(input).SetFont(f));
            cell.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            if (size > 0)
            {
                //size = -size;
                cell.SetFontSize(size);
                cell.SetFontColor(ColorConstants.BLACK);
            }
            // cell.SetNextRenderer(new RoundedCornersCellRenderer(cell));
            cell.SetNextRenderer(new RoundedCornersCellRenderer(cell));
            cell.SetPadding(5);
            cell.SetBorder(null);
           // cell.SetBackgroundColor(Color.GRAY);
            return cell;
        }

        public static Cell getGridCell(String input, float size)
        {
            if (String.IsNullOrEmpty(input))
            {
                return new Cell();
            }

            PdfFont f = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            Cell cell = new Cell().Add(new Paragraph(input).SetFont(f));
            cell.SetHorizontalAlignment(HorizontalAlignment.LEFT);
            if (size > 0)
            {
                //size = -size;
                cell.SetFontSize(size);
                cell.SetFontColor(ColorConstants.BLACK);
            }
            //cell.SetBorder(Border.NO_BORDER);
            cell.SetBorderLeft(new SolidBorder(ColorConstants.BLACK, 1));
            cell.SetBorderRight(new SolidBorder(ColorConstants.BLACK, 1));
            cell.SetPaddingTop(8).SetPaddingBottom(8);
           
            return cell;
        }


        private class RoundedCornersCellRenderer : CellRenderer
        {
        public RoundedCornersCellRenderer(Cell modelElement):base(modelElement)
        {
            
        }
             
        public override void Draw(DrawContext drawContext)
        {
            float llx = GetOccupiedAreaBBox().GetX() + 2;
            float lly = GetOccupiedAreaBBox().GetY() + 2;
            float urx = GetOccupiedAreaBBox().GetX() + GetOccupiedAreaBBox().GetWidth() - 2;
            float ury = GetOccupiedAreaBBox().GetY() + GetOccupiedAreaBBox().GetHeight() - 2;
            float r = 4;
            float b = 0.4477f;
            PdfCanvas canvas = drawContext.GetCanvas();
            canvas.MoveTo(llx, lly);
            canvas.LineTo(urx, lly);
            canvas.LineTo(urx, ury - r);
            canvas.CurveTo(urx, ury - r * b, urx - r * b, ury, urx - r, ury);
            canvas.LineTo(llx + r, ury);
            canvas.CurveTo(llx + r * b, ury, llx, ury - r * b, llx, ury - r);
            canvas.LineTo(llx, lly);
            canvas.Stroke();
           
            base.Draw(drawContext);
        }
    }

    }
}
