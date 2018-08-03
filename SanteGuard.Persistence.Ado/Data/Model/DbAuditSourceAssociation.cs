﻿using SanteDB.OrmLite.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Data.Model
{
    /// <summary>
    /// Audit source association
    /// </summary>
    [Table("aud_src_assoc_tbl")]
    public class DbAuditSourceAssociation : DbIdentified
    {
        /// <summary>
        /// Gets or sets the primary key
        /// </summary>
        [Column("assoc_id"), AutoGenerated, NotNull]
        public override Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the audit source key
        /// </summary>
        [Column("aud_src_id"), NotNull, ForeignKey(typeof(DbAuditSource), nameof(DbAuditSource.Key)), AlwaysJoin]
        public Guid AuditSourceKey { get; set; }

        /// <summary>
        /// Gets or sets the audit key
        /// </summary>
        [Column("aud_id"), NotNull, ForeignKey(typeof(DbAudit), nameof(DbAudit.Key))]
        public Guid AuditKey { get; set; }
    }
}
