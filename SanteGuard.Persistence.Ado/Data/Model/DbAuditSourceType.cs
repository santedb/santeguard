/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: justin
 * Date: 2018-10-27
 */
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
