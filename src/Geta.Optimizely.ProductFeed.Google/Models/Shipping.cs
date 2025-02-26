// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Xml.Serialization;

namespace Geta.Optimizely.ProductFeed.Google.Models;

[XmlType(TypeName = "shipping")]
[Serializable]
public class Shipping
{
    [XmlElement("country", Namespace = "http://base.google.com/ns/1.0")]
    public string Country { get; set; }

    [XmlElement("service", Namespace = "http://base.google.com/ns/1.0")]
    public string Service { get; set; }

    [XmlElement("price", Namespace = "http://base.google.com/ns/1.0")]
    public string Price { get; set; }
}
