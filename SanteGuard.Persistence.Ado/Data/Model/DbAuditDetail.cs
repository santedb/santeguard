﻿/*
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
    /// Represents audit detail
    /// </summary>
    [Table("dtl_id")]
    public class DbAuditDetail : DbIdentified
    {

        /// <summary>
        /// Gets or sets the primary key
        /// </summary>
        [Column("dtl_id"), AutoGenerated, PrimaryKey]
        public override Guid Key { get; set; }

        /// <summary>
        /// Getsr or sets the session key
        /// </summary>
        [Column("ses_id"), NotNull, ForeignKey(typeof(DbAuditSession), nameof(DbAuditSession.Key))]
        public Guid SessionKey { get; set; }

        /// <summary>
        /// Gets or sets the logging level
        /// </summary>
        [Column("level"), NotNull]
        public int IssueType { get; set; }

        /// <summary>
        /// Gets or sets the message
        /// </summary>
        [Column("msg"), NotNull]
        public String Message { get; set; }

        /// <summary>
        /// Gets or sets the audit key
        /// </summary>
        [Column("aud_id"), ForeignKey(typeof(DbAudit), nameof(DbAudit.Key))]
        public Guid AuditKey { get; set; }

        /// <summary>
        /// Gets or sets the stack trace if applicable
        /// </summary>
        [Column("stack")]
        public String StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the creation time
        /// </summary>
        [Column("crt_utc"), NotNull, AutoGenerated]
        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// Get the cause of this error
        /// </summary>
        [Column("caus_by_id"), NotNull, AutoGenerated]
        public Guid CausedByKey { get; set; }

        /// <summary>
        /// Indicates whether the detail is new or has been read
        /// </summary>
        [Column("is_new")]
        public Guid IsNew { get; set; }

    }
}
