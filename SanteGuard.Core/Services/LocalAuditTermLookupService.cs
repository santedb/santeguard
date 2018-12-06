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
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SanteGuard.Model;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var repo = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AuditTerm>>();
            if (repo == null)
                throw new InvalidOperationException("Cannot find audit term service");

            // CS Expression
            var expr = QueryExpressionParser.BuildLinqExpression<AuditTerm>(new NameValueCollection()
            {
                { "mnemonic", new List<String>() { code } },
                { "domain", codeSystem.ToList() }
            });
            int tr;
            return repo.Query(expr, 0, 1, out tr, AuthenticationContext.SystemPrincipal).FirstOrDefault();
        }

        /// <summary>
        /// Register a new audit code
        /// </summary>
        public AuditTerm Register(string code, string codeSystem, string displayName)
        {
            var repo = ApplicationServiceContext.Current.GetService<IRepositoryService<AuditTerm>>(); // We want this registration to be audited
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
