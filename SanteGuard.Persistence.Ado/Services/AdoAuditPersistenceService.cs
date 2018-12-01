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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Map;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteDB.Persistence.Data.ADO.Services;
using SanteGuard.Persistence.Ado.Configuration;
using SanteGuard.Persistence.Ado.Data.Extensions;
using SanteGuard.Persistence.Ado.Data.Model;
using SanteGuard.Persistence.Ado.Services.Persistence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;

namespace SanteGuard.Persistence.Ado.Services
{
    /// <summary>
    /// Represents the ADO persistence service
    /// </summary>
    [ServiceProvider("ADO.NET Audit Persistence Daemon")]
    public class AdoAuditPersistenceService : IDaemonService
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "ADO.NET Audit Persistence Daemon";

        // Tracer
        private static TraceSource s_tracer = new TraceSource(SanteGuardConstants.TraceSourceName);

        /// <summary>
        /// STatic ctor loads configuration and mappers
        /// </summary>
        static AdoAuditPersistenceService()
        {
            try
            {
                s_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<SanteGuardAdoConfiguration>();
                s_mapper = new ModelMapper(typeof(AdoAuditPersistenceService).Assembly.GetManifestResourceStream("SanteGuard.Persistence.Ado.Data.Map.ModelMap.xml"));
                s_queryBuilder = new QueryBuilder(s_mapper, s_configuration.Provider);
            }
            catch(ModelMapValidationException e)
            {
                s_tracer.TraceError("Model map for ADO persistence is invalid:");
                foreach (var i in e.ValidationDetails)
                    s_tracer.TraceError("{0} : {1} @ {2}", i.Level, i.Message, i.Location);
                throw;
            }
            catch(Exception e)
            {
                s_tracer.TraceError("Error initializing SanteGuard persistence: {0}", e.Message);
                throw;
            }
        }

        #region Helper Properties
        // Model mapper loaded
        private static ModelMapper s_mapper;
        private static SanteGuardAdoConfiguration s_configuration;
        // Cache
        private static Dictionary<Type, IAdoPersistenceService> s_persistenceCache = new Dictionary<Type, IAdoPersistenceService>();

        // Query builder
        private static QueryBuilder s_queryBuilder;

        /// <summary>
        /// Get configuration
        /// </summary>
        public static SanteGuardAdoConfiguration GetConfiguration() { return s_configuration; }

        /// <summary>
        /// Gets the mode mapper
        /// </summary>
        /// <returns></returns>
        public static ModelMapper GetMapper() { return s_mapper; }

        /// <summary>
        /// Get query builder
        /// </summary>
        public static QueryBuilder GetQueryBuilder()
        {
            return s_queryBuilder;
        }

        // <summary>
        /// Generic versioned persister service for any non-customized persister
        /// </summary>
        internal class GenericBasePersistenceService<TModel, TDomain> : BaseDataPersistenceService<TModel, TDomain>
            where TDomain : class, IDbBaseData, new()
            where TModel : BaseEntityData, new()
        {

            /// <summary>
            /// Ensure exists
            /// </summary>
            public override TModel InsertInternal(DataContext context, TModel data, IPrincipal principal)
            {
                foreach (var rp in typeof(TModel).GetRuntimeProperties().Where(o => typeof(IdentifiedData).GetTypeInfo().IsAssignableFrom(o.PropertyType.GetTypeInfo())))
                {
                    if (rp.GetCustomAttribute<DataIgnoreAttribute>() != null || !rp.CanRead || !rp.CanWrite)
                        continue;

                    var instance = rp.GetValue(data);
                    if (instance != null)
                        rp.SetValue(data, DataModelExtensions.EnsureExists(instance as IdentifiedData, context, principal));
                }
                return base.InsertInternal(context, data, principal);
            }

            /// <summary>
            /// Update the specified object
            /// </summary>
            public override TModel UpdateInternal(DataContext context, TModel data, IPrincipal principal)
            {
                foreach (var rp in typeof(TModel).GetRuntimeProperties().Where(o => typeof(IdentifiedData).GetTypeInfo().IsAssignableFrom(o.PropertyType.GetTypeInfo())))
                {
                    if (rp.GetCustomAttribute<DataIgnoreAttribute>() != null || !rp.CanRead || !rp.CanWrite)
                        continue;

                    var instance = rp.GetValue(data);
                    if (instance != null)
                        rp.SetValue(data, DataModelExtensions.EnsureExists(instance as IdentifiedData, context, principal));

                }
                return base.UpdateInternal(context, data, principal);
            }
        }

