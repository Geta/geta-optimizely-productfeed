using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Geta.Optimizely.GoogleProductFeed.Models;

namespace Geta.Optimizely.GoogleProductFeed.Infrastructure
{
    public static class ObjectXmlSerializer
    {
        public static string Serialize(object value, Type type)
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("g", "http://base.google.com/ns/1.0");

            var serializer = new XmlSerializer(type, "http://www.w3.org/2005/Atom");

            using var stringWriter = new EncodedStringWriter(Encoding.UTF8);
            using var xmlWriter = new XmlTextWriter(stringWriter);

            xmlWriter.WriteStartDocument();
            serializer.Serialize(xmlWriter, value, namespaces);
            xmlWriter.Close();

            return stringWriter.ToString();
        }
    }
}