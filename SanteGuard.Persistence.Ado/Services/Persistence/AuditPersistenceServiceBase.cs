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
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Map;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteDB.Persistence.Data.ADO.Services;
using SanteGuard.Model;
using SanteGuard.Persistence.Ado.Data.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Xml.Serialization;
using SanteDB.Core.Event;
using SanteDB.Core;
using SanteDB.Core.BusinessRules;
using SanteDB.Persistence.Data.ADO.Data;
using SanteDB.Core.Model.Query;

namespace SanteGuard.Persistence.Ado.Services
{
    /// <summary>
    /// Represents an audit persistence service
    /// </summary>
    public abstract class AuditPersistenceServiceBase<TModel> : IDataPersistenceService<TModel>, IFastQueryDataPersistenceService<TModel>, IStoredQueryDataPersistenceService<TModel>, IAdoPersistenceService where TModel : IdentifiedData
    {

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => $"ADO.NET SanteGuard Persister for {typeof(TModel).FullName}";
        /// <summary>
        /// Trace source name
        /// </summary>
        protected TraceSource m_tracer = new TraceSource(SanteGuardConstants.TraceSourceName + ".Ado");

        // Local mapper instance
        protected ModelMapper m_mapper = AdoAuditPersistenceService.GetMapper();

        /// <summary>
        /// The service is insterting data
        /// </summary>
        public event EventHandler<DataPersistingEventArgs<TModel>> Inserting;
        /// <summary>
        /// The service has inserted data
        /// </summary>
        public event EventHandler<DataPersistedEventArgs<TModel>> Inserted;
        /// <summary>
        /// The service is updating data 
        /// </summary>
        public event EventHandler<DataPersistingEventArgs<TModel>> Updating;
        /// <summary>
        /// The service has updated data
        /// </summary>
        public event EventHandler<DataPersistedEventArgs<TModel>> Updated;
        /// <summary>
        /// The service is obsoleting data
        /// </summary>
        public event EventHandler<DataPersistingEventArgs<TModel>> Obsoleting;
        /// <summary>
        /// The service has obsoleted data
        /// </summary>
        public event EventHandler<DataPersistedEventArgs<TModel>> Obsoleted;
        /// <summary>
        /// The service is retrieving data
        /// </summary>
        public event EventHandler<DataRetrievingEventArgs<TModel>> Retrieving;
        /// <summary>
        /// The service has retrieved data
        /// </summary>
        public event EventHandler<DataRetrievedEventArgs<TModel>> Retrieved;
        /// <summary>
        /// The service is querying data
        /// </summary>
        public event EventHandler<QueryRequestEventArgs<TModel>> Querying;
        /// <summary>
        /// The service has queried data
        /// </summary>
        public event EventHandler<QueryResultEventArgs<TModel>> Queried;

        /// <summary>
        /// Count the number of objects in the persistence store
        /// </summary>
        /// <param name="query">The query to be secute</param>
        /// <param name="authContext">The authentication context</param>
        /// <returns>The matching records</returns>
        public long Count(Expression<Func<TModel, bool>> query, IPrincipal authContext)
        {
            var tr = 0;
            this.Query(query, 0, 0, out tr, authContext);
            return tr;
        }

