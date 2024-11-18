// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Xml.Serialization;

namespace Geta.Optimizely.ProductFeed.Google.Models;

public class Tax
{
    [XmlElement("country", Namespace = "http://base.google.com/ns/1.0")]
    public string Country { get; set; }

    [XmlElement("region", Namespace = "http://base.google.com/ns/1.0")]
    public string Region { get; set; }

    [XmlElement("rate", Namespace = "http://base.google.com/ns/1.0")]
    public double Rate { get; set; }

    [XmlElement("tax_ship", Namespace = "http://base.google.com/ns/1.0")]
    public string TaxShip { get; set; }
}
