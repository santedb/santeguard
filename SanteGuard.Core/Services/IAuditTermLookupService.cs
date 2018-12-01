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
using SanteGuard.Model;
using System;

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