        /// <summary>
        /// Get the specified resource
        /// </summary>
        /// <typeparam name="TIdentifier">The type of identifier</typeparam>
        /// <param name="containerId">The id of the identifier to get</param>
        /// <param name="principal">The security principal to execute as</param>
        /// <param name="loadFast">True if to skip loading of some properties</param>
        /// <returns>The loaded model</returns>
        public TModel Get(Guid containerId, Guid? containerVersion, bool loadFast, IPrincipal principal)
        {
            // Try the cache if available
            var guidIdentifier = containerId;

            var cacheItem = ApplicationServiceContext.Current.GetService<IDataCachingService>()?.GetCacheItem<TModel>(guidIdentifier) as TModel;
            if (loadFast && cacheItem != null)
            {
                return cacheItem;
            }
            else
            {

#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif

                DataRetrievingEventArgs<TModel> preArgs = new DataRetrievingEventArgs<TModel>(containerId, containerVersion, principal);
                this.Retrieving?.Invoke(this, preArgs);
                if (preArgs.Cancel)
                {
                    this.m_tracer.TraceEvent(TraceEventType.Warning, 0, "Pre-Event handler indicates abort retrieve {0}", containerId);
                    return null;
                }

                // Query object
                using (var connection = AdoAuditPersistenceService.GetConfiguration().Provider.GetReadonlyConnection())
                    try
                    {
                        connection.Open();
                        this.m_tracer.TraceEvent(TraceEventType.Verbose, 0, "GET {0}", containerId);

                        if (loadFast)
                        {
                            connection.AddData("loadFast", true);
                            connection.LoadState = LoadState.PartialLoad;
                        }
                        else
                            connection.LoadState = LoadState.FullLoad;

                        var result = this.GetInternal(connection, guidIdentifier, principal);
                        var postData = new DataRetrievedEventArgs<TModel>(result, principal);
                        this.Retrieved?.Invoke(this, postData);

                        foreach (var itm in connection.CacheOnCommit)
                            ApplicationServiceContext.Current.GetService<IDataCachingService>()?.Add(itm);

                        return result;

                    }
                    catch (NotSupportedException e)
                    {
                        throw new DataPersistenceException("Cannot perform LINQ query", e);
                    }
                    catch (Exception e)
                    {
                        this.m_tracer.TraceEvent(TraceEventType.Error, 0, "Error : {0}", e);
                        throw;
                    }
                    finally
                    {
#if DEBUG
                        sw.Stop();
                        this.m_tracer.TraceEvent(TraceEventType.Verbose, 0, "Retrieve took {0} ms", sw.ElapsedMilliseconds);
#endif
                    }
            }
        }

        /// <summary>
        /// Insert the specified storage data
        /// </summary>
        /// <param name="data">The storage data to be inserted</param>
        /// <param name="principal">The authentication context</param>
        /// <param name="mode">The transaction control mode</param>
        /// <returns>The inserted data</returns>
        public TModel Insert(TModel data, TransactionMode mode, IPrincipal principal)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            DataPersistingEventArgs<TModel> preArgs = new DataPersistingEventArgs<TModel>(data, principal);
            this.Inserting?.Invoke(this, preArgs);
            if (preArgs.Cancel)
            {
                this.m_tracer.TraceEvent(TraceEventType.Warning, 0, "Pre-Event handler indicates abort insert for {0}", data);
                return data;
            }

            // Persist object
            using (var connection = AdoAuditPersistenceService.GetConfiguration().Provider.GetWriteConnection())
            {
                connection.Open();
                using (IDbTransaction tx = connection.BeginTransaction())
                    try
                    {
                        connection.EstablishProvenance(principal, (data as NonVersionedEntityData)?.CreatedByKey ?? (data as BaseEntityData)?.CreatedByKey);
                        // Disable inserting duplicate classified objects
                        var existing = data.TryGetExisting(connection, principal, true);
                        if (existing != null)
                        {
                            throw new DuplicateNameException(data.Key?.ToString());
                        }
                        else
                        {
                            this.m_tracer.TraceEvent(TraceEventType.Verbose, 0, "INSERT {0}", data);
                            data = this.InsertInternal(connection, data, principal);
                            connection.AddCacheCommit(data);
                        }
                        data.LoadState = LoadState.FullLoad; // We just persisted so it is fully loaded

                        if (mode == TransactionMode.Commit)
                        {
                            tx.Commit();
                            foreach (var itm in connection.CacheOnCommit)
                                ApplicationServiceContext.Current.GetService<IDataCachingService>()?.Add(itm);
                        }
                        else
                            tx.Rollback();

                        var args = new DataPersistedEventArgs<TModel>(data, principal);

                        this.Inserted?.Invoke(this, args);

                        return data;

                    }
                    catch (DbException e)
                    {

#if DEBUG
                        this.m_tracer.TraceEvent(TraceEventType.Error, 0, "Error : {0} -- {1}", e, this.ObjectToString(data));
#else
                            this.m_tracer.TraceEvent(TraceEventType.Error, 0, "Error : {0}", e.Message);
#endif
                        tx?.Rollback();

                        this.TranslateDbException(e);
                        throw;
                    }
                    catch (Exception e)
                    {
                        this.m_tracer.TraceEvent(TraceEventType.Error, 0, "Error : {0} -- {1}", e, this.ObjectToString(data));

                        tx?.Rollback();
                        throw new DataPersistenceException(e.Message, e);
                    }
            }
        }

