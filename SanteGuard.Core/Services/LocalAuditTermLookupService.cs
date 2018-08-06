using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Core.Services;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SanteGuard.Model;

namespace SanteGuard.Services
{
    /// <summary>
    /// Represents the local audit term lookup service
    /// </summary>
    public class LocalAuditTermLookupService : IAuditTermLookupService
    {
        /// <summary>
        /// Get the specified audit code
        /// </summary>
        public Guid? GetKey(string code, params string[] codeSystem)
        {
           return this.GetTerm(code, codeSystem)?.Key;
        }

        /// <summary>
        /// Get the specified audit term by code and code system
        /// </summary>
        public AuditTerm GetTerm(string code, params string[] codeSystem)
        {
            var repo = ApplicationContext.Current.GetService<IDataPersistenceService<AuditTerm>>();
            if (repo != null)
                throw new InvalidOperationException("Cannot find audit term service");

            // CS Expression
            var expr = QueryExpressionParser.BuildLinqExpression<AuditTerm>(new NameValueCollection()
            {
                { "mnemonic", new List<String>() { code } },
                { "domain", codeSystem.ToList() }
            });
            int tr;
            return repo.Query(expr, 0, 1, AuthenticationContext.Current.Principal, out tr).FirstOrDefault();
        }

        /// <summary>
        /// Register a new audit code
        /// </summary>
        public AuditTerm Register(string code, string codeSystem, string displayName)
        {
            var repo = ApplicationContext.Current.GetService<IRepositoryService<AuditTerm>>(); // We want this registration to be audited
            if (repo != null)
                throw new InvalidOperationException("Cannot find audit term repository");
            return repo.Insert(new AuditTerm()
            {
                DisplayName = displayName,
                Domain = codeSystem,
                Mnemonic = code
            });
        }
    }
}
