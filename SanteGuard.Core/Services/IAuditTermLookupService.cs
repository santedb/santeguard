using SanteGuard.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Services
{
    /// <summary>
    /// Audit Term lookup service
    /// </summary>
    public interface IAuditTermLookupService
    {
        /// <summary>
        /// Retrieve the audit term key only 
        /// </summary>
        Guid? GetKey(String code, params String[] codeSystem);

        /// <summary>
        /// Gets the specified audit term
        /// </summary>
        /// <param name="code">The code to retrieve</param>
        /// <param name="codeSystem">The code system to retrieve in</param>
        /// <returns>The located audit term</returns>
        AuditTerm GetTerm(String code, params String[] codeSystem);

        /// <summary>
        /// Register a code in the term
        /// </summary>
        AuditTerm Register(string code, string codeSystem, string displayName);
    }
}
