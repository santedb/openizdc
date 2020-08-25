using Newtonsoft.Json;
using OpenIZ.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace OpenIZ.Mobile.Core.Knowledgebase
{
    /// <summary>
    /// Knowledgebase entry
    /// </summary>
    [XmlType(nameof(KnowledgeBaseEntry), Namespace = "http://openiz.org/kb")]
    public class KnowledgeBaseEntry
    {

        // When clause
        private IEnumerable<Regex> m_whenClause;

        /// <summary>
        /// Type of error
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public DetectedIssuePriorityType Type { get; set; }

        /// <summary>
        /// The unique identifier of the event
        /// </summary>
        [XmlAttribute("id"), JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// When the kb event is fired
        /// </summary>
        [XmlArray("when"), XmlArrayItem("match"), JsonIgnore]
        public List<string> WhenClauseXml { get; set; }

        /// <summary>
        /// When clause
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public IEnumerable<Regex> WhenClause
        {
            get
            {
                if (this.m_whenClause == null)
                    this.m_whenClause = this.WhenClauseXml.Select(o => new Regex(o));
                return this.m_whenClause;
            }
        }

        /// <summary>
        /// True if this matches
        /// </summary>
        public bool Matches(string exceptionMessage)
        {
            if (!string.IsNullOrEmpty(exceptionMessage))
                return this.WhenClause.Any(o => o.IsMatch(exceptionMessage));
            return false;
        }

        /// <summary>
        /// Message
        /// </summary>
        [XmlElement("message"), JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Resolution
        /// </summary>
        [XmlElement("resolution"), JsonProperty("resolution")]
        public string Resolution { get; set; }

    }
}