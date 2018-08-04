﻿using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Core.Data;
using MARC.HI.EHRS.SVC.Core.Services;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteGuard.Persistence.Ado.Data.Extensions;
using SanteGuard.Persistence.Ado.Data.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Services.Persistence
{
    /// <summary>
    /// Core persistence service which contains helpful functions
    /// </summary>
    public abstract class CorePersistenceService<TModel, TDomain, TQueryReturn> : AuditPersistenceServiceBase<TModel>
        where TModel : IdentifiedData, new()
        where TDomain : class, new()
    {

        // Query persistence
        protected SanteDB.Core.Services.IQueryPersistenceService m_queryPersistence = ApplicationContext.Current.GetService<SanteDB.Core.Services.IQueryPersistenceService>();

        /// <summary>
        /// Get the order by function
        /// </summary>
        protected virtual SqlStatement AppendOrderBy(SqlStatement rawQuery)
        {
            return rawQuery;
        }

        /// <summary>
        /// Maps the data to a model instance
        /// </summary>
        /// <returns>The model instance.</returns>
        /// <param name="dataInstance">Data instance.</param>
        public override TModel ToModelInstance(object dataInstance, DataContext context, IPrincipal principal)
        {
            var dInstance = (dataInstance as CompositeResult)?.Values.OfType<TDomain>().FirstOrDefault() ?? dataInstance as TDomain;
            var retVal = this.m_mapper.MapDomainInstance<TDomain, TModel>(dInstance);
            retVal.LoadAssociations(context, principal);
            this.m_tracer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "Model instance {0} created", dataInstance);

            return retVal;
        }

        /// <summary>
		/// Performthe actual insert.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="data">Data.</param>
		public override TModel InsertInternal(DataContext context, TModel data, IPrincipal principal)
        {
            var domainObject = this.FromModelInstance(data, context, principal) as TDomain;

            domainObject = context.Insert<TDomain>(domainObject);

            if (domainObject is IDbIdentified)
                data.Key = (domainObject as IDbIdentified)?.Key;
            //data.CopyObjectData(this.ToModelInstance(domainObject, context, principal));
            //data.Key = domainObject.Key
            return data;
        }

        /// <summary>
        /// Perform the actual update.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="data">Data.</param>
        public override TModel UpdateInternal(DataContext context, TModel data, IPrincipal principal)
        {
            // Sanity 
            if (data.Key == Guid.Empty)
                throw new ConstraintException("Non Identity Update");

            // Map and copy
            var newDomainObject = this.FromModelInstance(data, context, principal) as TDomain;
            context.Update<TDomain>(newDomainObject);
            return data;
        }

        /// <summary>
        /// Performs the actual obsoletion
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="data">Data.</param>
        public override TModel ObsoleteInternal(DataContext context, TModel data, IPrincipal principal)
        {
            if (data.Key == Guid.Empty)
                throw new ConstraintException("Non Identity Update");

            context.Delete<TDomain>((TDomain)this.FromModelInstance(data, context, principal));

            return data;
        }

        /// <summary>
        /// O
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        /// <param name="queryId"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="totalResults"></param>
        /// <param name="principal"></param>
        /// <param name="countResults"></param>
        /// <returns></returns>
        public override IEnumerable<TModel> QueryInternal(DataContext context, Expression<Func<TModel, bool>> query, Guid queryId, int offset, int? count, out int totalResults, IPrincipal principal, bool countResults = true)
        {
            int resultCount = 0;
            var results = this.QueryInternalEx(context, query, queryId, offset, count, out resultCount, countResults).ToList();
            totalResults = resultCount;

            return results.AsParallel().Select(o =>
            {
                var subContext = context;
                var newSubContext = results.Count() > 1;

                try
                {
                    if (newSubContext) subContext = subContext.OpenClonedContext();

                    if (o is Guid)
                        return this.GetInternal(subContext, (Guid)o, principal);
                    else
                        return this.CacheConvert(o, subContext, principal);
                }
                catch (Exception e)
                {
                    this.m_tracer.TraceError("Error performing sub-query: {0}", e);
                    throw;
                }
                finally
                {
                    if (newSubContext)
                        subContext.Dispose();
                }
            });
        }

        /// <summary>
        /// Perform the query 
        /// </summary>
        protected virtual IEnumerable<Object> QueryInternalEx(DataContext context, Expression<Func<TModel, bool>> query, Guid queryId, int offset, int? count, out int totalResults, bool incudeCount = true)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif

            SqlStatement domainQuery = null;
            try
            {

                // Query has been registered?
                if (queryId != Guid.Empty && this.m_queryPersistence?.IsRegistered(queryId) == true)
                {
                    totalResults = (int)this.m_queryPersistence.QueryResultTotalQuantity(queryId);
                    var resultKeys = this.m_queryPersistence.GetQueryResults(queryId, offset, count.Value);
                    return resultKeys.OfType<Object>();
                }

                // Is obsoletion time already specified?
                if (!query.ToString().Contains("ObsoletionTime") && typeof(BaseEntityData).IsAssignableFrom(typeof(TModel)))
                {
                    var obsoletionReference = Expression.MakeBinary(ExpressionType.Equal, Expression.MakeMemberAccess(query.Parameters[0], typeof(TModel).GetProperty(nameof(BaseEntityData.ObsoletionTime))), Expression.Constant(null));
                    query = Expression.Lambda<Func<TModel, bool>>(Expression.MakeBinary(ExpressionType.AndAlso, obsoletionReference, query.Body), query.Parameters);
                }

                // Domain query
                domainQuery = context.CreateSqlStatement<TDomain>().SelectFrom();
                var expression = m_mapper.MapModelExpression<TModel, TDomain>(query, false);
                if (expression != null)
                {
                    Type lastJoined = typeof(TDomain);
                    if (typeof(CompositeResult).IsAssignableFrom(typeof(TQueryReturn)))
                        foreach (var p in typeof(TQueryReturn).GenericTypeArguments.Select(o => AdoAuditPersistenceService.GetMapper().MapModelType(o)))
                            if (p != typeof(TDomain))
                            {
                                // Find the FK to join
                                domainQuery.InnerJoin(lastJoined, p);
                                lastJoined = p;
                            }

                    domainQuery.Where<TDomain>(expression);
                }
                else
                {
                    m_tracer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "Will use slow query construction due to complex mapped fields");
                    domainQuery = AdoAuditPersistenceService.GetQueryBuilder().CreateQuery(query);
                }

                // Count = 0 means we're not actually fetching anything so just hit the db
                if (count != 0)
                {
                    domainQuery = this.AppendOrderBy(domainQuery);

                    // Query id just get the UUIDs in the db
                    if (queryId != Guid.Empty && count != 0)
                    {
                        ColumnMapping pkColumn = null;
                        if (typeof(CompositeResult).IsAssignableFrom(typeof(TQueryReturn)))
                        {
                            foreach (var p in typeof(TQueryReturn).GenericTypeArguments.Select(o => AdoAuditPersistenceService.GetMapper().MapModelType(o)))
                                if (!typeof(IDbVersionedData).IsAssignableFrom(p))
                                {
                                    pkColumn = TableMapping.Get(p).Columns.SingleOrDefault(o => o.IsPrimaryKey);
                                    break;
                                }
                        }
                        else
                            pkColumn = TableMapping.Get(typeof(TQueryReturn)).Columns.SingleOrDefault(o => o.IsPrimaryKey);

                        var keyQuery = AdoAuditPersistenceService.GetQueryBuilder().CreateQuery(query, pkColumn).Build();

                        var resultKeys = context.Query<Guid>(keyQuery.Build());

                        //ApplicationContext.Current.GetService<IThreadPoolService>().QueueNonPooledWorkItem(a => this.m_queryPersistence?.RegisterQuerySet(queryId.ToString(), resultKeys.Select(o => new Identifier<Guid>(o)).ToArray(), query), null);
                        // Another check
                        this.m_queryPersistence?.RegisterQuerySet(queryId, resultKeys.Take(1000).ToArray(), query, resultKeys.Count());

                        ApplicationContext.Current.GetService<IThreadPoolService>().QueueNonPooledWorkItem(o =>
                        {
                            int ofs = 1000;
                            var rkeys = o as Guid[];
                            while (ofs < rkeys.Length)
                            {
                                this.m_queryPersistence?.AddResults(queryId, rkeys.Skip(ofs).Take(1000).ToArray());
                                ofs += 1000;
                            }
                        }, resultKeys.ToArray());

                        if (incudeCount)
                            totalResults = (int)resultKeys.Count();
                        else
                            totalResults = 0;

                        var retVal = resultKeys.Skip(offset);
                        if (count.HasValue)
                            retVal = retVal.Take(count.Value);
                        return retVal.OfType<Object>();
                    }
                    else if (incudeCount)
                    {
                        totalResults = context.Count(domainQuery);
                        if (totalResults == 0)
                            return new List<Object>();
                    }
                    else
                        totalResults = 0;

                    if (offset > 0)
                        domainQuery.Offset(offset);
                    if (count.HasValue)
                        domainQuery.Limit(count.Value);

                    return this.DomainQueryInternal<TQueryReturn>(context, domainQuery, ref totalResults).OfType<Object>();
                }
                else
                {
                    totalResults = context.Count(domainQuery);
                    return new List<Object>();
                }
            }
            catch (Exception ex)
            {
                if (domainQuery != null)
                    this.m_tracer.TraceEvent(TraceEventType.Error, ex.HResult, context.GetQueryLiteral(domainQuery.Build()));
                context.Dispose(); // No longer important

                throw;
            }
