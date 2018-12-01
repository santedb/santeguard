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
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteGuard.Model
{
    /// <summary>
    /// Represents a participation in an audit
    /// </summary>
    [XmlType(nameof(AuditParticipation), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditParticipation))]
    public class AuditParticipation : Association<Audit>
    {

        private AuditActor m_actor = null;

        /// <summary>
        /// Creates a new audit participation
        /// </summary>
        public AuditParticipation()
        {
            this.Roles = new List<AuditTerm>();
        }

        /// <summary>
        /// Gets or sets the actor
        /// </summary>
        [XmlElement("actor"), JsonProperty("actor")]
        public Guid? ActorKey { get; set; }

        /// <summary>
        /// Gets or sets whether the actor is the requestor
        /// </summary>
        [XmlElement("requestor"), JsonProperty("requestor")]
        public Boolean IsRequestor { get; set; }

        /// <summary>
        /// Gets or sets the roles
        /// </summary>
        [XmlElement("role"), JsonProperty("role")]
        public List<AuditTerm> Roles { get; set; }

        /// <summary>
        /// Gets or sets the actor
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(ActorKey))]
        public AuditActor Actor
        {
            get
            {
                this.m_actor = base.DelayLoad(this.ActorKey, this.m_actor);
                return this.m_actor;
            }
            set
            {
                this.m_actor = value;
                this.ActorKey = value.Key;
            }
        }
    }
}
