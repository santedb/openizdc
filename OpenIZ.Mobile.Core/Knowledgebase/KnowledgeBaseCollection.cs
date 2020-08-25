using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenIZ.Mobile.Core.Knowledgebase
{
    /// <summary>
    /// Knwoledge 
    /// </summary>
    [XmlType(nameof(KnowledgeBaseCollection), Namespace = "http://openiz.org/kb")]
    [XmlRoot(nameof(KnowledgeBaseCollection), Namespace = "http://openiz.org/kb")]
    public class KnowledgeBaseCollection
    {

        // Serializer 
        private static XmlSerializer m_xsz = new XmlSerializer(typeof(KnowledgeBaseCollection));

        /// <summary>
        /// Knowledgebase entry collection
        /// </summary>
        [XmlElement("entry")]
        public List<KnowledgeBaseEntry> Entry { get; set; }

        /// <summary>
        /// Load the specified entry
        /// </summary>
        public static KnowledgeBaseCollection Load(Stream s)
        {
            return m_xsz.Deserialize(s) as KnowledgeBaseCollection;
        }
    }
}
