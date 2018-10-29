using AtnaApi.Model;
using MARC.HI.EHRS.SVC.Auditing.Data;
using MARC.HI.EHRS.SVC.Core;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using SanteGuard.Core.Model;
using SanteGuard.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        public IEnumerable<AuditData> Find(Expression<Func<AuditData, bool>> query, int offset, int? count, out int totalResults)
        {

            // Convert query to easier format and map common parameters
            var inQuery = QueryExpressionBuilder.BuildQuery(query);
            var outQuery = new NameValueCollection();

            var termService = ApplicationContext.Current.GetService<IAuditTermLookupService>();

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
            return ApplicationContext.Current.GetService<IRepositoryService<Audit>>().Find(newQuery, offset, count, out totalResults)
                .Select(o => o.ToAuditData());
        }

        /// <summary>
        /// Get the specified audit
        /// </summary>
        public AuditData Get(object correlationKey)
        {
            Guid correlationUuid = Guid.Parse(correlationKey.ToString());
            int tr;
            return ApplicationContext.Current.GetService<IRepositoryService<Audit>>().Find(o => o.CorrelationToken == correlationUuid, 0, 1, out tr).FirstOrDefault()?.ToAuditData();
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
            return ApplicationContext.Current.GetService<IRepositoryService<Audit>>().Insert(rawAudit).ToAuditData();
        }
    }
}
