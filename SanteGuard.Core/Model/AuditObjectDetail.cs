using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Interfaces;
using System.Xml.Serialization;

namespace SanteGuard.Model
{
    /// <summary>
    /// Represents an audit object detail item
    /// </summary>
    [XmlType(nameof(AuditObjectDetail), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditObjectDetail))]
    [Classifier(nameof(DetailKey))]
    public class AuditObjectDetail : Association<AuditObject>
    {

        /// <summary>
        /// Gets the key of the detail
        /// </summary>
        [XmlElement("key")]
        public string DetailKey { get; set; }

        /// <summary>
        /// Gets the value
        /// </summary>
        [XmlElement("value")]
        public byte[] Value { get; set; }

    }
}