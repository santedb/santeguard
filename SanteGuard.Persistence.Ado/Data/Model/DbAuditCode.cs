﻿using SanteDB.OrmLite.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Data.Model
{
    /// <summary>
    /// Represents an audit code
    /// </summary>
    [Table("aud_cd_tbl")]
    public class DbAuditCode : DbIdentified
    {
        /// <summary>
        /// Gets or sets the key
        /// </summary>
        [PrimaryKey, Column("cd_id"), AutoGenerated]
        public override Guid Key { get; set; }

        /// <summary>
        /// Gets or set sthe code
        /// </summary>
        [Column("mnemonic"), NotNull]
        public String Mnemonic { get; set; }

        /// <summary>
        /// Gets or sets the code system
        /// </summary>
        [Column("domain"), NotNull]
        public String Domain { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        [Column("display")]
        public String DisplayName { get; set; }
    }
}