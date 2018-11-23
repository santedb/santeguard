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
using SanteDB.Core.Model.Security;
using System;
using System.Xml.Serialization;

namespace SanteGuard.Model
{
    /// <summary>
    /// Represents a node on the security network. This can be on one or more physical devices
    /// </summary>
    [XmlType(nameof(AuditNode), Namespace = "http://santedb.org/santeguard")]
    [XmlRoot(nameof(AuditNode), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditNode))]
    public class AuditNode : NonVersionedEntityData
    {

        // Security device
        private SecurityDevice m_securityDevice;

        /// <summary>
        /// Gets or sets the security device key if known
        /// </summary>
        [XmlElement("securityDevice"), JsonProperty("securityDevice")]
        public Guid? SecurityDeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the security device 
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(SecurityDeviceKey))]
        public SecurityDevice SecurityDevice
        {
            get
            {
                this.m_securityDevice = base.DelayLoad(this.SecurityDeviceKey, this.m_securityDevice);
                return this.m_securityDevice;
            }
            set
            {
                this.m_securityDevice = value;
                this.SecurityDeviceKey = value?.Key;
            }
        }

        /// <summary>
        /// Gets or sets the name of the node
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the host name of the node
        /// </summary>
        [XmlElement("host"), JsonProperty("host")]
        public String HostName { get; set; }

        /// <summary>
        /// Gets or sets the status of the node
        /// </summary>
        [XmlElement("status"), JsonProperty("status")]
        public AuditStatusType Status { get; set; }


    }
}