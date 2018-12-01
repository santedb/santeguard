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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using System;
using System.Xml.Serialization;

namespace SanteGuard.Model
{

    /// <summary>
    /// Represents error data related to the parsing and storage of a santeguard audit
    /// </summary>
    [XmlType(nameof(AuditDetailData), Namespace = "http://santedb.org/santeguard")]
    [XmlRoot(nameof(AuditDetailData), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditDetailData))]
    public class AuditDetailData : Association<Audit>
    {
        // Caused by
        private AuditDetailData m_causedBy = null;

        /// <summary>
        /// Gets or sets the session identifier related to the audit
        /// </summary>
        [XmlElement("session"), JsonProperty("session")]
        public Guid SessionKey { get; set; }

        /// <summary>
        /// Gets or sets the priority of the issue
        /// </summary>
        [XmlElement("priority"), JsonProperty("priority")]
        public DetectedIssuePriorityType IssueType { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        [XmlElement("message"), JsonProperty("message")]
        public String Message { get; set; }

        /// <summary>
        /// Gets or sets the stack trace
        /// </summary>
        [XmlElement("stack"), JsonProperty("stack")]
        public String StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the error that caused this
        /// </summary>
        [XmlElement("causedBy"), JsonProperty("causedBy")]
        public Guid? CausedByKey { get; set; }

        /// <summary>
        /// Gets or sets the caused by key
        /// </summary>
        [SerializationReference(nameof(CausedByKey)), JsonIgnore, XmlIgnore]
        public AuditDetailData CausedBy {
            get
            {
                return base.DelayLoad<AuditDetailData>(this.CausedByKey, this.m_causedBy);
            }
            set
            {
                if (value == null) this.CausedByKey = null;
                else this.CausedByKey = value?.Key;
                this.m_causedBy = value;
            }
        }

        /// <summary>
        /// Get the time that the audit was modified
        /// </summary>
        public override DateTimeOffset ModifiedOn => DateTimeOffset.Now;
    }
}
