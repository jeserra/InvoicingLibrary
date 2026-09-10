using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.BindingModels
{
    public class ConceptoConsumoDeCombustibles
    {
        public ConceptoConsumoDeCombustibles()
        {
            ListaDeterminados = new List<Determinados>();
        }
        public  string Identificador { get; set; }
        public  DateTime Fecha { get; set; }
        
        public  String RFC { get; set; }

        public  String ClaveEstacion { get; set; }
        public  decimal Cantidad { get; set; }
        public  String NombreCombustible { get; set; }
        
        public  String FolioOperacion { get; set; }
        public  decimal ValorUnitario { get; set; }
        public  decimal Importe { get; set; }

        public List<Determinados> ListaDeterminados { get; set; } 
    }

    public class Determinados
    {
        public  string Impuesto { get; set; } // IVA - IEPS
        public decimal Tasa { get; set; }

        public decimal Importe { get; set; }

    }
}