        /// <summary>
        /// Generic versioned persister service for any non-customized persister
        /// </summary>
        internal class GenericIdentityPersistenceService<TModel, TDomain> : IdentifiedPersistenceService<TModel, TDomain>
            where TModel : IdentifiedData, new()
            where TDomain : class, IDbIdentified, new()
        {
            /// <summary>
            /// Ensure exists
            /// </summary>
            public override TModel InsertInternal(DataContext context, TModel data, IPrincipal principal)
            {
                foreach (var rp in typeof(TModel).GetRuntimeProperties().Where(o => typeof(IdentifiedData).GetTypeInfo().IsAssignableFrom(o.PropertyType.GetTypeInfo())))
                {
                    if (rp.GetCustomAttribute<DataIgnoreAttribute>() != null || !rp.CanRead || !rp.CanWrite)
                        continue;

                    var instance = rp.GetValue(data);
                    if (instance != null)
                        rp.SetValue(data, DataModelExtensions.EnsureExists(instance as IdentifiedData, context, principal));

                }
                return base.InsertInternal(context, data, principal);
            }

            /// <summary>
            /// Update the specified object
            /// </summary>
            public override TModel UpdateInternal(DataContext context, TModel data, IPrincipal principal)
            {
                foreach (var rp in typeof(TModel).GetRuntimeProperties().Where(o => typeof(IdentifiedData).GetTypeInfo().IsAssignableFrom(o.PropertyType.GetTypeInfo())))
                {
                    if (rp.GetCustomAttribute<DataIgnoreAttribute>() != null || !rp.CanRead || !rp.CanWrite)
                        continue;

                    var instance = rp.GetValue(data);
                    if (instance != null)
                        rp.SetValue(data, DataModelExtensions.EnsureExists(instance as IdentifiedData, context, principal));

                }
                return base.UpdateInternal(context, data, principal);
            }
        }

        /// <summary>
        /// Generic association persistence service
        /// </summary>
        internal class GenericBaseAssociationPersistenceService<TModel, TDomain> :
            GenericBasePersistenceService<TModel, TDomain>, IAdoAssociativePersistenceService
            where TModel : BaseEntityData, ISimpleAssociation, new()
            where TDomain : class, IDbBaseData, new()
        {
            /// <summary>
            /// Get all the matching TModel object from source
            /// </summary>
            public IEnumerable GetFromSource(DataContext context, Guid sourceId, decimal? versionSequenceId)
            {
                int tr = 0;
                return this.QueryInternal(context, base.BuildSourceQuery<TModel>(sourceId), Guid.Empty, 0, null, out tr, AuthenticationContext.Current.Principal).ToList();
            }
        }

        /// <summary>
        /// Generic association persistence service
        /// </summary>
        internal class GenericBaseVersionedAssociationPersistenceService<TModel, TDomain> :
            GenericBasePersistenceService<TModel, TDomain>, IAdoAssociativePersistenceService
            where TModel : BaseEntityData, IVersionedAssociation, new()
            where TDomain : class, IDbBaseData, new()
        {
            /// <summary>
            /// Get all the matching TModel object from source
            /// </summary>
            public IEnumerable GetFromSource(DataContext context, Guid sourceId, decimal? versionSequenceId)
            {
                int tr = 0;
                // TODO: Check that this query is actually building what it is supposed to.
                return this.QueryInternal(context, base.BuildSourceQuery<TModel>(sourceId, versionSequenceId), Guid.Empty, 0, null, out tr, AuthenticationContext.Current.Principal).ToList();
            }
        }

        /// <summary>
        /// Get persister for the specified type
        /// </summary>
        internal static IAdoPersistenceService GetPersister(Type type)
        {
            IAdoPersistenceService retVal = null;
            if(!s_persistenceCache.TryGetValue(type, out retVal))
            {
                var idt = typeof(IDataPersistenceService<>).MakeGenericType(type);
                retVal = ApplicationServiceContext.Current.GetService(idt) as IAdoPersistenceService;
                if (retVal != null)
                    lock (s_persistenceCache)
                        if(!s_persistenceCache.ContainsKey(type))
                            s_persistenceCache.Add(type, retVal);
            }
            return retVal;
        }

        /// <summary>
        /// Generic association persistence service
        /// </summary>
        internal class GenericIdentityAssociationPersistenceService<TModel, TDomain> :
            GenericIdentityPersistenceService<TModel, TDomain>, IAdoAssociativePersistenceService
            where TModel : IdentifiedData, ISimpleAssociation, new()
            where TDomain : class, IDbIdentified, new()
        {
            /// <summary>
            /// Get all the matching TModel object from source
            /// </summary>
            public IEnumerable GetFromSource(DataContext context, Guid sourceId, decimal? versionSequenceId)
            {
                int tr = 0;
                return this.QueryInternal(context, base.BuildSourceQuery<TModel>(sourceId), Guid.Empty, 0, null, out tr, AuthenticationContext.Current.Principal).ToList();
            }
        }

        /// <summary>
        /// Generic association persistence service
        /// </summary>
        internal class GenericIdentityVersionedAssociationPersistenceService<TModel, TDomain> :
            GenericIdentityPersistenceService<TModel, TDomain>, IAdoAssociativePersistenceService
            where TModel : IdentifiedData, IVersionedAssociation, new()
            where TDomain : class, IDbIdentified, new()
        {
            /// <summary>
            /// Get all the matching TModel object from source
            /// </summary>
            public IEnumerable GetFromSource(DataContext context, Guid sourceId, decimal? versionSequenceId)
            {
                int tr = 0;
                // TODO: Check that this query is actually building what it is supposed to.
                return this.QueryInternal(context, base.BuildSourceQuery<TModel>(sourceId, versionSequenceId), Guid.Empty, 0, null, out tr, AuthenticationContext.Current.Principal).ToList();
            }
        }

