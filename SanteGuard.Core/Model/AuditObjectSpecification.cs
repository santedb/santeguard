using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using System;
using System.Xml.Serialization;

namespace SanteGuard.Model
{
    /// <summary>
    /// Gets or sets the audit object specification
    /// </summary>
    [XmlType(nameof(AuditObjectSpecification), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditObjectSpecification))]
    [Classifier(nameof(SpecificationType))]
    public class AuditObjectSpecification : Association<AuditObject>
    {
        /// <summary>
        /// Get the specification 
        /// </summary>
        [XmlElement("value"), JsonProperty("value")]
        public String Specification { get; set; }

        /// <summary>
        /// Gets the spec type
        /// </summary>
        [XmlElement("type"), JsonProperty("type")]
        public Char SpecificationType { get; set; }

        /// <summary>
        /// Get the modified on
        /// </summary>
        public override DateTimeOffset ModifiedOn => DateTimeOffset.Now;
    }
}