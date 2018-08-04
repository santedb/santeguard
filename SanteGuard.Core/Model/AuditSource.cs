using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanteGuard.Model
{
    /// <summary>
    /// Represents an audit source
    /// </summary>
    [XmlType(nameof(AuditSource), Namespace = "http://santedb.org/santeguard")]
    [XmlRoot(nameof(AuditSource), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditSource))]
    public class AuditSource : IdentifiedData
    {
        /// <summary>
        /// Gets or sets the modified time
        /// </summary>
        public override DateTimeOffset ModifiedOn => DateTimeOffset.MinValue;

        /// <summary>
        /// Enterprise site identifier
        /// </summary>
        [XmlElement("enterpriseSite"), JsonProperty("enterpriseSite")]
        public String EnterpriseSiteId { get; set; }

        /// <summary>
        /// Gets or sets the audit source
        /// </summary>
        [XmlElement("auditSource"), JsonProperty("auditSource")]
        public String AuditSourceId { get; set; }

        /// <summary>
        /// Gets or sets the source type
        /// </summary>
        [AutoLoad, XmlElement("sourceType"), JsonProperty("sourceType")]
        public List<AuditTerm> SourceType { get; set; }

    }
}
