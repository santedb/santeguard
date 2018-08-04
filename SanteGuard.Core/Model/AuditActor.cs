﻿using Newtonsoft.Json;
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
    /// Represents an actor participant
    /// </summary>
    [XmlType(nameof(AuditActor), Namespace = "http://santedb.org/santeguard")]
    [XmlRoot(nameof(AuditActor), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditActor))]
    public class AuditActor : IdentifiedData
    {
        // The loaded node
        private AuditNode m_node = null;

        /// <summary>
        /// Get the time that the participant was modified
        /// </summary>
        public override DateTimeOffset ModifiedOn => DateTimeOffset.MinValue;

        /// <summary>
        /// Gets or sets the key of the node this participant represents
        /// </summary>
        [XmlElement("node"), JsonProperty("node")]
        public Guid? NodeKey { get; set; }

        /// <summary>
        /// Gets the SID of the actor (device, application or user)
        /// </summary>
        [XmlElement("sid"), JsonProperty("sid")]
        public Guid? SecurityIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the user identifier
        /// </summary>
        [XmlElement("userId"), JsonProperty("userId")]
        public String UserIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        [XmlElement("userName"), JsonProperty("userName")]
        public String UserName { get; set; }

        /// <summary>
        /// Gets or sets the network access point
        /// </summary>
        [XmlElement("networkAp"), JsonProperty("networkAp")]
        public String NetworkAccessPoint { get; set; }

        /// <summary>
        /// Gets or sets the access point type
        /// </summary>
        [XmlElement("networkApType"), JsonProperty("networkApType")]
        public int NetworkAccessPointType { get; set; }

        /// <summary>
        /// Gets or sets the node
        /// </summary>
        [AutoLoad, SerializationReference(nameof(NodeKey)), XmlIgnore, JsonIgnore]
        public AuditNode Node
        {
            get
            {
                this.m_node = base.DelayLoad(this.NodeKey, this.m_node);
                return this.m_node;
            }
            set
            {
                this.m_node = value;
                this.NodeKey = value?.Key;
            }
        }
    }
}
