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
using SanteDB.OrmLite;
using SanteGuard.Model;
using SanteGuard.Persistence.Ado.Data.Extensions;
using SanteGuard.Persistence.Ado.Data.Model;
using System;
using System.Linq;
using System.Security.Principal;

namespace SanteGuard.Persistence.Ado.Services.Persistence
{
    /// <summary>
    /// Persistence service for audit object
    /// </summary>
    public class AuditObjectPersistenceService : IdentifiedPersistenceService<AuditObject, DbAuditObject>
    {

        /// <summary>
        /// Insert the specified audit object into the datastore
        /// </summary>
        public override AuditObject InsertInternal(DataContext context, AuditObject data, IPrincipal principal)
        {
            if (data.IdTypeCode != null) data.IdTypeCode = data.IdTypeCode.EnsureExists(context, principal) as AuditTerm;
            if (data.LifecycleCode != null) data.LifecycleCode = data.LifecycleCode.EnsureExists(context, principal) as AuditTerm;
            if (data.RoleCode != null) data.RoleCode = data.RoleCode.EnsureExists(context, principal) as AuditTerm;
            if (data.TypeCode != null) data.TypeCode = data.TypeCode.EnsureExists(context, principal) as AuditTerm;
            data.IdTypeCodeKey = data.IdTypeCode?.Key ?? data.IdTypeCodeKey;
            data.LifecycleCodeKey = data.LifecycleCode?.Key ?? data.LifecycleCodeKey;
            data.RoleCodeKey = data.RoleCode?.Key ?? data.RoleCodeKey;
            data.TypeCodeKey = data.TypeCode?.Key ?? data.TypeCodeKey;

            var retVal = base.InsertInternal(context, data, principal);

            // Audit spec
            if (data.Specification != null)
                base.UpdateAssociatedItems<AuditObjectSpecification, DbAuditObjectSpecification>(
                    data.Specification,
                    retVal,
                    context,
                    principal);

            // Audit details
            if (data.Details != null)
                base.UpdateAssociatedItems<AuditObjectDetail, DbAuditObjectDetail>(
                    data.Details.Where(o=>!String.IsNullOrEmpty(o.Type) || o.Value.Length > 0),
                    retVal,
                    context,
                    principal);

            return retVal;
        }

        /// <summary>
        /// Update the audit object
        /// </summary>
        public override AuditObject UpdateInternal(DataContext context, AuditObject data, IPrincipal principal)
        {
            throw new InvalidOperationException("Audit Details Cannot be Updated");
        }
    }
}
