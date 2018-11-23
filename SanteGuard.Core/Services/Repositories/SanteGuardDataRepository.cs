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
using SanteDB.Core.Interfaces;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Attribute;
using SanteDB.Core.Services;
using SanteDB.Core.Services.Impl;
using SanteGuard.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Services.Repositories
{
    /// <summary>
    /// SanteGuard Data Repository
    /// </summary>
    [PolicyPermission(System.Security.Permissions.SecurityAction.Demand, PolicyId = PermissionPolicyIdentifiers.AccessAuditLog)]
    public class SanteGuardDataRepository : LocalEntityRepositoryServiceBase,
        IRepositoryService<Audit>,
        IRepositoryService<AuditActor>,
        IRepositoryService<AuditDetailData>,
        IRepositoryService<AuditNode>,
        IRepositoryService<AuditSession>,
        IRepositoryService<AuditTerm>,
        IRepositoryService<AuditSource>,
        IRepositoryService<AuditBundle>,
        ISecurityAuditEventSource
    {

        /// <summary>
        /// Ctor for sante
        /// </summary>
        public SanteGuardDataRepository()
        {
        }

        /// <summary>
        /// Security attributes have changed
        /// </summary>
        public event EventHandler<SecurityAuditDataEventArgs> SecurityAttributesChanged;
        /// <summary>
        /// Security resource was created
        /// </summary>
        public event EventHandler<SecurityAuditDataEventArgs> SecurityResourceCreated;
        /// <summary>
        /// Security resource was deleted
        /// </summary>
        public event EventHandler<SecurityAuditDataEventArgs> SecurityResourceDeleted;

        /// <summary>
        /// Find audit actor
        /// </summary>
        IEnumerable<AuditActor> IRepositoryService<AuditActor>.Find(Expression<Func<AuditActor, bool>> query)
        {
            int tr = 0;
            return base.Find(query, 0, 100, out tr, Guid.Empty);
        }

        /// <summary>
        /// Find audit actor
        /// </summary>
        IEnumerable<AuditActor> IRepositoryService<AuditActor>.Find(Expression<Func<AuditActor, bool>> query, int offset, int? count, out int totalResults)
        {
            return base.Find(query, offset, count, out totalResults, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit term
        /// </summary>
        IEnumerable<AuditTerm> IRepositoryService<AuditTerm>.Find(Expression<Func<AuditTerm, bool>> query)
        {
            int tr = 0;
            return base.Find(query, 0, 100, out tr, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit terms
        /// </summary>
        IEnumerable<AuditTerm> IRepositoryService<AuditTerm>.Find(Expression<Func<AuditTerm, bool>> query, int offset, int? count, out int totalResults)
        {
            return base.Find(query, offset, count, out totalResults, Guid.Empty);
        }

        /// <summary>
        /// Get audit actor
        /// </summary>
        AuditActor IRepositoryService<AuditActor>.Get(Guid key)
        {
            return base.Get<AuditActor>(key, Guid.Empty);
        }

        /// <summary>
        /// Get audit actor version
        /// </summary>
        AuditActor IRepositoryService<AuditActor>.Get(Guid key, Guid versionKey)
        {
            return base.Get<AuditActor>(key, versionKey);
        }

        /// <summary>
        /// Insert audit actor
        /// </summary>
        AuditActor IRepositoryService<AuditActor>.Insert(AuditActor data)
        {
            return base.Insert(data);
        }

        /// <summary>
        /// Insert audit term
        /// </summary>
        AuditTerm IRepositoryService<AuditTerm>.Insert(AuditTerm data)
        {
            return base.Insert(data);
        }

        /// <summary>
        /// Obsolete audit actor
        /// </summary>
        AuditActor IRepositoryService<AuditActor>.Obsolete(Guid key)
        {
            return base.Obsolete<AuditActor>(key);
        }

        /// <summary>
        /// Save audit actor
        /// </summary>
        AuditActor IRepositoryService<AuditActor>.Save(AuditActor data)
        {
            return base.Save(data);
        }

        /// <summary>
        /// Create or update the audit term
        /// </summary>
        AuditTerm IRepositoryService<AuditTerm>.Save(AuditTerm data)
        {
            return base.Save(data);
        }

        /// <summary>
        /// Find the specified extended audit data from the underlying persistence layer
        /// </summary>
        IEnumerable<Audit> IRepositoryService<Audit>.Find(Expression<Func<Audit, bool>> query)
        {
            int tr = 0;
            return base.Find(query, 0, 100, out tr, Guid.Empty);
        }

        /// <summary>
        /// Find the specified extended audit data
        /// </summary>
        IEnumerable<Audit> IRepositoryService<Audit>.Find(Expression<Func<Audit, bool>> query, int offset, int? count, out int totalResults)
        {
            return base.Find(query, offset, count, out totalResults, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit detail data
        /// </summary>
        IEnumerable<AuditDetailData> IRepositoryService<AuditDetailData>.Find(Expression<Func<AuditDetailData, bool>> query)
        {
            int tr = 0;
            return base.Find(query, 0, 100, out tr, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit detail data
        /// </summary>
        IEnumerable<AuditDetailData> IRepositoryService<AuditDetailData>.Find(Expression<Func<AuditDetailData, bool>> query, int offset, int? count, out int totalResults)
        {
            return base.Find(query, offset, count, out totalResults, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit node data
        /// </summary>
        IEnumerable<AuditNode> IRepositoryService<AuditNode>.Find(Expression<Func<AuditNode, bool>> query)
        {
            int tr = 0;
            return base.Find(query, 0, 100, out tr, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit node data
        /// </summary>
        IEnumerable<AuditNode> IRepositoryService<AuditNode>.Find(Expression<Func<AuditNode, bool>> query, int offset, int? count, out int totalResults)
        {
            return base.Find(query, offset, count, out totalResults, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit session information
        /// </summary>
        IEnumerable<AuditSession> IRepositoryService<AuditSession>.Find(Expression<Func<AuditSession, bool>> query)
        {
            int tr = 0;
            return base.Find(query, 0, 100, out tr, Guid.Empty);
        }

        /// <summary>
        /// Find the specified audit session information
        /// </summary>
        IEnumerable<AuditSession> IRepositoryService<AuditSession>.Find(Expression<Func<AuditSession, bool>> query, int offset, int? count, out int totalResults)
        {
            return base.Find(query, offset, count, out totalResults, Guid.Empty);
        }

        /// <summary>
        /// Get the specified extended audit data
        /// </summary>
        Audit IRepositoryService<Audit>.Get(Guid key)
        {
            return base.Get<Audit>(key, Guid.Empty);
        }

        /// <summary>
        /// Get the specified extended audit data
        /// </summary>
        Audit IRepositoryService<Audit>.Get(Guid key, Guid versionKey)
        {
            return base.Get<Audit>(key, versionKey);
        }

        /// <summary>
        /// Get the specified audit detail data
        /// </summary>
        AuditDetailData IRepositoryService<AuditDetailData>.Get(Guid key)
        {
            return base.Get<AuditDetailData>(key, Guid.Empty);
        }

        /// <summary>
        /// Get the specific version of the detail data
        /// </summary>
        AuditDetailData IRepositoryService<AuditDetailData>.Get(Guid key, Guid versionKey)
        {
            return base.Get<AuditDetailData>(key, versionKey);
        }

        /// <summary>
        /// Get the specified audit node
        /// </summary>
        AuditNode IRepositoryService<AuditNode>.Get(Guid key)
        {
            return base.Get<AuditNode>(key, Guid.Empty);
        }

        /// <summary>
        /// Get the specified version of the audit node
        /// </summary>
        AuditNode IRepositoryService<AuditNode>.Get(Guid key, Guid versionKey)
        {
            return base.Get<AuditNode>(key, versionKey);
        }

        /// <summary>
        /// Get the specified audit session
        /// </summary>
        AuditSession IRepositoryService<AuditSession>.Get(Guid key)
        {
            return base.Get<AuditSession>(key, Guid.Empty);
        }

        /// <summary>
        /// Get the specified version of the audit session
        /// </summary>
        AuditSession IRepositoryService<AuditSession>.Get(Guid key, Guid versionKey)
        {
            return base.Get<AuditSession>(key, versionKey);
        }

        /// <summary>
        /// Get the audit term
        /// </summary>
        AuditTerm IRepositoryService<AuditTerm>.Get(Guid key)
        {
            return base.Get<AuditTerm>(key, Guid.Empty);
        }

        /// <summary>
        /// Get the audit term
        /// </summary>
        AuditTerm IRepositoryService<AuditTerm>.Get(Guid key, Guid versionKey)
        {
            return base.Get<AuditTerm>(key, versionKey);
        }

        /// <summary>
        /// Insert the specified audit data
        /// </summary>
        Audit IRepositoryService<Audit>.Insert(Audit data)
        {
            return base.Insert(data);
        }

        /// <summary>
        /// Insert the specified detail data
        /// </summary>
        AuditDetailData IRepositoryService<AuditDetailData>.Insert(AuditDetailData data)
        {
            return base.Insert(data);
        }

        /// <summary>
        /// Insert the specified node data
        /// </summary>
        AuditNode IRepositoryService<AuditNode>.Insert(AuditNode data)
        {
            var retVal = base.Insert(data);
            this.SecurityResourceCreated?.Invoke(this, new SecurityAuditDataEventArgs(retVal));
            return retVal;
        }

        /// <summary>
        /// Insert the specified session data
        /// </summary>
        AuditSession IRepositoryService<AuditSession>.Insert(AuditSession data)
        {
            return base.Insert(data);
        }

        /// <summary>
        /// Obsolete the specified audit data
        /// </summary>
        Audit IRepositoryService<Audit>.Obsolete(Guid key)
        {
            var retVal = base.Obsolete<Audit>(key);
            this.SecurityResourceDeleted?.Invoke(this, new SecurityAuditDataEventArgs(retVal, "obsoleted"));
            return retVal;
        }

        /// <summary>
        /// Obsoletes the specified detail data
        /// </summary>
        AuditDetailData IRepositoryService<AuditDetailData>.Obsolete(Guid key)
        {
            return base.Obsolete<AuditDetailData>(key);
        }

        /// <summary>
        /// Obsoletes the specified node
        /// </summary>
        AuditNode IRepositoryService<AuditNode>.Obsolete(Guid key)
        {
            var retVal = base.Obsolete<AuditNode>(key);
            this.SecurityResourceDeleted?.Invoke(this, new SecurityAuditDataEventArgs(retVal, "obsoleted"));
            return retVal;
        }

        /// <summary>
        /// Obsoletes the specified session
        /// </summary>
        AuditSession IRepositoryService<AuditSession>.Obsolete(Guid key)
        {
            var retVal = base.Obsolete<AuditSession>(key);
            this.SecurityResourceDeleted?.Invoke(this, new SecurityAuditDataEventArgs(retVal, "obsoleted"));
            return retVal;
        }

        AuditTerm IRepositoryService<AuditTerm>.Obsolete(Guid key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save the specified audit data
        /// </summary>
        Audit IRepositoryService<Audit>.Save(Audit data)
        {
            var retVal = base.Save(data);
            this.SecurityAttributesChanged?.Invoke(this, new SecurityAuditDataEventArgs(retVal));
            return retVal;
        }

        /// <summary>
        /// Update or create the audit detail data
        /// </summary>
        AuditDetailData IRepositoryService<AuditDetailData>.Save(AuditDetailData data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Save the specified audit node
        /// </summary>
        AuditNode IRepositoryService<AuditNode>.Save(AuditNode data)
        {
            var retVal = base.Save(data);
            this.SecurityAttributesChanged?.Invoke(this, new SecurityAuditDataEventArgs(retVal));
            return retVal;
        }

        /// <summary>
        /// Save the specified session
        /// </summary>
        AuditSession IRepositoryService<AuditSession>.Save(AuditSession data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Get audit source by key
        /// </summary>
        AuditSource IRepositoryService<AuditSource>.Get(Guid key)
        {
            return base.Get<AuditSource>(key, Guid.Empty);
        }

        /// <summary>
        /// Get audit source version
        /// </summary>
        AuditSource IRepositoryService<AuditSource>.Get(Guid key, Guid versionKey)
        {
            return base.Get<AuditSource>(key, versionKey);

        }

        /// <summary>
        /// Find audit source
        /// </summary>
        IEnumerable<AuditSource> IRepositoryService<AuditSource>.Find(Expression<Func<AuditSource, bool>> query)
        {
            int tr = 0;
            return this.Find(query, 0, null, out tr, Guid.Empty);
        }

        /// <summary>
        /// Total results
        /// </summary>
        IEnumerable<AuditSource> IRepositoryService<AuditSource>.Find(Expression<Func<AuditSource, bool>> query, int offset, int? count, out int totalResults)
        {
            return this.Find(query, offset, count, out totalResults, Guid.Empty);
        }

        /// <summary>
        /// Insert the audit source
        /// </summary>
        AuditSource IRepositoryService<AuditSource>.Insert(AuditSource data)
        {
            var retVal = base.Insert(data);
            this.SecurityResourceCreated?.Invoke(this, new SecurityAuditDataEventArgs(data));
            return retVal;
        }

        /// <summary>
        /// Update audit source
        /// </summary>
        AuditSource IRepositoryService<AuditSource>.Save(AuditSource data)
        {
            var retVal = base.Save(data);
            this.SecurityAttributesChanged?.Invoke(this, new SecurityAuditDataEventArgs(data));
            return retVal;
        }

        /// <summary>
        /// Obsolete audit source
        /// </summary>
        AuditSource IRepositoryService<AuditSource>.Obsolete(Guid key)
        {
            throw new NotSupportedException();
        }

        AuditBundle IRepositoryService<AuditBundle>.Get(Guid key)
        {
            throw new NotSupportedException();

        }

        AuditBundle IRepositoryService<AuditBundle>.Get(Guid key, Guid versionKey)
        {
            throw new NotSupportedException();

        }

        IEnumerable<AuditBundle> IRepositoryService<AuditBundle>.Find(Expression<Func<AuditBundle, bool>> query)
        {
            throw new NotSupportedException();

        }

        IEnumerable<AuditBundle> IRepositoryService<AuditBundle>.Find(Expression<Func<AuditBundle, bool>> query, int offset, int? count, out int totalResults)
        {
            throw new NotSupportedException();
        }

        AuditBundle IRepositoryService<AuditBundle>.Insert(AuditBundle data)
        {
            return base.Insert(data);
        }

        AuditBundle IRepositoryService<AuditBundle>.Save(AuditBundle data)
        {
            return base.Save(data);
        }

        AuditBundle IRepositoryService<AuditBundle>.Obsolete(Guid key)
        {
            throw new NotSupportedException();
        }
    }
}
