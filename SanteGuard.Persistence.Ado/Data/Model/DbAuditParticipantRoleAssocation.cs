using SanteDB.OrmLite.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Data.Model
{
    /// <summary>
    /// Represents the audit participant role 
    /// </summary>
    [Table("aud_ptcpt_rol_cd_assoc_tbl")]
    public class DbAuditParticipantRoleAssocation 
    {

      
        /// <summary>
        /// Reference to the association
        /// </summary>
        [Column("assoc_id"), ForeignKey(typeof(DbAuditParticipantAuditAssociation), nameof(DbAuditParticipantAuditAssociation.Key)), PrimaryKey]
        public Guid ParticipantKey { get; set; }

        /// <summary>
        /// The role 
        /// </summary>
        [Column("cd_id"), ForeignKey(typeof(DbAuditCode), nameof(DbAuditCode.Key)), NotNull, PrimaryKey, AlwaysJoin]
        public Guid RoleCodeKey { get; set; }
    }
}
