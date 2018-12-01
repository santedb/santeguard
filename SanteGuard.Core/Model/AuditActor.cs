/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: justin
 * Date: 2018-10-27
 */
using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using System;
using System.Xml.Serialization;

namespace SanteGuard.Model
{
    /// <summary>
    /// Represents the types of network access point values
    /// </summary>
    [XmlType("NetworkAccessPointType", Namespace = "http://santedb.org/santeguard")]
    public enum NetworkAccessPointType
    {
        MachineName = 1,
        IPAddress = 2,
        TelephoneNumber = 3,
        Email = 4,
        Uri = 5
    }

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
        public NetworkAccessPointType NetworkAccessPointType { get; set; }

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