        #endregion

        /// <summary>
        /// Gets whether the persistence service is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Service is starting
        /// </summary>
        public event EventHandler Starting;
        /// <summary>
        /// Service is stopping
        /// </summary>
        public event EventHandler Stopping;
        /// <summary>
        /// Service has started
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// Service has stopped
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Starts the service which scans this ASM for persistence and adds any missing persisters to the application context
        /// </summary>
        /// <returns>True if the service was started successfully</returns>
        public bool Start()
        {
            if (this.IsRunning) return true; /// Already registered

            this.Starting?.Invoke(this, EventArgs.Empty);

            // Iterate the persistence services
            foreach (var t in typeof(AdoAuditPersistenceService).Assembly.ExportedTypes.Where(o => typeof(IAdoPersistenceService).IsAssignableFrom(o) && !o.GetTypeInfo().IsAbstract && !o.IsGenericTypeDefinition))
            {
                try
                {
                    s_tracer.TraceEvent(TraceEventType.Information, 0, "Loading {0}...", t.AssemblyQualifiedName);
                    (ApplicationServiceContext.Current as IServiceManager).AddServiceProvider(t);
                }
                catch (Exception e)
                {
                    s_tracer.TraceEvent(TraceEventType.Error, e.HResult, "Error adding service {0} : {1}", t.AssemblyQualifiedName, e);
                }
            }

            // Now iterate through the map file and ensure we have all the mappings, if a class does not exist create it
            try
            {
                s_tracer.TraceEvent(TraceEventType.Information, 0, "Creating secondary model maps...");

                var map = ModelMap.Load(typeof(AdoAuditPersistenceService).Assembly.GetManifestResourceStream("SanteGuard.Persistence.Ado.Data.Map.ModelMap.xml"));
                foreach (var itm in map.Class)
                {
                    // Is there a persistence service?
                    var idpType = typeof(IDataPersistenceService<>);
                    Type modelClassType = Type.GetType(itm.ModelClass),
                        domainClassType = Type.GetType(itm.DomainClass);
                    idpType = idpType.MakeGenericType(modelClassType);

                    if (modelClassType.IsAbstract || domainClassType.IsAbstract) continue;

                    // Already created
                    if (ApplicationServiceContext.Current.GetService(idpType) != null)
                        continue;

                    s_tracer.TraceEvent(TraceEventType.Verbose, 0, "Creating map {0} > {1}", modelClassType, domainClassType);


                    if (modelClassType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IBaseEntityData)) &&
                        domainClassType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IDbBaseData)))
                    {
                        // Construct a type
                        Type pclass = null;
                        if (modelClassType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IVersionedAssociation)))
                            pclass = typeof(GenericBaseVersionedAssociationPersistenceService<,>);
                        else if (modelClassType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ISimpleAssociation)))
                            pclass = typeof(GenericBaseAssociationPersistenceService<,>);
                        else
                            pclass = typeof(GenericBasePersistenceService<,>);
                        pclass = pclass.MakeGenericType(modelClassType, domainClassType);
                        (ApplicationServiceContext.Current as IServiceManager).AddServiceProvider(pclass);
                        // Add to cache since we're here anyways
                        s_persistenceCache.Add(modelClassType, Activator.CreateInstance(pclass) as IAdoPersistenceService);
                    }
                    else if (modelClassType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IIdentifiedEntity)) &&
                        domainClassType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IDbIdentified)))
                    {
                        // Construct a type
                        Type pclass = null;
                        if (modelClassType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IVersionedAssociation)))
                            pclass = typeof(GenericIdentityVersionedAssociationPersistenceService<,>);
                        else if (modelClassType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ISimpleAssociation)))
                            pclass = typeof(GenericIdentityAssociationPersistenceService<,>);
                        else
                            pclass = typeof(GenericIdentityPersistenceService<,>);

                        pclass = pclass.MakeGenericType(modelClassType, domainClassType);
                        (ApplicationServiceContext.Current as IServiceManager).AddServiceProvider(pclass);
                        s_persistenceCache.Add(modelClassType, Activator.CreateInstance(pclass) as IAdoPersistenceService);
                    }
                    else
                        s_tracer.TraceEvent(TraceEventType.Warning, 0, "Classmap {0} > {1} cannot be created, ignoring", modelClassType, domainClassType);

                }
            }
            catch (Exception e)
            {
                s_tracer.TraceEvent(TraceEventType.Error, e.HResult, "Error initializing local persistence: {0}", e);
                throw e;
            }

            this.Started?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Stops the service
        /// </summary>
        /// <remarks>As this is not a true daemon service this method doesn't really do much</remarks>
        /// <returns></returns>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);
            this.IsRunning = false;
            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}
