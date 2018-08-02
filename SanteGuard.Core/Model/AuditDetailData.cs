using MARC.HI.EHRS.SVC.Auditing.Data;
using Newtonsoft.Json;
using SanteDB.Core;
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
    /// Represents error data related to the parsing and storage of a santeguard audit
    /// </summary>
    [XmlType(nameof(AuditDetailData), Namespace = "http://santedb.org/santeguard")]
    [XmlRoot(nameof(AuditDetailData), Namespace = "http://santedb.org/santeguard")]
    [JsonObject(nameof(AuditDetailData))]
    public class AuditDetailData : IdentifiedData
    {
        // Caused by
        private AuditDetailData m_causedBy = null;
        // Audit
        private AuditData m_audit = null;

        /// <summary>
        /// Gets or sets the session identifier related to the audit
        /// </summary>
        [XmlElement("session"), JsonProperty("session")]
        public Guid SessionKey { get; set; }

        /// <summary>
        /// Gets or sets the priority of the issue
        /// </summary>
        [XmlElement("priority"), JsonProperty("priority")]
        public DetectedIssuePriorityType Priority { get; set; }

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
        /// Gets or sets the key of the audit which caused this error
        /// </summary>
        [XmlElement("audit"), JsonProperty("audit")]
        public Guid? AuditKey { get; set; }

        /// <summary>
        /// Gets or sets the audit that caused this error
        /// </summary>
        [SerializationReference(nameof(AuditKey)), JsonIgnore, XmlIgnore]
        public AuditData Audit
        {
            get
            {
                if (this.m_audit == null && this.AuditKey.HasValue)
                    this.m_audit = ApplicationServiceContext.Current.GetSerivce<IAuditRepositoryService>().Get(this.AuditKey);
                return this.m_audit;
            }
            set
            {
                this.m_audit = value;
                this.AuditKey = value.CorrelationToken;
            }
        }

        /// <summary>
        /// Get the time that the audit was modified
        /// </summary>
        public override DateTimeOffset ModifiedOn => DateTimeOffset.Now;
    }
}
