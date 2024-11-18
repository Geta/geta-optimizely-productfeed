// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Xml.Serialization;

namespace Geta.Optimizely.ProductFeed.Google.Models;

public class ProductDetail
{
    [XmlElement("section_name", Namespace = "http://base.google.com/ns/1.0")]
    public string SectionName { get; set; }

    [XmlElement("attribute_name", Namespace = "http://base.google.com/ns/1.0")]
    public string AttributeName { get; set; }

    [XmlElement("attribute_value", Namespace = "http://base.google.com/ns/1.0")]
    public string AttributeValue { get; set; }
}
