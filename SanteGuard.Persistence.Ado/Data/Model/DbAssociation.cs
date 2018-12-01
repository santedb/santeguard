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
using System;

namespace SanteGuard.Persistence.Ado.Data.Model
{

    /// <summary>
    /// Database association
    /// </summary>
    public interface IDbAssociation
    {
        /// <summary>
        /// Gets or sets the source of the association
        /// </summary>
        Guid SourceKey { get; set; }
    }

    /// <summary>
    /// Represents the databased associated entity
    /// </summary>
    public abstract class DbAssociation : DbIdentified, IDbAssociation
    {
        /// <summary>
        /// Gets or sets the key of the item associated with this object
        /// </summary>
        public abstract Guid SourceKey { get; set; }

    }
}
