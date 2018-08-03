using MARC.HI.EHRS.SVC.Auditing.Data;
using SanteDB.Core.Services;
using SanteGuard.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Services.Repositories
{
    /// <summary>
    /// Represents an implementation of the audit repository classes for SanteGuard
    /// </summary>
    public class SanteGuardAuditRepository : IAuditRepositoryService
    {
        public IEnumerable<AuditData> Find(Expression<Func<AuditData, bool>> query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AuditData> Find(Expression<Func<AuditData, bool>> query, int offset, int? count, out int totalResults)
        {
            throw new NotImplementedException();
        }

        public AuditData Get(object correlationKey)
        {
            throw new NotImplementedException();
        }

        public AuditData Insert(AuditData audit)
        {
            throw new NotImplementedException();
        }
    }
}
