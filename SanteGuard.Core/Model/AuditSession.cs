using Newtonsoft.Json;
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
