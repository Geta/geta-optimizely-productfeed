// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Xml.Serialization;

namespace Geta.Optimizely.ProductFeed.Google.Models
{
    public class SubscriptionCost
    {
        [XmlElement("period", Namespace = "http://base.google.com/ns/1.0")]
        public string Period { get; set; }

        [XmlElement("period_length", Namespace = "http://base.google.com/ns/1.0")]
        public int PeriodLength { get; set; }

        [XmlElement("amount", Namespace = "http://base.google.com/ns/1.0")]
        public string Amount { get; set; }
    }
}
