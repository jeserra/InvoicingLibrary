using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Text;

namespace TRSF.Invoicing.Utils
{
    public static class XmlSerializerHelper
    {
        public static T Deserialize<T>(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            
            TextReader reader = new StreamReader(fileName);
            T obj = (T)serializer.Deserialize(reader);
            reader.Close();
            return obj;
        }

        public static void Serialize<T>(T obj, string fileName)
        {
            TextWriter writer = new StreamWriter(fileName,false,System.Text.Encoding.UTF8);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, obj);
            writer.Close();
        }

        public static void Serialize<T>(T obj, XmlSerializerNamespaces ns, string fileName)
        {
            TextWriter writer = new StreamWriter(fileName, false, System.Text.Encoding.UTF8);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, obj);
            writer.Close();
        }

        public static string ToXmlString<T>(this T input, XmlSerializerNamespaces ns,Encoding encoding)
        {
            using (var writer = new Utf8StringWriter())
            {
                input.ToXml(ns, writer, encoding);
                return writer.ToString();
            }
        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        public static string ToXmlString<T>(this T input)
        {
            using (var writer = new StringWriter())
            {
                input.ToXml(writer, System.Text.Encoding.UTF8);
                return writer.ToString();
            }
        }

        public static void ToXml<T>(this T objectToSerialize, XmlSerializerNamespaces ns, Stream stream,Encoding encoding)
        {
            //XmlWriterSettings settings = new XmlWriterSettings();
            //settings.Encoding = encoding;
            //settings.Indent = true;
            //settings.IndentChars = "\t";
            //settings.NewLineChars = Environment.NewLine;
            //settings.ConformanceLevel = ConformanceLevel.Document;

               //var serializer = new XmlSerializer(typeof(SomeSerializableObject));
            //using (var memStm = new MemoryStream())
            //using (var xw = XmlWriter.Create(memStm))
            //{
            //    serializer.Serialize(xw, entry);
            //    var utf8 = memStm.ToArray();
            //}


//            var serializer = new XmlSerializer(typeof(SomeSerializableObject));
//using(var memStm = new MemoryStream())
//using(var  xw = XmlWriter.Create(memStm))
//{
//  serializer.Serialize(xw, entry);
//  var utf8 = memStm.ToArray();
//}

            //MemoryStream ms = new MemoryStream();
            //XmlTextWriter xmlTextWriter = new XmlTextWriter(ms, encoding);
            

            //typeof(T)).Serialize(xmlTextWriter, objectToSerialize, ns);
            //ms = (MemoryStream)xmlTextWriter.BaseStream;
            //return encoding.GetString(ms.ToArray());



           XmlSerializer ser = new XmlSerializer(typeof(T));
            
        
           MemoryStream ms = new MemoryStream();
           XmlTextWriter xmlWriter = new XmlTextWriter(ms, encoding);
           //// xmlWriter.Namespaces = true;

           ser.Serialize(stream, objectToSerialize, ns);
           // xmlWriter.Close();
           // ms.Close();
           // string xml;
           // xml = Encoding.UTF8.GetString(ms.GetBuffer());
           // xml = xml.Substring(xml.IndexOf(Convert.ToChar(60)));
           // xml = xml.Substring(0, (xml.LastIndexOf(Convert.ToChar(62)) + 1));
           // return xml;


            new XmlSerializer(typeof(T)).Serialize(stream, objectToSerialize, ns);
        }

        public static void ToXml<T>(this T objectToSerialize, XmlSerializerNamespaces ns, StringWriter writer,Encoding encoding)
        {
            new XmlSerializer(typeof(T)).Serialize(writer, objectToSerialize, ns);
        }

        public static void ToXml<T>(this T objectToSerialize, Stream stream,Encoding encoding)
        {
            new XmlSerializer(typeof(T)).Serialize(stream, objectToSerialize);
        }

        public static void ToXml<T>(this T objectToSerialize, StringWriter writer,Encoding encoding)
        {
            new XmlSerializer(typeof(T)).Serialize(writer, objectToSerialize);
        }



        public static T Deserialize<T>(StringReader dataIn)
        {
            return (T)(new XmlSerializer(typeof(T), String.Empty)).Deserialize(dataIn);
        }

    }
}
