﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace InvoicingLibrary.BindingModels
{

    public class Comprobante
    {

        public Comprobante ()
        {
            Emisor = new Emisor();
            Receptor = new Receptor();
            Conceptos = new List<Concepto>();
            //ValesDespensa= new ValesDeDespensa();
        }

        /* No estan en el estandar del cfdi */
        public int CustomerId { get; set; }
        public int SupplierId { get; set; }
        public int ProductId { get; set; }


        public String Version { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public string LugarExpedicion { get; set; }
        public string Moneda { get; set; }
        public string FormaPago { get; set; } // Obtener id desde la bd 
        public string CondicionesDePago { get; set; }
        public string MetodoPago { get; set; } // PUE, PIP, PPD
        public string noCertificado { get; set; }
        public string Certificado { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Descuento { get; set; } // TODO Revisar si aplican descuentos
        public decimal Total { get; set; }
        public string TipoComprobante { get; set; }
        public DateTime Fecha { get; set; }
 
        public Emisor Emisor
        {
            get;set;
        }
        public Receptor Receptor
        {
            get; set;
        }

        public List<Concepto> Conceptos
        {
            get; set;
        }

        public ValesDeDespensa ValesDespensa
        {
            get; set;
        }

        public ConsumoDeCombustibles ConsumoCombustibles { get; set; }

        public Pagos Pagos { get; set; }

        public String UsoCFDI
        {
            get; set;
        }

        public Guid UUID { get; set; }

        public DateTime FechaTimbrado { get; set; }
    }
}