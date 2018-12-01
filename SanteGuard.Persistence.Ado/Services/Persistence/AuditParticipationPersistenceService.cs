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
using System.Security.Principal;

namespace SanteGuard.Persistence.Ado.Services.Persistence
{
    /// <summary>
    /// Represents a persistence service for audit participations
    /// </summary>
    public class AuditParticipationPersistenceService : IdentifiedPersistenceService<AuditParticipation, DbAuditParticipantAuditAssociation>
    {
        /// <summary>
        /// Insert the audit participation association
        /// </summary>
        public override AuditParticipation InsertInternal(DataContext context, AuditParticipation data, IPrincipal principal)
        {
            if (data.Actor != null) data.Actor = data.Actor.EnsureExists(context, principal) as AuditActor;
            data.ActorKey = data.Actor?.Key ?? data.ActorKey;

            var retVal = base.InsertInternal(context, data, principal);

            if (retVal.Roles != null)
                foreach (var r in retVal.Roles)
                    context.Insert(new DbAuditParticipantRoleAssocation()
                    {
                        AssociationKey = retVal.Key.Value,
                        RoleCodeKey = r.EnsureExists(context, principal).Key.Value
                    });

            return retVal;
        }

        /// <summary>
        /// Update the audit participant
        /// </summary>
        public override AuditParticipation UpdateInternal(DataContext context, AuditParticipation data, IPrincipal principal)
        {
            throw new InvalidOperationException("Audit Details Cannot be Updated");
        }
    }
}
