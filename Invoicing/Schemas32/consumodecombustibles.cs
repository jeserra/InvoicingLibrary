﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.6.1055.0.
// 

namespace Invoicing.cfdi33
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true,
        Namespace = "http://www.sat.gob.mx/consumodecombustibles")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.sat.gob.mx/consumodecombustibles",
        IsNullable = false)]
    public partial class ConsumoDeCombustibles
    {

        private ConsumoDeCombustiblesConceptoConsumoDeCombustibles[] conceptosField;

        private string versionField;

        private string tipoOperacionField;

        private string numeroDeCuentaField;

        private decimal subTotalField;

        private bool subTotalFieldSpecified;

        private decimal totalField;

        public ConsumoDeCombustibles()
        {
            this.versionField = "1.0";
            this.tipoOperacionField = "monedero electrónico";
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ConceptoConsumoDeCombustibles", IsNullable = false)]
        public ConsumoDeCombustiblesConceptoConsumoDeCombustibles[] Conceptos
        {
            get { return this.conceptosField; }
            set { this.conceptosField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string version
        {
            get { return this.versionField; }
            set { this.versionField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string tipoOperacion
        {
            get { return this.tipoOperacionField; }
            set { this.tipoOperacionField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string numeroDeCuenta
        {
            get { return this.numeroDeCuentaField; }
            set { this.numeroDeCuentaField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal subTotal
        {
            get { return this.subTotalField; }
            set { this.subTotalField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool subTotalSpecified
        {
            get { return this.subTotalFieldSpecified; }
            set { this.subTotalFieldSpecified = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal total
        {
            get { return this.totalField; }
            set { this.totalField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true,
        Namespace = "http://www.sat.gob.mx/consumodecombustibles")]
    public partial class ConsumoDeCombustiblesConceptoConsumoDeCombustibles
    {

        private ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminado[] determinadosField;

        private string identificadorField;

        private System.DateTime fechaField;

        private string rfcField;

        private string claveEstacionField;

        private decimal cantidadField;

        private string nombreCombustibleField;

        private string folioOperacionField;

        private decimal valorUnitarioField;

        private decimal importeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Determinado", IsNullable = false)]
        public ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminado[] Determinados
        {
            get { return this.determinadosField; }
            set { this.determinadosField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string identificador
        {
            get { return this.identificadorField; }
            set { this.identificadorField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime fecha
        {
            get { return this.fechaField; }
            set { this.fechaField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string rfc
        {
            get { return this.rfcField; }
            set { this.rfcField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string claveEstacion
        {
            get { return this.claveEstacionField; }
            set { this.claveEstacionField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal cantidad
        {
            get { return this.cantidadField; }
            set { this.cantidadField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string nombreCombustible
        {
            get { return this.nombreCombustibleField; }
            set { this.nombreCombustibleField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string folioOperacion
        {
            get { return this.folioOperacionField; }
            set { this.folioOperacionField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal valorUnitario
        {
            get { return this.valorUnitarioField; }
            set { this.valorUnitarioField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal importe
        {
            get { return this.importeField; }
            set { this.importeField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true,
        Namespace = "http://www.sat.gob.mx/consumodecombustibles")]
    public partial class ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminado
    {

        private ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminadoImpuesto impuestoField;

        private decimal tasaField;

        private decimal importeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminadoImpuesto impuesto
        {
            get { return this.impuestoField; }
            set { this.impuestoField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal tasa
        {
            get { return this.tasaField; }
            set { this.tasaField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal importe
        {
            get { return this.importeField; }
            set { this.importeField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true,
        Namespace = "http://www.sat.gob.mx/consumodecombustibles")]
    public enum ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminadoImpuesto
    {

        /// <remarks/>
        IVA,

        /// <remarks/>
        IEPS,
    }
}