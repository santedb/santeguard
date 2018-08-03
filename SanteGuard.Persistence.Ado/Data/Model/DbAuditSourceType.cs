using SanteDB.OrmLite.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Data.Model
{
    /// <summary>
    /// Represents an audit source type
    /// </summary>
    [Table("aud_src_typ_tbl")]
    public class DbAuditSourceType
    {
        /// <summary>
        /// Gets or sets the audit source key
        /// </summary>
        [Column("aud_src_id"), NotNull, PrimaryKey, ForeignKey(typeof(DbAuditSource), nameof(DbAuditSource.Key))]
        public Guid AuditSourceKey { get; set; }

        /// <summary>
        /// Gets or sets the type code key
        /// </summary>
        [Column("cd_id"), NotNull, PrimaryKey, ForeignKey(typeof(DbAuditCode), nameof(DbAuditCode.Key)), AlwaysJoin]
        public Guid TypeCodeKey { get; set; }

    }
}
