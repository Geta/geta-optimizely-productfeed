using System.Xml.Serialization;

namespace Geta.Optimizely.ProductFeed.Google.Models
{
    public class ProductDetail
    {
        [XmlElement("section_name", Namespace = "http://base.google.com/ns/1.0")]
        public string SectionName { get; set; }

        [XmlElement("attribute_name", Namespace = "http://base.google.com/ns/1.0")]
        public string AttributeName { get; set; }

        [XmlElement("attribute_value", Namespace = "http://base.google.com/ns/1.0")]
        public string AttributeValue { get; set; }
    }
}
