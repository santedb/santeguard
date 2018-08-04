using Newtonsoft.Json;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanteGuard.Model
{
    /// <summary>
    /// Represents a simple audit term
    /// </summary>
    [XmlType(nameof(AuditTerm), Namespace = "http://santedb.org/santeguard")]
    [XmlRoot(nameof(AuditTerm), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditTerm))]
    public class AuditTerm : IdentifiedData
    {
        /// <summary>
        /// Gets the time the term was modified
        /// </summary>
        public override DateTimeOffset ModifiedOn => DateTimeOffset.MinValue;

        /// <summary>
        /// Gets or sets the mnemonic
        /// </summary>
        [XmlElement("mnemonic"), JsonProperty("mnemonic")]
        public String Mnemonic { get; set; }

        /// <summary>
        /// Gets or sets the domain
        /// </summary>
        [XmlElement("domain"), JsonProperty("domain")]
        public String Domain { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        [XmlElement("display"), JsonProperty("display")]
        public String DisplayName { get; set; }
    }
}
