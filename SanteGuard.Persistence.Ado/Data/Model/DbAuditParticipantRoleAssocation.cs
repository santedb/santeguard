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
        public Guid AssociationKey { get; set; }

        /// <summary>
        /// The role 
        /// </summary>
        [Column("cd_id"), ForeignKey(typeof(DbAuditCode), nameof(DbAuditCode.Key)), NotNull, PrimaryKey, AlwaysJoin]
        public Guid RoleCodeKey { get; set; }
    }
}
