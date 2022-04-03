// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Xml.Serialization;

namespace Geta.Optimizely.ProductFeed.Google.Models
{
    public class Installment
    {
        [XmlElement("months", Namespace = "http://base.google.com/ns/1.0")]
        public int Months { get; set; }

        [XmlElement("amount", Namespace = "http://base.google.com/ns/1.0")]
        public string Amount { get; set; }
    }
}
