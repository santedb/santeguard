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
using SanteDB.Core;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteGuard.Persistence.Ado.Data.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;

namespace SanteGuard.Persistence.Ado.Services.Persistence
{
    /// <summary>
    /// Base persistence service
    /// </summary>
    public abstract class BaseDataPersistenceService<TModel, TDomain> : BaseDataPersistenceService<TModel, TDomain, TDomain>
        where TModel : BaseEntityData, new()
        where TDomain : class, IDbBaseData, new()
    { }

    /// <summary>
    /// Base data persistence service
    /// </summary>
    public abstract class BaseDataPersistenceService<TModel, TDomain, TQueryResult> : IdentifiedPersistenceService<TModel, TDomain, TQueryResult>
        where TModel : BaseEntityData, new()
        where TDomain : class, IDbBaseData, new()
    {

        /// <summary>
        /// Performthe actual insert.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="data">Data.</param>
        public override TModel InsertInternal(DataContext context, TModel data, IPrincipal principal)
        {
            data.CreatedByKey = data.CreatedBy?.Key ?? data.CreatedByKey;

            // HACK: For now, modified on can only come from one property, some non-versioned data elements are bound on UpdatedTime
            var nvd = data as NonVersionedEntityData;
            if (nvd != null)
            {
                nvd.UpdatedByKey = nvd.UpdatedBy?.Key ?? nvd.UpdatedByKey ?? ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>().GetUser(principal.Identity.Name)?.Key;
                nvd.UpdatedTime = DateTimeOffset.Now;
            }

            if (data.CreationTime == DateTimeOffset.MinValue || data.CreationTime.Year < 100)
                data.CreationTime = DateTimeOffset.Now;

            var domainObject = this.FromModelInstance(data, context, principal) as TDomain;

            // Ensure created by exists
            data.CreatedByKey = domainObject.CreatedByKey = context.ContextId;
            domainObject = context.Insert<TDomain>(domainObject);
            data.CreationTime = (DateTimeOffset)domainObject.CreationTime;
            data.Key = domainObject.Key;
            return data;

        }

        /// <summary>
        /// Perform the actual update.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="data">Data.</param>
        public override TModel UpdateInternal(DataContext context, TModel data, IPrincipal principal)
        {
            var nvd = data as NonVersionedEntityData;
            if (nvd != null)
                nvd.UpdatedByKey = nvd.UpdatedBy?.Key ?? nvd.UpdatedByKey ?? ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>().GetUser(principal.Identity.Name)?.Key;

            // Check for key
            if (data.Key == Guid.Empty)
                throw new ConstraintException("NonIdentityUpdate");

            // Get current object
            var domainObject = this.FromModelInstance(data, context, principal) as TDomain;
            var currentObject = context.FirstOrDefault<TDomain>(o => o.Key == data.Key);
            // Not found
            if (currentObject == null)
                throw new KeyNotFoundException(data.Key.ToString());

            // VObject
            var vobject = domainObject as IDbNonVersionedBaseData;
            if (vobject != null)
            {
                nvd.UpdatedByKey = vobject.UpdatedByKey = ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>().GetUser(principal.Identity.Name)?.Key;
                nvd.UpdatedTime = vobject.UpdatedTime = DateTimeOffset.Now;
            }

            if (currentObject.CreationTime == domainObject.CreationTime) // HACK: Someone keeps passing up the same data so we have to correct here
                domainObject.CreationTime = DateTimeOffset.Now;

            currentObject.CopyObjectData(domainObject);
            currentObject = context.Update<TDomain>(currentObject);

            return data;
        }

        /// <summary>
        /// Query the specified object ordering by creation time
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<TModel> QueryInternal(DataContext context, Expression<Func<TModel, bool>> query, Guid queryId, int offset, int? count, out int totalResults, IPrincipal principal, bool countResults, ModelSort<TModel>[] orderBy)
        {
            var qresult = this.QueryInternalEx(context, query, queryId, offset, count, out totalResults, countResults, orderBy);
            return qresult.Select(o => o is Guid ? this.GetInternal(context, (Guid)o, principal) : this.CacheConvert(o, context, principal)).ToList();
        }

        /// <summary>
        /// Performs the actual obsoletion
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="data">Data.</param>
        public override TModel ObsoleteInternal(DataContext context, TModel data, IPrincipal principal)
        {
            if (data.Key == Guid.Empty)
                throw new ConstraintException("NonIdentityUpdate");


            // Current object
            var currentObject = context.FirstOrDefault<TDomain>(o => o.Key == data.Key);
            if (currentObject == null)
                throw new KeyNotFoundException(data.Key.ToString());

            //data.ObsoletedBy?.EnsureExists(context, principal);
            data.ObsoletedByKey = currentObject.ObsoletedByKey = ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>().GetUser(principal.Identity.Name)?.Key;
            data.ObsoletionTime = currentObject.ObsoletionTime = currentObject.ObsoletionTime ?? DateTimeOffset.Now;

            context.Update(currentObject);
            return data;
        }


    }
}
