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
        public Guid ActorKey { get; set; }

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
                this.ActorKey = value.Key.Value;
            }
        }
    }
}
