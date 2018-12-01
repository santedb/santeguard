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
    /// Gets or sets the audit session
    /// </summary>
    [XmlType(nameof(AuditSession), Namespace = "http://santedb.org/santeguard")]
    [XmlRoot(nameof(AuditSession), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditSession))]
    public class AuditSession : BaseEntityData
    {

        // receiver node
        private AuditNode m_receiverNode = null;
        // sender node
        private AuditNode m_senderNode = null;

        /// <summary>
        /// Gets or sets the id of the sender
        /// </summary>
        [XmlElement("sender"), JsonProperty("sender")]
        public Guid SenderKey { get; set; }

        /// <summary>
        /// Gets or sets the receiver of the audit
        /// </summary>
        [XmlElement("receiver"), JsonProperty("receiver")]
        public Guid ReceiverKey { get; set; }

        /// <summary>
        /// Gets or sets the sender data
        /// </summary>
        [SerializationReference(nameof(SenderKey)), XmlIgnore, JsonIgnore]
        public AuditNode Sender
        {
            get
            {
                this.m_senderNode = base.DelayLoad(this.SenderKey, this.m_senderNode);
                return this.m_senderNode;
            }
            set
            {
                this.m_senderNode = value;
                this.SenderKey = value.Key.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Gets or sets the sender data
        /// </summary>
        [SerializationReference(nameof(ReceiverKey)), XmlIgnore, JsonIgnore]
        public AuditNode Receiver
        {
            get
            {
                this.m_receiverNode = base.DelayLoad(this.ReceiverKey, this.m_receiverNode);
                return this.m_receiverNode;
            }
            set
            {
                this.m_receiverNode = value;
                this.ReceiverKey = value.Key.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Gets or sets the receiving endpoint
        /// </summary>
        [XmlElement("receivingEp"), JsonProperty("receivingEp")]
        public string ReceivingEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the senders endpoint
        /// </summary>
        [XmlElement("sendingEp"), JsonProperty("sendingEp")]
        public string SenderEndpoint { get; set; }
    }
}
