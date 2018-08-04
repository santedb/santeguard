using MARC.HI.EHRS.SVC.Core;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteGuard.Persistence.Ado.Data.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Services.Persistence
{
    /// <summary>
    /// Generic persistnece service for identified entities
    /// </summary>
    public abstract class IdentifiedPersistenceService<TModel, TDomain> : IdentifiedPersistenceService<TModel, TDomain, TDomain>
        where TModel : IdentifiedData, new()
        where TDomain : class, IDbIdentified, new()
    { }

    /// <summary>
    /// Generic persistence service which can persist between two simple types.
    /// </summary>
    public abstract class IdentifiedPersistenceService<TModel, TDomain, TQueryResult> : CorePersistenceService<TModel, TDomain, TQueryResult>
        where TModel : IdentifiedData, new()
        where TDomain : class, IDbIdentified, new()
    {

        #region implemented abstract members of LocalDataPersistenceService


        /// <summary>
        /// Performthe actual insert.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="data">Data.</param>
        public override TModel InsertInternal(DataContext context, TModel data, IPrincipal principal)
        {
            try
            {
                var domainObject = this.FromModelInstance(data, context, principal) as TDomain;

                domainObject = context.Insert<TDomain>(domainObject);
                data.Key = domainObject.Key;

                return data;
            }
            catch (DbException ex)
            {
                this.m_tracer.TraceEvent(System.Diagnostics.TraceEventType.Error, ex.HResult, "Error inserting {0} - {1}", data, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Perform the actual update.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="data">Data.</param>
        public override TModel UpdateInternal(DataContext context, TModel data, IPrincipal principal)
        {
            try
            {
                // Sanity 
                if (data.Key == Guid.Empty)
                    throw new ConstraintException("NonIdentityUpdate");

                // Map and copy
                var newDomainObject = this.FromModelInstance(data, context, principal) as TDomain;
                var oldDomainObject = context.SingleOrDefault<TDomain>(o => o.Key == newDomainObject.Key);
                if (oldDomainObject == null)
                    throw new KeyNotFoundException(data.Key.ToString());

                oldDomainObject.CopyObjectData(newDomainObject);
                context.Update<TDomain>(oldDomainObject);
                return data;
            }
            catch (DbException ex)
            {
                this.m_tracer.TraceEvent(System.Diagnostics.TraceEventType.Error, ex.HResult, "Error updating {0} - {1}", data, ex.Message);
                throw;
            }

        }

        /// <summary>
        /// Performs the actual obsoletion
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="data">Data.</param>
        public override TModel ObsoleteInternal(DataContext context, TModel data, IPrincipal principal)
        {
            try
            {
                if (data.Key == Guid.Empty)
                    throw new ConstraintException("NonIdentityUpdate");

                var domainObject = context.FirstOrDefault<TDomain>(o => o.Key == data.Key);

                if (domainObject == null)
                    throw new KeyNotFoundException(data.Key.ToString());

                context.Delete(domainObject);

                return data;
            }
            catch (DbException ex)
            {
                this.m_tracer.TraceEvent(System.Diagnostics.TraceEventType.Error, ex.HResult, "Error obsoleting {0} - {1}", data, ex.Message);
                throw;
            }

        }

        /// <summary>
        /// Performs the actual query
        /// </summary>
        public override IEnumerable<TModel> QueryInternal(DataContext context, Expression<Func<TModel, bool>> query, Guid queryId, int offset, int? count, out int totalResults, IPrincipal principal, bool countResults = false)
        {
            return this.QueryInternalEx(context, query, queryId, offset, count, out totalResults, countResults).Select(o => o is Guid ? this.GetInternal(context, (Guid)o, principal) : this.CacheConvert(o, context, principal));
        }

        /// <summary>
        /// Get the specified object
        /// </summary>
        public override TModel GetInternal(DataContext context, Guid key, IPrincipal principal)
        {
            var cacheService = ApplicationContext.Current.GetService<IDataCachingService>();
            var retVal = cacheService?.GetCacheItem<TModel>(key) ??
                    context.GetCacheCommit(key) as TModel;
            if (retVal != null)
                return retVal;
            else
                return this.CacheConvert(context.FirstOrDefault<TDomain>(o => o.Key == key), context, principal);
        }

        #endregion
    }
}
