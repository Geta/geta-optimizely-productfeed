// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Xml.Serialization;

namespace Geta.Optimizely.ProductFeed.Google.Models
{
    public class LoyaltyPoints
    {
        [XmlElement("name", Namespace = "http://base.google.com/ns/1.0")]
        public string Name { get; set; }

        [XmlElement("points_value", Namespace = "http://base.google.com/ns/1.0")]
        public int PointsValue { get; set; }

        [XmlElement("ratio", Namespace = "http://base.google.com/ns/1.0")]
        public double Ration { get; set; }
    }
}
