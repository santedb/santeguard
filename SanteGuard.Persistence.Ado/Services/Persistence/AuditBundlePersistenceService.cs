﻿using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Core.Services;
using SanteDB.Core.Model;
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
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Services.Persistence
{
    /// <summary>
    /// Represents a bundle service
    /// </summary>
    public class AuditBundlePersistenceService : AuditPersistenceServiceBase<AuditBundle>, IReportProgressChanged
    {
        // Progress has changed
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

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
            this.m_tracer.TraceInformation("Audit Bundle has {0} objects...", data.Item.Count);

            if (AdoPersistenceService.GetConfiguration().PrepareStatements)
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
                        this.m_tracer.TraceInformation("Will update {0} object from bundle...", itm);
                        data.Item[i] = svc.Update(context, itm) as IdentifiedData;
                    }
                    else
                    {
                        this.m_tracer.TraceInformation("Will insert {0} object from bundle...", itm);
                        data.Item[i] = svc.Insert(context, itm) as IdentifiedData;
                    }
                }
                catch (TargetInvocationException e)
                {
                    this.m_tracer.TraceEvent(System.Diagnostics.TraceEventType.Error, e.HResult, "Error inserting bundle: {0}", e);
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
                var svc = ApplicationContext.Current.GetService(idp);
                var mi = svc.GetType().GetRuntimeMethod("Obsolete", new Type[] { typeof(DataContext), itm.GetType(), typeof(IPrincipal) });

                itm.CopyObjectData(mi.Invoke(ApplicationContext.Current.GetService(idp), new object[] { context, itm }));
            }
            return data;
        }

        /// <summary>
        /// Query the specified object
        /// </summary>
        public override IEnumerable<AuditBundle> QueryInternal(DataContext context, Expression<Func<AuditBundle, bool>> query, Guid queryId, int offset, int? count, out int totalResults, IPrincipal principal, bool countResults = true)
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
