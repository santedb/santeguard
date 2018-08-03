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
    /// Audit node persistence service
    /// </summary>
    public class AuditNodePersistenceService : AdoBasePersistenceService<AuditNode>
    {
        public override object FromModelInstance(AuditNode modelInstance, DataContext context, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override AuditNode InsertInternal(DataContext context, AuditNode data, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override AuditNode ObsoleteInternal(DataContext context, AuditNode data, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<AuditNode> QueryInternal(DataContext context, Expression<Func<AuditNode, bool>> query, Guid queryId, int offset, int? count, out int totalResults, IPrincipal principal, bool countResults = true)
        {
            throw new NotImplementedException();
        }

        public override AuditNode ToModelInstance(object dataInstance, DataContext context, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override AuditNode UpdateInternal(DataContext context, AuditNode data, IPrincipal principal)
        {
            throw new NotImplementedException();
        }
    }
}
