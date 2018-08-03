using MARC.HI.EHRS.SVC.Core.Event;
using MARC.HI.EHRS.SVC.Core.Services;
using SanteDB.OrmLite;
using SanteDB.Persistence.Data.ADO.Services;
using SanteGuard.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Persistence.Ado.Services
{
    /// <summary>
    /// Audit session persistence service
    /// </summary>
    public class AuditSessionPersistenceService : AdoBasePersistenceService<AuditSession>
    {
        public override object FromModelInstance(AuditSession modelInstance, DataContext context, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override AuditSession InsertInternal(DataContext context, AuditSession data, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override AuditSession ObsoleteInternal(DataContext context, AuditSession data, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<AuditSession> QueryInternal(DataContext context, Expression<Func<AuditSession, bool>> query, Guid queryId, int offset, int? count, out int totalResults, IPrincipal principal, bool countResults = true)
        {
            throw new NotImplementedException();
        }

        public override AuditSession ToModelInstance(object dataInstance, DataContext context, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override AuditSession UpdateInternal(DataContext context, AuditSession data, IPrincipal principal)
        {
            throw new NotImplementedException();
        }
    }
}
