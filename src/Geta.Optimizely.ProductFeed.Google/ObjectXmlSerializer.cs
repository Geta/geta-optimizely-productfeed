// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Geta.Optimizely.ProductFeed.Google
{
    public static class ObjectXmlSerializer
    {
        public static byte[] Serialize(object value, Type type)
        {
            var serializer = new XmlSerializer(type, "http://www.w3.org/2005/Atom");

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("g", "http://base.google.com/ns/1.0");

            using var stringWriter = new EncodedStringWriter(Encoding.UTF8);
            using var xmlWriter = new XmlTextWriter(stringWriter);

            xmlWriter.WriteStartDocument();
            serializer.Serialize(xmlWriter, value, namespaces);
            xmlWriter.Close();

            return Encoding.UTF8.GetBytes(stringWriter.ToString());
        }
    }
}
