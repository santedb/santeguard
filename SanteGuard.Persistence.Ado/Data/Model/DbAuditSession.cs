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
    /// Represents an audit connection to the server
    /// </summary>
    [Table("aud_ses_tbl")]
    public class DbAuditSession : DbBaseData
    {
        /// <summary>
        /// Gets the primary key for the session
        /// </summary>
        [PrimaryKey, Column("ses_id")]
        public override Guid Key { get; set; }

        /// <summary>
        /// Gets the receiver key
        /// </summary>
        [Column("rcv_node_id"), ForeignKey(typeof(DbAuditNode), nameof(DbAuditNode.Key)), NotNull]
        public Guid ReceiverKey { get; set; }

        /// <summary>
        /// Gets or sets the sender key
        /// </summary>
        [Column("snd_node_id"), ForeignKey(typeof(DbAuditNode), nameof(DbAuditNode.Key)), NotNull]
        public Guid SenderKey { get; set; }

        /// <summary>
        /// Gets or sets the receiving endpoint
        /// </summary>
        [Column("rcv_ep"), NotNull]
        public String ReceivingEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the sender endpoint
        /// </summary>
        [Column("snd_ep"), NotNull]
        public String SenderEndpoint { get; set; }
    }
}
