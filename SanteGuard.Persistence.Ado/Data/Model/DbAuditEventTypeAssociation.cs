using SanteDB.OrmLite.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Data.Model
{
    /// <summary>
    /// Represents the audit event type association
    /// </summary>
    [Table("aud_evt_typ_cd_assoc_tbl")]
    public class DbAuditEventTypeAssociation
    {
        /// <summary>
        /// Gets or sets the audit key
        /// </summary>
        [Column("aud_id"), NotNull, ForeignKey(typeof(DbAudit), nameof(DbAudit.Key)), PrimaryKey]
        public Guid AuditKey { get; set; }

        /// <summary>
        /// Type code key
        /// </summary>
        [Column("cd_id"), NotNull, PrimaryKey, ForeignKey(typeof(DbAuditCode), nameof(DbAuditCode.Key)), AlwaysJoin]
        public Guid TypeCodeKey { get; set; }

    }
}
