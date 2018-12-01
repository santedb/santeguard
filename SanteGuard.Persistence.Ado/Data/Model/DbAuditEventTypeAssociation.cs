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

namespace SanteGuard.Persistence.Ado.Data.Model
{
    /// <summary>
    /// Represents the audit event type association
    /// </summary>
    [Table("aud_evt_typ_cd_assoc_tbl")]
    public class DbAuditEventTypeAssociation : DbAssociation
    {

        /// <summary>
        /// Key is ignored
        /// </summary>
        public override Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the audit key
        /// </summary>
        [Column("aud_id"), NotNull, ForeignKey(typeof(DbAudit), nameof(DbAudit.Key)), PrimaryKey]
        public override Guid SourceKey { get; set; }

        /// <summary>
        /// Type code key
        /// </summary>
        [Column("cd_id"), NotNull, PrimaryKey, ForeignKey(typeof(DbAuditCode), nameof(DbAuditCode.Key)), AlwaysJoin]
        public Guid TypeCodeKey { get; set; }

    }
}
