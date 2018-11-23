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
using System.Xml.Serialization;
using System;
using System.Collections;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using System.Collections.Generic;

namespace SanteGuard.Model
{
    /// <summary>
    /// Represents an object being audited
    /// </summary>
    [XmlType(nameof(AuditObject), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditObject))]
    public class AuditObject : Association<Audit>
    {
        // Backing fields
        private AuditTerm m_typeCode;
        private AuditTerm m_roleCode;
        private AuditTerm m_lifecycleCode;
        private AuditTerm m_idTypeCode;

        /// <summary>
        /// Create a new audit object
        /// </summary>
        public AuditObject()
        {
            this.Specification = new List<AuditObjectSpecification>();
            this.Details = new List<AuditObjectDetail>();
        }

        /// <summary>
        /// Gets or sets the external identifier
        /// </summary>
        [XmlElement("identifier"), JsonProperty("identifier")]
        public String ExternalIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the type code 
        /// </summary>
        [XmlElement("type"), JsonProperty("type")]
        public Guid? TypeCodeKey { get; set; }

        /// <summary>
        /// Gets or sets the role code
        /// </summary>
        [XmlElement("role"), JsonProperty("role")]
        public Guid? RoleCodeKey { get; set; }

        /// <summary>
        /// Gets or sets the lifecycle
        /// </summary>
        [XmlElement("lifecycle"), JsonProperty("lifecycle")]
        public Guid? LifecycleCodeKey { get; set; }

        /// <summary>
        /// Gets or sets the id type code
        /// </summary>
        [XmlElement("idType"), JsonProperty("idType")]
        public Guid? IdTypeCodeKey { get; set; }

        /// <summary>
        /// Gets or sets the type code
        /// </summary>
        [AutoLoad, XmlIgnore, JsonIgnore, SerializationReference(nameof(TypeCodeKey))]
        public AuditTerm TypeCode {
            get
            {
                this.m_typeCode = base.DelayLoad(this.TypeCodeKey, this.m_typeCode);
                return this.m_typeCode;
            }
            set
            {
                this.m_typeCode = value;
                this.TypeCodeKey = value?.Key;
            }
        }


        /// <summary>
        /// Gets or sets the type code
        /// </summary>
        [AutoLoad, XmlIgnore, JsonIgnore, SerializationReference(nameof(RoleCodeKey))]
        public AuditTerm RoleCode
        {
            get
            {
                this.m_roleCode = base.DelayLoad(this.RoleCodeKey, this.m_roleCode);
                return this.m_roleCode;
            }
            set
            {
                this.m_roleCode = value;
                this.RoleCodeKey = value?.Key;
            }
        }

        /// <summary>
        /// Gets or sets the type code
        /// </summary>
        [AutoLoad, XmlIgnore, JsonIgnore, SerializationReference(nameof(LifecycleCodeKey))]
        public AuditTerm LifecycleCode
        {
            get
            {
                this.m_lifecycleCode = base.DelayLoad(this.LifecycleCodeKey, this.m_lifecycleCode);
                return this.m_lifecycleCode;
            }
            set
            {
                this.m_lifecycleCode = value;
                this.LifecycleCodeKey = value?.Key;
            }
        }

        /// <summary>
        /// Gets or sets the type code
        /// </summary>
        [AutoLoad, XmlIgnore, JsonIgnore, SerializationReference(nameof(IdTypeCodeKey))]
        public AuditTerm IdTypeCode
        {
            get
            {
                this.m_idTypeCode = base.DelayLoad(this.IdTypeCodeKey, this.m_idTypeCode);
                return this.m_idTypeCode;
            }
            set
            {
                this.m_idTypeCode = value;
                this.IdTypeCodeKey = value?.Key;
            }
        }

        /// <summary>
        /// Gets or sets the specifications
        /// </summary>
        [XmlElement("spec"), JsonProperty("spec")]
        public List<AuditObjectSpecification> Specification { get; set; }

        /// <summary>
        /// Gets or sets the audit details
        /// </summary>
        [XmlElement("detail"), JsonProperty("detail")]
        public List<AuditObjectDetail> Details { get; set; }

        /// <summary>
        /// Get modification time
        /// </summary>
        public override DateTimeOffset ModifiedOn => DateTimeOffset.Now;
    }
}