#if DEBUG
            finally
            {
                sw.Stop();
            }
#endif
        }

        /// <summary>
        /// Perform a domain query
        /// </summary>
        protected IEnumerable<TResult> DomainQueryInternal<TResult>(DataContext context, SqlStatement domainQuery, ref int totalResults)
        {

            // Build and see if the query already exists on the stack???
            domainQuery = domainQuery.Build();
            var cachedQueryResults = context.CacheQuery(domainQuery);
            if (cachedQueryResults != null)
            {
                totalResults = cachedQueryResults.Count();
                return cachedQueryResults.OfType<TResult>();
            }

            var results = context.Query<TResult>(domainQuery).ToList();

            // Cache query result
            context.AddQuery(domainQuery, results.OfType<Object>());
            return results;

        }


        /// <summary>
        /// Build source query
        /// </summary>
        protected Expression<Func<TAssociation, bool>> BuildSourceQuery<TAssociation>(Guid sourceId) where TAssociation : ISimpleAssociation
        {
            return o => o.SourceEntityKey == sourceId;
        }

        /// <summary>
        /// Build source query
        /// </summary>
        protected Expression<Func<TAssociation, bool>> BuildSourceQuery<TAssociation>(Guid sourceId, decimal? versionSequenceId) where TAssociation : IVersionedAssociation
        {
            if (versionSequenceId == null)
                return o => o.SourceEntityKey == sourceId && o.ObsoleteVersionSequenceId == null;
            else
                return o => o.SourceEntityKey == sourceId && o.EffectiveVersionSequenceId <= versionSequenceId && (o.ObsoleteVersionSequenceId == null || o.ObsoleteVersionSequenceId > versionSequenceId);
        }

        /// <summary>
        /// Tru to load from cache
        /// </summary>
        protected virtual TModel CacheConvert(Object o, DataContext context, IPrincipal principal)
        {
            if (o == null) return null;

            var cacheService =ApplicationContext.Current.GetService<IDataCachingService>();

            var idData = (o as CompositeResult)?.Values.OfType<IDbIdentified>().FirstOrDefault() ?? o as IDbIdentified;
            var objData = (o as CompositeResult)?.Values.OfType<IDbBaseData>().FirstOrDefault() ?? o as IDbBaseData;
            if (objData?.ObsoletionTime != null || idData == null || idData.Key == Guid.Empty)
                return this.ToModelInstance(o, context, principal);
            else
            {
                var cacheItem = cacheService?.GetCacheItem<TModel>(idData?.Key ?? Guid.Empty) as TModel ??
                    context.GetCacheCommit(idData?.Key ?? Guid.Empty) as TModel;
                if (cacheItem != null)
                {
                    if (cacheItem.LoadState < context.LoadState)
                    {
                        cacheItem.LoadAssociations(context, principal);
                        cacheService?.Add(cacheItem);
                    }
                    return cacheItem;
                }
                else
                {
                    cacheItem = this.ToModelInstance(o, context, principal);
                    if (context.Transaction == null)
                        cacheService?.Add(cacheItem);
                }
                return cacheItem;
            }
        }

        /// <summary>
        /// Froms the model instance.
        /// </summary>
        /// <returns>The model instance.</returns>
        /// <param name="modelInstance">Model instance.</param>
        /// <param name="context">Context.</param>
        public override object FromModelInstance(TModel modelInstance, DataContext context, IPrincipal princpal)
        {
            return m_mapper.MapModelInstance<TModel, TDomain>(modelInstance);
        }

        /// <summary>
        /// Update associated items
        /// </summary>
        protected virtual void UpdateAssociatedItems<TAssociation, TDomainAssociation>(IEnumerable<TAssociation> storage, TModel source, DataContext context, IPrincipal principal)
            where TAssociation : IdentifiedData, ISimpleAssociation, new()
            where TDomainAssociation : DbAssociation, new()
        {
            var persistenceService = ApplicationContext.Current.GetService<IDataPersistenceService<TAssociation>>() as AuditPersistenceServiceBase<TAssociation>;
            if (persistenceService == null)
            {
                this.m_tracer.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "Missing persister for type {0}", typeof(TAssociation).Name);
                return;
            }
            // Ensure the source key is set
            foreach (var itm in storage)
                if (itm.SourceEntityKey == Guid.Empty ||
                    itm.SourceEntityKey == null)
                    itm.SourceEntityKey = source.Key;

            // Get existing
            var existing = context.Query<TDomainAssociation>(o => o.SourceKey == source.Key);
            // Remove old associations
            var obsoleteRecords = existing.Where(o => !storage.Any(ecn => ecn.Key == o.Key));
            foreach (var del in obsoleteRecords) // Obsolete records = delete as it is non-versioned association
                context.Delete(del);

            // Update those that need it
            var updateRecords = storage.Where(o => existing.Any(ecn => ecn.Key == o.Key && o.Key != Guid.Empty));
            foreach (var upd in updateRecords)
                persistenceService.UpdateInternal(context, upd, principal);

            // Insert those that do not exist
            var insertRecords = storage.Where(o => !existing.Any(ecn => ecn.Key == o.Key));
            foreach (var ins in insertRecords)
                persistenceService.InsertInternal(context, ins, principal);

        }


    }
}
