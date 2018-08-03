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
 * User: fyfej
 * Date: 2017-9-1
 */
using SanteDB.OrmLite.Attributes;
using SanteDB.Persistence.Data.ADO.Data.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Data.Model
{
    /// <summary>
    /// Base data interface
    /// </summary>
    public interface IDbBaseData : IDbIdentified
    {
        /// <summary>
        /// Gets or sets the entity id which created this
        /// </summary>
        Guid CreatedByKey { get; set; }
        /// <summary>
        /// Gets or sets the id which obsoleted this
        /// </summary>
        Guid? ObsoletedByKey { get; set; }
        /// <summary>
        /// Gets or sets the creation time
        /// </summary>
        DateTimeOffset CreationTime { get; set; }
        /// <summary>
        /// Gets or sets the obsoletion time
        /// </summary>
        DateTimeOffset? ObsoletionTime { get; set; }
    }

    /// <summary>
    /// Represents non-versioendd base data
    /// </summary>
    public interface IDbNonVersionedBaseData : IDbBaseData
    {
        /// <summary>
        /// Gets or sets the updated user
        /// </summary>
        Guid? UpdatedByKey { get; set; }

        /// <summary>
        /// Gets or sets the time of updating
        /// </summary>
        DateTimeOffset? UpdatedTime { get; set; }
    }

    /// <summary>
    /// Represents base data
    /// </summary>
    public abstract class DbBaseData : DbIdentified, IDbBaseData
    {
        /// <summary>
        /// Gets or sets the entity id which created this
        /// </summary>
        [Column("crt_usr_id"), ForeignKey(typeof(DbSecurityUser), nameof(DbSecurityUser.Key))]
        public Guid CreatedByKey { get; set; }
        /// <summary>
        /// Gets or sets the id which obsoleted this
        /// </summary>
        [Column("obslt_usr_id"), ForeignKey(typeof(DbSecurityUser), nameof(DbSecurityUser.Key))]
        public Guid? ObsoletedByKey { get; set; }
        /// <summary>
        /// Gets or sets the creation time
        /// </summary>
        [Column("crt_utc"), AutoGenerated]
        public DateTimeOffset CreationTime { get; set; }
        /// <summary>
        /// Gets or sets the obsoletion time
        /// </summary>
        [Column("obslt_utc")]
        public DateTimeOffset? ObsoletionTime { get; set; }
    }

    /// <summary>
    /// Non-versioned base data
    /// </summary>
    public abstract class DbNonVersionedBaseData : DbBaseData, IDbNonVersionedBaseData
    {

        /// <summary>
        /// Gets or sets the updated user
        /// </summary>
        [Column("upd_usr_id"), ForeignKey(typeof(DbSecurityUser), nameof(DbSecurityUser.Key))]
        public Guid? UpdatedByKey { get; set; }

        /// <summary>
        /// Gets or sets the time of updating
        /// </summary>
        [Column("upd_utc")]
        public DateTimeOffset? UpdatedTime { get; set; }
    }
    
}
