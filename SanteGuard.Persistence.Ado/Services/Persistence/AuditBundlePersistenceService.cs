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
 * Date: 2018-10-28
 */
using SanteDB.Core;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteDB.Persistence.Data.ADO.Services;
using SanteGuard.Model;
using SanteGuard.Persistence.Ado.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;

namespace SanteGuard.Persistence.Ado.Services.Persistence
{
    /// <summary>
    /// Represents a bundle service
    /// </summary>
    public class AuditBundlePersistenceService : AuditPersistenceServiceBase<AuditBundle>, IReportProgressChanged
    {
        // Progress has changed
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        // Local instance of the persistence service
        private AdoPersistenceService m_persistenceService;

        /// <summary>
        /// Bundles don't really ever exist
        /// </summary>
        public override bool Exists(DataContext context, Guid key)
        {
            return false;
        }

        /// <summary>
        /// Bundle persistence
        /// </summary>
        public AuditBundlePersistenceService()
        {
            this.m_persistenceService = ApplicationServiceContext.Current.GetService<AdoPersistenceService>();
        }
        /// <summary>
        /// From model instance
        /// </summary>
        public override object FromModelInstance(AuditBundle modelInstance, DataContext context, IPrincipal principal)
        {
            return m_mapper.MapModelInstance<AuditBundle, Object>(modelInstance);
        }

        /// <summary>
        /// Insert or update contents of the bundle
        /// </summary>
        /// <returns></returns>
        public override AuditBundle InsertInternal(DataContext context, AuditBundle data, IPrincipal principal)
        {

            if (data.Item == null) return data;
            this.m_tracer.TraceInfo("Audit Bundle has {0} objects...", data.Item.Count);

            if (this.m_persistenceService.GetConfiguration().PrepareStatements)
                context.PrepareStatements = true;
            for (int i = 0; i < data.Item.Count; i++)
            {
                var itm = data.Item[i];
                var svc = AdoAuditPersistenceService.GetPersister(itm.GetType());

                this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs((float)(i + 1) / data.Item.Count, itm));
                try
                {
                    if (svc == null)
                        throw new InvalidOperationException($"Cannot find persister for {itm.GetType()}");
                    if (itm.TryGetExisting(context, principal, true) != null)
                    {
                        this.m_tracer.TraceInfo("Will update {0} object from bundle...", itm);
                        data.Item[i] = svc.Update(context, itm) as IdentifiedData;
                    }
                    else
                    {
                        this.m_tracer.TraceInfo("Will insert {0} object from bundle...", itm);
                        data.Item[i] = svc.Insert(context, itm) as IdentifiedData;
                    }
                }
                catch (TargetInvocationException e)
                {
                    this.m_tracer.TraceError( "Error inserting bundle: {0}", e);
                    throw e.InnerException;
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not insert bundle due to sub-object persistence (bundle item {i})", e);
                }

            }

            // Cache items
            foreach (var itm in data.Item)
            {
                itm.LoadState = LoadState.FullLoad;
                context.AddCacheCommit(itm);
            }
            return data;
        }

        /// <summary>
        /// Obsolete each object in the bundle
        /// </summary>
        public override AuditBundle ObsoleteInternal(DataContext context, AuditBundle data, IPrincipal principal)
        {
            foreach (var itm in data.Item)
            {
                var idp = typeof(IDataPersistenceService<>).MakeGenericType(new Type[] { itm.GetType() });
                var svc = ApplicationServiceContext.Current.GetService(idp);
                var mi = svc.GetType().GetRuntimeMethod("Obsolete", new Type[] { typeof(DataContext), itm.GetType(), typeof(IPrincipal) });

                itm.CopyObjectData(mi.Invoke(ApplicationServiceContext.Current.GetService(idp), new object[] { context, itm }));
            }
            return data;
        }

        /// <summary>
        /// Query the specified object
        /// </summary>
        public override IEnumerable<AuditBundle> QueryInternal(DataContext context, Expression<Func<AuditBundle, bool>> query, Guid queryId, int offset, int? count, out int totalResults, IPrincipal principal, bool countResults, ModelSort<AuditBundle>[] orderBy)
        {
            totalResults = 0;
            return new List<AuditBundle>().AsQueryable();
        }


        /// <summary>
        /// Model instance 
        /// </summary>
        public override AuditBundle ToModelInstance(object dataInstance, DataContext context, IPrincipal principal)
        {
            return m_mapper.MapModelInstance<Object, AuditBundle>(dataInstance);

        }

        /// <summary>
        /// Update all items in the bundle
        /// </summary>
        public override AuditBundle UpdateInternal(DataContext context, AuditBundle data, IPrincipal principal)
        {
            return this.InsertInternal(context, data, principal);
        }


    }
}
