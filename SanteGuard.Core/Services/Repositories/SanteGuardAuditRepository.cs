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
using AtnaApi.Model;
using SanteDB.Core;
using SanteDB.Core.Auditing;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using SanteGuard.Core.Model;
using SanteGuard.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace SanteGuard.Services.Repositories
{
    /// <summary>
    /// Represents an implementation of the audit repository classes for SanteGuard
    /// </summary>
    [ServiceProvider("SanteGuard Enhanced Audit Repository")]
    public class SanteGuardAuditRepository : IAuditRepositoryService
    {

        public string ServiceName => "SanteGuard Enhanced Audit Repository";

        // Trace source
        private TraceSource m_tracer = new TraceSource(SanteGuardConstants.TraceSourceName);

        /// <summary>
        /// Find the specified audit data (nb: This functionality is very limited in this repository)
        /// </summary>
        public IEnumerable<AuditData> Find(Expression<Func<AuditData, bool>> query)
        {
            int tr;
            return this.Find(query, 0, 100, out tr);
        }

        /// <summary>
        /// Find the specified audits with the specified query 
        /// </summary>
        public IEnumerable<AuditData> Find(Expression<Func<AuditData, bool>> query, int offset, int? count, out int totalResults, params ModelSort<AuditData>[] orderBy)
        {

            // Convert query to easier format and map common parameters
            var inQuery = QueryExpressionBuilder.BuildQuery(query);
            var outQuery = new NameValueCollection();

            var termService = ApplicationServiceContext.Current.GetService<IAuditTermLookupService>();

            // Map query
            foreach(var kv in inQuery)
            {
                object valueList = kv.Value;
                if (!(valueList is IList))
                    valueList = new List<Object>() { valueList };

                // Iterate and add values
                foreach(var val in (valueList as IList).OfType<String>())
                    switch(kv.Key)
                    {
                        case "action":
                            {
                                var code = new CodeValue<AtnaApi.Model.ActionType>(
                                    (AtnaApi.Model.ActionType)Enum.Parse(typeof(AtnaApi.Model.ActionType), val)
                                );
                                outQuery.Add("action", termService.GetKey(code.Code, code.CodeSystem, "ActionType").ToString());
                            }
                            break;
                        case "correlationId":
                            outQuery.Add("correlationToken", val);
                            break;
                        case "event":
                            {
                                var code = new CodeValue<AtnaApi.Model.EventIdentifierType>(
                                    (AtnaApi.Model.EventIdentifierType)Enum.Parse(typeof(AtnaApi.Model.EventIdentifierType), val)
                                );
                                outQuery.Add("eventId", termService.GetKey(code.Code, code.CodeSystem, "EventIdentifierType").ToString());
                                break;
                            }
                        case "type.code":
                            outQuery.Add("eventType.mnemonic", val);
                            break;
                        case "type.codeSystem":
                            outQuery.Add("eventType.domain", val);
                            break;
                        case "outcome":
                            {
                                var code = new CodeValue<AtnaApi.Model.OutcomeIndicator>(
                                    (AtnaApi.Model.OutcomeIndicator)Enum.Parse(typeof(AtnaApi.Model.OutcomeIndicator), val)
                                );
                                outQuery.Add("outcome", termService.GetKey(code.Code, code.CodeSystem, "OutcomeIndicator").ToString());
                                break;
                            }
                        case "timestamp":
                            outQuery.Add("eventTime", val);
                            break;
                        default:
                            throw new InvalidOperationException($"Cannot map {kv.Key} as this property is unkown in SanteGuard");
                    }
            }

            var newQuery = QueryExpressionParser.BuildLinqExpression<Audit>(outQuery);
            return ApplicationServiceContext.Current.GetService<IRepositoryService<Audit>>().Find(newQuery, offset, count, out totalResults)
                .Select(o => o.ToAuditData());
        }

        /// <summary>
        /// Get the specified audit
        /// </summary>
        public AuditData Get(object correlationKey)
        {
            Guid correlationUuid = Guid.Parse(correlationKey.ToString());
            int tr;
            return ApplicationServiceContext.Current.GetService<IRepositoryService<Audit>>().Find(o => o.CorrelationToken == correlationUuid, 0, 1, out tr).FirstOrDefault()?.ToAuditData();
        }

        /// <summary>
        /// Insert the specified audit
        /// </summary>
        public AuditData Insert(AuditData audit)
        {
            this.m_tracer.TraceInfo("Persisting internal audit: AC={0}, ET={1}, OC={2}",
                audit.ActionCode,
                audit.EventTypeCode.Code,
                audit.Outcome);


            var rawAudit = audit.ToAudit();
            return ApplicationServiceContext.Current.GetService<IRepositoryService<Audit>>().Insert(rawAudit).ToAuditData();
        }
    }
}
