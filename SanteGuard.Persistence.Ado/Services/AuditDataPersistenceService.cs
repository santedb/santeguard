using MARC.HI.EHRS.SVC.Core.Event;
using MARC.HI.EHRS.SVC.Core.Services;
using SanteGuard.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using SanteDB.
using SanteDB.Persistence.Data.ADO.Services;
using SanteDB.OrmLite;

namespace SanteGuard.Persistence.Ado.Services
{
    /// <summary>
    /// Represents a persister that stores audit data
    /// </summary>
    public class AuditDataPersistenceService : AdoBasePersistenceService<AuditDataEx>
    {
        public override object FromModelInstance(AuditDataEx modelInstance, DataContext context, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override AuditDataEx InsertInternal(DataContext context, AuditDataEx data, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override AuditDataEx ObsoleteInternal(DataContext context, AuditDataEx data, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<AuditDataEx> QueryInternal(DataContext context, Expression<Func<AuditDataEx, bool>> query, Guid queryId, int offset, int? count, out int totalResults, IPrincipal principal, bool countResults = true)
        {
            throw new NotImplementedException();
        }

        public override AuditDataEx ToModelInstance(object dataInstance, DataContext context, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override AuditDataEx UpdateInternal(DataContext context, AuditDataEx data, IPrincipal principal)
        {
            throw new NotImplementedException();
        }
    }
}
