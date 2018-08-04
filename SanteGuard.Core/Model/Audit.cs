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
    /// Audit status type 
    /// </summary>
    [XmlType(nameof(AuditStatusType), Namespace = "http://santedb.org/santeguard")]
    public enum AuditStatusType
    {
        [XmlEnum("new")]
        New,
        [XmlEnum("active")]
        Active,
        [XmlEnum("held")]
        Held,
        [XmlEnum("nullified")]
        Nullified,
        [XmlEnum("obsolete")]
        Obsolete,
        [XmlEnum("archived")]
        Archived,
        [XmlEnum("system")]
        System
    }

    /// <summary>
    /// Represents an audit with versioning information
    /// </summary>
    [XmlType(nameof(Audit), Namespace = "http://santedb.org/santeguard")]
    [XmlRoot(nameof(Audit), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(Audit))]
    public class Audit : VersionedEntityData<Audit>
    {
        // Audit information
        public Audit()
        {
            this.EventTypeCodes = new List<AuditTerm>();
            this.Objects = new List<AuditObject>();
            this.Participants = new List<AuditParticipation>();
        }

        private AuditSession m_session = null;
        private AuditTerm m_actionCode = null;
        private AuditTerm m_outcomeCode = null;
        private AuditTerm m_eventCode = null;
        private AuditSource m_source = null;

        /// <summary>
        /// Correlation token
        /// </summary>
        [XmlElement("correlationToken"), JsonProperty("correlationToken")]
        public Guid CorrelationToken { get; set; }

        /// <summary>
        /// Gets or sets the action
        /// </summary>
        [XmlElement("action"), JsonProperty("action")]
        public Guid ActionCodeKey { get; set; }

        /// <summary>
        /// Gets or sets the outcome
        /// </summary>
        [XmlElement("outcome"), JsonProperty("outcome")]
        public Guid OutcomeCodeKey { get; set; }

        /// <summary>
        /// Gets or sets the event type
        /// </summary>
        [XmlElement("eventId"), JsonProperty("eventId")]
        public Guid EventIdCodeKey { get; set; }

        /// <summary>
        /// Gets or sets the source
        /// </summary>
        [XmlElement("source"), JsonProperty("source")]
        public Guid AuditSourceKey { get; set; }

        /// <summary>
        /// Gets or sets the event timestamp
        /// </summary>
        [XmlElement("eventTime"), JsonProperty("eventTime")]
        public DateTimeOffset EventTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the process name
        /// </summary>
        [XmlElement("processName"), JsonProperty("processName")]
        public string ProcessName { get; set; }

        /// <summary>
        /// Gets or sets the process id
        /// </summary>
        [XmlElement("processId"), JsonProperty("processId")]
        public String ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the audit status
        /// </summary>
        [XmlElement("status"), JsonProperty("status")]
        public AuditStatusType Status { get; set; }

        /// <summary>
        /// Gets or sets the alert status on this audit
        /// </summary>
        [XmlElement("isAlert"), JsonProperty("isAlert")]
        public bool IsAlert { get; set; }

        /// <summary>
        /// Gets or sets the session identifier
        /// </summary>
        [XmlElement("session"), JsonProperty("session")]
        public Guid? SessionKey { get; set; }

        /// <summary>
        /// Gets the audit session
        /// </summary>
        [SerializationReference(nameof(SessionKey)), XmlIgnore, JsonIgnore]
        public AuditSession Session
        {
            get
            {
                this.m_session = base.DelayLoad(this.SessionKey, this.m_session);
                return this.m_session;
            }
            set
            {
                this.SessionKey = value?.Key;
                this.m_session = value;
            }
        }

        /// <summary>
        /// Gets or sets the actual audit term
        /// </summary>
        [SerializationReference(nameof(ActionCodeKey)), XmlIgnore, JsonIgnore, AutoLoad]
        public AuditTerm ActionCode
        {
            get
            {
                this.m_actionCode = base.DelayLoad(this.ActionCodeKey, this.m_actionCode);
                return this.m_actionCode;
            }
            set
            {
                this.m_actionCode = value;
                this.ActionCodeKey = value.Key.Value;
            }
        }

        /// <summary>
        /// Gets or sets the actual audit term
        /// </summary>
        [SerializationReference(nameof(OutcomeCodeKey)), XmlIgnore, JsonIgnore, AutoLoad]
        public AuditTerm OutcomeCode
        {
            get
            {
                this.m_outcomeCode = base.DelayLoad(this.OutcomeCodeKey, this.m_outcomeCode);
                return this.m_outcomeCode;
            }
            set
            {
                this.m_outcomeCode = value;
                this.OutcomeCodeKey = value.Key.Value;
            }
        }

        /// <summary>
        /// Gets or sets the actual audit term
        /// </summary>
        [SerializationReference(nameof(EventIdCodeKey)), XmlIgnore, JsonIgnore, AutoLoad]
        public AuditTerm EventIdCode
        {
            get
            {
                this.m_eventCode = base.DelayLoad(this.EventIdCodeKey, this.m_eventCode);
                return this.m_eventCode;
            }
            set
            {
                this.m_eventCode = value;
                this.EventIdCodeKey = value.Key.Value;
            }
        }
        
        /// <summary>
        /// Gets or sets the participants
        /// </summary>
        [XmlElement("participant"), JsonProperty("participant"), AutoLoad]
        public List<AuditParticipation> Participants { get; set; }

        /// <summary>
        /// Gets or sets the objects
        /// </summary>
        [XmlElement("object"), JsonProperty("object")]
        public List<AuditObject> Objects { get; set; }

        /// <summary>
        /// Gets or sets the event type codes
        /// </summary>
        [XmlElement("eventType"), JsonProperty("eventType"), AutoLoad]
        public List<AuditTerm> EventTypeCodes { get; set; }

        /// <summary>
        /// Gets or sets the audit source key
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(AuditSourceKey))]
        public AuditSource AuditSource {
            get
            {
                this.m_source = base.DelayLoad(this.AuditSourceKey, this.m_source);
                return this.m_source;
            }
            set
            {
                this.m_source = value;
                this.AuditSourceKey = value.Key.Value;
            }
        }

        /// <summary>
        /// Gets additional details about the analysis of the object
        /// </summary>
        [AutoLoad, XmlElement("detail"), JsonProperty("detail")]
        public List<AuditDetailData> Details { get; set; }
    }
}
