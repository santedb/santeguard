using MARC.HI.EHRS.SVC.Auditing.Data;
using MARC.HI.EHRS.SVC.Core;
using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Services;
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
    /// Represents audit version information
    /// </summary>
    [XmlType(nameof(AuditDataEx), Namespace = "http://santedb.org/santeguard")]
    [XmlRoot(nameof(AuditDataEx), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditDataEx))]
    public class AuditDataEx : BaseEntityData
    {
        // The backing audit data
        private AuditData m_audit = null;
        private AuditSession m_session = null;

        /// <summary>
        /// Gets or sets the audit key
        /// </summary>
        [XmlElement("audit"), JsonProperty("audit")]
        public Guid AuditKey { get; set; }

        /// <summary>
        /// Gets or sets the Audit data for the audit wrapper
        /// </summary>
        [SerializationReference(nameof(AuditKey)), JsonIgnore, XmlIgnore]
        public AuditData Audit
        {
            get
            {
                if (this.m_audit != null)
                    this.m_audit = ApplicationContext.Current.GetService<IAuditRepositoryService>().Get(this.AuditKey);
                return this.m_audit;
            }
            set
            {
                this.m_audit = value;
                this.AuditKey = value.CorrelationToken;
            }
        }

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
        /// Gets or sets the previous version key
        /// </summary>
        [XmlElement("previousVersion"), JsonProperty("previousVersion")]
        public Guid? PreviousVersionKey { get; set; }

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
        /// Gets or sets the process name
        /// </summary>
        [XmlElement("processName"), JsonProperty("processName")]
        public String ProcessName { get; set; }

        /// <summary>
        /// Gets or sets the process identifier
        /// </summary>
        [XmlElement("processId"), JsonProperty("processId")]
        public String ProcessId { get; set; }
    }
}
