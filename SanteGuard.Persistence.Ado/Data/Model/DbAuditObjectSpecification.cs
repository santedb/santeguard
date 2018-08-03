﻿using SanteDB.OrmLite.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Data.Model
{
    /// <summary>
    /// Represents an object specification
    /// </summary>
    [Table("aud_obj_spec_tbl")]
    public class DbAuditObjectSpecification : DbIdentified
    {

        /// <summary>
        /// Gets or sets the key 
        /// </summary>
        [Column("spec_id"), AutoGenerated, PrimaryKey]
        public override Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the object to which the spec is attached
        /// </summary>
        [Column("obj_id"), NotNull, ForeignKey(typeof(DbAuditObject), nameof(DbAuditObject.Key))]
        public Guid ObjectKey { get; set; }

        /// <summary>
        /// Gets or sets the specification
        /// </summary>
        [Column("spec"), NotNull]
        public String Specification { get; set; }

        /// <summary>
        /// Gets or sets the spec type
        /// </summary>
        [Column("spec_typ"), NotNull]
        public String SpecificationType { get; set; }
    }
}