        /// <summary>
        /// Obsolete the specified key data
        /// </summary>
        /// <param name="storageData">The storage data to be obsoleted</param>
        /// <param name="principal">The principal to use to obsolete</param>
        /// <param name="mode">The mode of obsoletion</param>
        /// <returns>The obsoleted record</returns>
        public TModel Obsolete(TModel data, TransactionMode mode, IPrincipal principal)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            else if (data.Key == Guid.Empty)
                throw new InvalidOperationException("Data missing key");

            DataPersistingEventArgs<TModel> preArgs = new DataPersistingEventArgs<TModel>(data, principal);
            this.Obsoleting?.Invoke(this, preArgs);
            if (preArgs.Cancel)
            {
                this.m_tracer.TraceEvent(TraceEventType.Warning, 0, "Pre-Event handler indicates abort for {0}", data);
                return data;
            }

            // Obsolete object
            using (var connection = AdoAuditPersistenceService.GetConfiguration().Provider.GetWriteConnection())
            {

                connection.Open();
                using (IDbTransaction tx = connection.BeginTransaction())
                    try
                    {
                        //connection.Connection.Open();

                        this.m_tracer.TraceEvent(TraceEventType.Verbose, 0, "OBSOLETE {0}", data);
                        connection.EstablishProvenance(principal, (data as NonVersionedEntityData)?.ObsoletedByKey ?? (data as BaseEntityData)?.ObsoletedByKey);

                        data = this.ObsoleteInternal(connection, data, principal);
                        connection.AddCacheCommit(data);
                        if (mode == TransactionMode.Commit)
                        {
                            tx.Commit();
                            foreach (var itm in connection.CacheOnCommit)
                                ApplicationServiceContext.Current.GetService<IDataCachingService>()?.Remove(itm.Key.Value);
                        }
                        else
                            tx.Rollback();

                        var args = new DataPersistedEventArgs<TModel>(data, principal);

                        this.Obsoleted?.Invoke(this, args);

                        return data;
                    }
                    catch (Exception e)
                    {
                        this.m_tracer.TraceEvent(TraceEventType.Error, 0, "Error : {0}", e);
                        tx?.Rollback();
                        throw new DataPersistenceException(e.Message, e);
                    }
                    finally
                    {
                    }
            }
        }

        /// <summary>
        /// Perform a query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="authContext"></param>
        /// <returns></returns>
        public IEnumerable<TModel> Query(Expression<Func<TModel, bool>> query, IPrincipal authContext)
        {
            var tr = 0;
            return this.QueryInvoke(query, Guid.Empty, 0, null, authContext, out tr, true, null);
        }

        /// <summary>
        /// Perform a query
        /// </summary>
        /// <param name="query">The query to perform</param>
        /// <param name="offset">The offset</param>
        /// <param name="count">The count of results</param>
        /// <param name="authContext">The authorization context</param>
        /// <param name="totalCount">The total count</param>
        /// <returns></returns>
        public IEnumerable<TModel> Query(Expression<Func<TModel, bool>> query, int offset, int? count, out int totalCount, IPrincipal authContext, params ModelSort<TModel>[] orderBy)
        {
            return this.QueryInvoke(query, Guid.Empty, offset, count, authContext, out totalCount, false, orderBy);
        }

        /// <summary>
        /// Performs query logic
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="queryId">The query identifier</param>
        /// <param name="offset">The offset of the query</param>
        /// <param name="count">The total count to return</param>
        /// <param name="authContext">The authentication context</param>
        /// <param name="totalCount">The total results matching</param>
        /// <param name="fastQuery">True if fast querying should be performed</param>
        /// <returns>The matching results</returns>
        protected virtual IEnumerable<TModel> QueryInvoke(Expression<Func<TModel, bool>> query, Guid queryId, int offset, int? count, IPrincipal authContext, out int totalCount, bool fastQuery, ModelSort<TModel>[] orderBy)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif

            QueryRequestEventArgs<TModel> preArgs = new QueryRequestEventArgs<TModel>(query, offset, count, queryId, authContext);
            this.Querying?.Invoke(this, preArgs);
            if (preArgs.Cancel)
            {
                this.m_tracer.TraceEvent(TraceEventType.Warning, 0, "Pre-Event handler indicates abort query {0}", query);
                totalCount = preArgs.TotalResults;
                return preArgs.Results;
            }

            // Query object
            using (var connection = AdoAuditPersistenceService.GetConfiguration().Provider.GetReadonlyConnection())
                try
                {
                    connection.Open();

                    this.m_tracer.TraceEvent(TraceEventType.Verbose, 0, "QUERY {0}", query);

                    // Is there an obsoletion item already specified?
                    if ((count ?? 1000) > 25 && AdoAuditPersistenceService.GetConfiguration().PrepareStatements)
                        connection.PrepareStatements = true;
                    if (fastQuery)
                    {
                        connection.AddData("loadFast", true);
                        connection.LoadState = LoadState.PartialLoad;
                    }
                    else
                        connection.LoadState = LoadState.FullLoad;

                    var results = this.QueryInternal(connection, query, queryId, offset, count ?? 1000, out totalCount, authContext, true, orderBy);
                    var postData = new QueryResultEventArgs<TModel>(query, results.AsQueryable(), offset, count, totalCount, queryId, authContext);
                    this.Queried?.Invoke(this, postData);

                    var retVal = postData.Results.ToList();

                    // Add to cache
                    foreach (var i in retVal.AsParallel().Where(i => i != null))
                        connection.AddCacheCommit(i);

                    ApplicationServiceContext.Current.GetService<IThreadPoolService>()?.QueueUserWorkItem(o =>
                    {
                        foreach (var itm in (o as IEnumerable<IdentifiedData>))
                            ApplicationServiceContext.Current.GetService<IDataCachingService>()?.Add(itm);
                    }, connection.CacheOnCommit.ToList());

                    this.m_tracer.TraceEvent(TraceEventType.Verbose, 0, "Returning {0}..{1} or {2} results", offset, offset + (count ?? 1000), totalCount);

                    return retVal;

                }
                catch (NotSupportedException e)
                {
                    throw new DataPersistenceException("Cannot perform LINQ query", e);
                }
                catch (Exception e)
                {
                    this.m_tracer.TraceEvent(TraceEventType.Error, 0, "Error : {0}", e);
                    throw;
                }
                finally
                {
#if DEBUG
                    sw.Stop();
                    this.m_tracer.TraceEvent(TraceEventType.Verbose, 0, "Query {0} took {1} ms", query, sw.ElapsedMilliseconds);
#endif
                }
        }

        /// <summary>
        /// Update the specified container
        /// </summary>
        /// <param name="data">The data to be updated</param>
        /// <param name="principal">The principal for authorization</param>
        /// <param name="mode">The mode of operation</param>
        /// <returns>The updated model</returns>
        public TModel Update(TModel data, TransactionMode mode, IPrincipal principal)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            else if (data.Key == Guid.Empty)
                throw new InvalidOperationException("Data missing key");

            DataPersistingEventArgs<TModel> preArgs = new DataPersistingEventArgs<TModel>(data, principal);
            this.Updating?.Invoke(this, preArgs);
            if (preArgs.Cancel)
            {
                this.m_tracer.TraceEvent(TraceEventType.Warning, 0, "Pre-Event handler indicates abort update for {0}", data);
                return data;
            }

            // Persist object
            using (var connection = AdoAuditPersistenceService.GetConfiguration().Provider.GetWriteConnection())
            {
                connection.Open();
                using (IDbTransaction tx = connection.BeginTransaction())
                    try
                    {
                        //connection.Connection.Open();
                        connection.EstablishProvenance(principal, (data as NonVersionedEntityData)?.UpdatedByKey ?? (data as BaseEntityData)?.CreatedByKey);

                        this.m_tracer.TraceEvent(TraceEventType.Verbose, 0, "UPDATE {0}", data);

                        data = this.UpdateInternal(connection, data, principal);
                        connection.AddCacheCommit(data);
                        data.LoadState = LoadState.FullLoad; // We just persisted this so it is fully loaded

                        if (mode == TransactionMode.Commit)
                        {
                            tx.Commit();
                            foreach (var itm in connection.CacheOnCommit)
                                ApplicationServiceContext.Current.GetService<IDataCachingService>()?.Add(itm);

                        }
                        else
                            tx.Rollback();

                        var args = new DataPersistedEventArgs<TModel>(data, principal);

                        this.Updated?.Invoke(this, args);

                        return data;
                    }
                    catch (DbException e)
                    {

#if DEBUG
                        this.m_tracer.TraceEvent(TraceEventType.Error, 0, "Error : {0} -- {1}", e, this.ObjectToString(data));
#else
                            this.m_tracer.TraceEvent(TraceEventType.Error, 0, "Error : {0}", e.Message);
#endif
                        tx?.Rollback();

                        this.TranslateDbException(e);
                        throw;
                    }
                    catch (Exception e)
                    {

#if DEBUG
                        this.m_tracer.TraceEvent(TraceEventType.Error, 0, "Error : {0} -- {1}", e, this.ObjectToString(data));
#else
                        this.m_tracer.TraceEvent(TraceEventType.Error, 0, "Error : {0}", e.Message);
#endif
                        tx?.Rollback();

                        // if the exception is key not found, we want the caller to know
                        // so that a potential insert can take place
                        if (e is KeyNotFoundException)
                        {
                            throw new KeyNotFoundException(e.Message, e);
                        }

                        // if the exception is anything else, we want to throw a data persistence exception
                        throw new DataPersistenceException(e.Message, e);

                    }
                    finally
                    {
                    }
            }
        }

        #region Helper Functions

        /// <summary>
        /// Get the specified key.
        /// </summary>
        /// <param name="key">The Key</param>
        public virtual TModel GetInternal(DataContext context, Guid key, IPrincipal principal)
        {
            int tr = 0;
            var cacheService = ApplicationServiceContext.Current.GetService<IDataCachingService>();
            var cacheItem = cacheService.GetCacheItem(key) as TModel ?? context.GetCacheCommit(key) as TModel;
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
                cacheItem = this.QueryInternal(context, o => o.Key == key, Guid.Empty, 0, 1, out tr, principal, false, null)?.FirstOrDefault();
                if (cacheService != null)
                    cacheService.Add(cacheItem);
                return cacheItem;
            }
        }

        /// <summary>
        /// Maps the data to a model instance
        /// </summary>
        /// <returns>The model instance.</returns>
        /// <param name="dataInstance">Data instance.</param>
        public abstract TModel ToModelInstance(Object dataInstance, DataContext context, IPrincipal principal);

        /// <summary>
        /// Froms the model instance.
        /// </summary>
        /// <returns>The model instance.</returns>
        /// <param name="modelInstance">Model instance.</param>
        public abstract Object FromModelInstance(TModel modelInstance, DataContext context, IPrincipal principal);

        /// <summary>
        /// Performthe actual insert.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="data">Data.</param>
        public abstract TModel InsertInternal(DataContext context, TModel data, IPrincipal principal);

        /// <summary>
        /// Perform the actual update.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="data">Data.</param>
        public abstract TModel UpdateInternal(DataContext context, TModel data, IPrincipal principal);

        /// <summary>
        /// Performs the actual obsoletion
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="data">Data.</param>
        public abstract TModel ObsoleteInternal(DataContext context, TModel data, IPrincipal principal);

        /// <summary>
        /// Performs the actual query
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="query">Query.</param>
        public abstract IEnumerable<TModel> QueryInternal(DataContext context, Expression<Func<TModel, bool>> query, Guid queryId, int offset, int? count, out int totalResults, IPrincipal principal, bool countResults, ModelSort<TModel>[] orderBy);

        /// <summary>
        /// Convert object to string
        /// </summary>
        private String ObjectToString(TModel data)
        {
            if (data == null) return "null";
            IEnumerable<Type> extraTypes = new Type[] { typeof(TModel) };
            if (data is AuditBundle)
                extraTypes = extraTypes.Union((data as AuditBundle).Item.Select(o => o.GetType()));

            XmlSerializer xsz = new XmlSerializer(data.GetType(), extraTypes.ToArray());
            using (MemoryStream ms = new MemoryStream())
            {
                xsz.Serialize(ms, data);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Translates a DB exception to an appropriate SanteDB exception
        /// </summary>
        /// <summary>
        /// Translates a DB exception to an appropriate SanteDB exception
        /// </summary>
        private void TranslateDbException(DbException e)
        {
            if (e.Data["SqlState"] != null)
            {
                switch (e.Data["SqlState"].ToString())
                {
                    case "O9001": // SanteDB => Data Validation Error
                        throw new DetectedIssueException(
                            new DetectedIssue(DetectedIssuePriorityType.Error, e.Message, DetectedIssueKeys.InvalidDataIssue));
                    case "O9002": // SanteDB => Codification error
                        throw new DetectedIssueException(new List<DetectedIssue>() {
                                        new DetectedIssue(DetectedIssuePriorityType.Error, e.Message, DetectedIssueKeys.CodificationIssue),
                                        new DetectedIssue(DetectedIssuePriorityType.Information, "HINT: Select a code that is from the correct concept set or add the selected code to the concept set", DetectedIssueKeys.CodificationIssue)
                                    });
                    case "23502": // PGSQL - NOT NULL 
                        throw new DetectedIssueException(
                                        new DetectedIssue(DetectedIssuePriorityType.Error, e.Message, DetectedIssueKeys.InvalidDataIssue)
                                    );
                    case "23503": // PGSQL - FK VIOLATION
                        throw new DetectedIssueException(
                                        new DetectedIssue(DetectedIssuePriorityType.Error, e.Message, DetectedIssueKeys.FormalConstraintIssue)
                                    );
                    case "23505": // PGSQL - UQ VIOLATION
                        throw new DetectedIssueException(
                                        new DetectedIssue(DetectedIssuePriorityType.Error, e.Message, DetectedIssueKeys.AlreadyDoneIssue)
                                    );
                    case "23514": // PGSQL - CK VIOLATION
                        throw new DetectedIssueException(new List<DetectedIssue>()
                        {
                            new DetectedIssue(DetectedIssuePriorityType.Error, e.Message, DetectedIssueKeys.FormalConstraintIssue),
                            new DetectedIssue(DetectedIssuePriorityType.Information, "HINT: The code you're using may be incorrect for the given context", DetectedIssueKeys.CodificationIssue)
                        });
                    default:
                        throw new DataPersistenceException(e.Message, e);
                }
            }
            else
            {
                throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, e.Message, DetectedIssueKeys.OtherIssue));
            }
        }
        #endregion 

        #region IAdoPersistenceService  Implementation
        /// <summary>
        /// Insert the object for generic methods
        /// </summary>
        object IAdoPersistenceService.Insert(DataContext context, object data)
        {
            return this.InsertInternal(context, (TModel)data, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Update the object for generic methods
        /// </summary>
        object IAdoPersistenceService.Update(DataContext context, object data)
        {
            return this.UpdateInternal(context, (TModel)data,AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Obsolete the object for generic methods
        /// </summary>
        object IAdoPersistenceService.Obsolete(DataContext context, object data)
        {
            return this.ObsoleteInternal(context, (TModel)data, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Get the specified data
        /// </summary>
        object IAdoPersistenceService.Get(DataContext context, Guid id)
        {
            return this.GetInternal(context, id, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Insert the object
        /// </summary>
        object IDataPersistenceService.Insert(object data)
        {
            return this.Insert((TModel)data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Update the specified data
        /// </summary>
        object IDataPersistenceService.Update(object data)
        {
            return this.Update((TModel)data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Obsolete specified data
        /// </summary>
        object IDataPersistenceService.Obsolete(object data)
        {
            return this.Obsolete((TModel)data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Get the specified data
        /// </summary>
        object IDataPersistenceService.Get(Guid id)
        {
            return this.Get(id, Guid.Empty, false, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Generic to model instance for other callers
        /// </summary>
        /// <returns></returns>
        object IAdoPersistenceService.ToModelInstance(object domainInstance, DataContext context)
        {
            return this.ToModelInstance(domainInstance, context, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Perform generic query
        /// </summary>
        IEnumerable IDataPersistenceService.Query(Expression query, int offset, int? count, out int totalResults)
        {
            return this.Query((Expression<Func<TModel, bool>>)query, offset, count, out totalResults, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Perform an identified query
        /// </summary>
        public IEnumerable<TModel> Query(Expression<Func<TModel, bool>> query, Guid queryId, int offset, int? count, out int totalCount, IPrincipal authContext, params ModelSort<TModel>[] orderBy)
        {
            return this.QueryInvoke(query, queryId, offset, count, authContext, out totalCount, false, orderBy);
        }

        /// <summary>
        /// Perform query in a lightweight manner
        /// </summary>
        public IEnumerable<TModel> QueryFast(Expression<Func<TModel, bool>> query, Guid queryId, int offset, int? count, out int totalCount, IPrincipal authContext)
        {
            return this.QueryInvoke(query, queryId, offset, count, authContext, out totalCount, true, null);

        }

        #endregion
    }
}
