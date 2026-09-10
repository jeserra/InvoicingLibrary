using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace TRSF.Invoicing.Utils
{
    public class XsdValidator
        {
            public List<XmlSchema> Schemas { get; set; }
            public List<String> Errors { get; set; }
            public List<String> Warnings { get; set; }

            public XsdValidator()
            {
                Schemas = new List<XmlSchema>();
            }

            /// <summary>
            /// Add a schema to be used during the validation of the XML document
            /// </summary>
            /// <param name="schemaFileLocation">The file path for the XSD schema file to be added for validation</param>
            /// <returns>True if the schema file was successfully loaded, else false (if false, view Errors/Warnings for reason why)</returns>
            public bool AddSchema(string schemaFileLocation)
            {
                if (String.IsNullOrEmpty(schemaFileLocation)) return false;
                if (!File.Exists(schemaFileLocation)) return false;

                // Reset the Error/Warning collections
                Errors = new List<string>();
                Warnings = new List<string>();

                XmlSchema schema;

                using (var fs = File.OpenRead(schemaFileLocation))
                {
                    schema = XmlSchema.Read(fs, ValidationEventHandler);
                }

                var isValid = !(Errors.Count>0) && !(Warnings.Count>0);

                if (isValid)
                {
                    Schemas.Add(schema);
                }

                return isValid;
            }

            /// <summary>
            /// Perform the XSD validation against the specified XML document
            /// </summary>
            /// <param name="xmlLocation">The full file path of the file to be validated</param>
            /// <returns>True if the XML file conforms to the schemas, else false</returns>
            public bool IsValid(string xmlLocation)
            {
                if (!File.Exists(xmlLocation))
                {
                    throw new FileNotFoundException("The specified XML file does not exist", xmlLocation);
                }

                using (var xmlStream = File.OpenRead(xmlLocation))
                {
                    return IsValid(xmlStream);
                }
            }

            /// <summary>
            /// Perform the XSD validation against the supplied XML stream
            /// </summary>
            /// <param name="xmlStream">The XML stream to be validated</param>
            /// <returns>True is the XML stream conforms to the schemas, else false</returns>
            private bool IsValid(Stream xmlStream)
            {
                // Reset the Error/Warning collections
                Errors = new List<string>();
                Warnings = new List<string>();

                XmlReaderSettings settings = new XmlReaderSettings();
              
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationEventHandler += ValidationEventHandler;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema; //Preload the schemas into the validator
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings; //Preload the schemas into the validator
                // settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation; // Resolve and load the resources during processing (incompatible with Preload the schemas into the validator)

                foreach (var xmlSchema in Schemas)
                {
                    settings.Schemas.Add(xmlSchema);
                }

                var xmlFile = XmlReader.Create(xmlStream, settings);

                try
                {
                    while (xmlFile.Read()) { }
                }
                catch (XmlException xex)
                {
                    Errors.Add(xex.Message);
                }

                return !(Errors.Count > 0) && !(Warnings.Count > 0);
            }

            private void ValidationEventHandler(object sender, ValidationEventArgs e)
            {
                switch (e.Severity)
                {
                    case XmlSeverityType.Error:
                        Errors.Add(e.Message);
                        break;
                    case XmlSeverityType.Warning:
                        Warnings.Add(e.Message);
                        break;
                }
            }
        }
}


    

    //public class ValidateXML
    //{
    //    private TextReader _theXML;
    //    private TextReader _theXSD;
    //    private List<ValidationEventArgs> _vargs = new List<ValidationEventArgs>();


    //    public TextReader TheXML
    //    {
    //        get { return _theXML; }
    //        set { _theXML = value; }
    //    }

    //    public TextReader TheXSD
    //    {
    //        get { return _theXSD; }
    //        set { _theXSD = value; }
    //    }

    //    public List<ValidationEventArgs> ValidationArgs
    //    {
    //        get { return _vargs; }
    //        set { throw new NotImplementedException(); }
    //    }

    //    public bool IsValid()
    //    {
    //        ValidationEventHandler ValidationHandler = new ValidationEventHandler(ValidationCallBack);

    //        XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
    //        xmlSchemaSet.ValidationEventHandler += ValidationHandler;
    //        xmlSchemaSet.XmlResolver = new XmlUrlResolver();

    //        XmlSchema xmlSchema = XmlSchema.Read(_theXSD, ValidationHandler);

    //        // Adding a target schema here will automatically resolve all
    //        // include/import'ed schemas - these will reside together with the
    //        // target schema in the XmlSchemaSet.Schemas() collection of 
    //        // XmlSchema objects.
    //        xmlSchemaSet.Add(xmlSchema);
    //        xmlSchemaSet.Compile();
    //        if (xmlSchemaSet.IsCompiled)
    //        {
    //            XmlReaderSettings settings = new XmlReaderSettings();
    //            settings.Schemas.Add(xmlSchemaSet);
    //            settings.ValidationType = ValidationType.Schema;
    //            settings.ValidationEventHandler += ValidationHandler;
    //            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
    //            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
    //            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

    //            XmlReader reader = XmlReader.Create(_theXML, settings);
    //            // Parse the file.  
    //            while (reader.Read()) ;
    //        }
    //        return (0 == _vargs.Count);
    //    }


    //    // Display any warnings or errors. 
    //    private void ValidationCallBack(object sender, ValidationEventArgs args)
    //    {
    //        _vargs.Add(args);
    //    }




    //} 
//}
