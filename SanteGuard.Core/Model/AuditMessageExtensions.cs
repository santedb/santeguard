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
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SanteGuard.Configuration;
using SanteGuard.Model;
using SanteGuard.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace SanteGuard.Core.Model
{
    /// <summary>
    /// Audit message extensions
    /// </summary>
    public static class AuditMessageExtensions
    {
        /// <summary>
        /// Map or create a simple code
        /// </summary>
        private static AuditTerm MapOrCreateCode<T>(T simpleCode)
        {
            return MapOrCreateCode(new CodeValue<T>(simpleCode));
        }

        /// <summary>
        /// Map or create a simple code
        /// </summary>
        private static AuditTerm MapOrCreateCode(AuditCode code)
        {
            if (code == null) return null;
            return MapOrCreateCode(new CodeValue<String>(code.Code, code.CodeSystem, code.DisplayName));
        }

        /// <summary>
        /// Map code
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        private static AuditTerm MapOrCreateCode<T>(CodeValue<T> code)
        {
            if (code == null) return null;

            var auditTermService = ApplicationServiceContext.Current.GetService<IAuditTermLookupService>();
            if (auditTermService == null)
                throw new InvalidOperationException("Cannot locate audit term lookup");

            var retVal = auditTermService.GetTerm(code.Code, code.CodeSystem, typeof(T).Name);
            if (retVal == null)
                retVal = auditTermService.Register(code.Code, code.CodeSystem ?? typeof(T).Name, code.DisplayName);
            return retVal;
        }

        /// <summary>
        /// Map code
        /// </summary>
        private static CodeValue<T> MapTerm<T>(AuditTerm term)
        {
            if (term == null) return null;

            return new CodeValue<T>()
            {
                Code = term.Mnemonic,
                CodeSystem = term.Domain,
                DisplayName = term.DisplayName
            };
        }
        /// <summary>
        /// Map simple code
        /// </summary>
        private static Nullable<T> MapSimple<T>(object other, bool specified = true) where T : struct
        {
            if (!specified) return null;
            return new Nullable<T>((T)Enum.Parse(typeof(T), other.ToString()));
        }

        /// <summary>
        /// Convert to audit data from ATNA message
        /// </summary>
        public static Audit ToAudit(this AuditMessage me)
        {
            TraceSource traceSource = new TraceSource(SanteGuardConstants.TraceSourceName);
            Audit retVal = new Audit();
            retVal.ActionCode = MapOrCreateCode(me.EventIdentification.ActionCode);
            retVal.EventIdCode = MapOrCreateCode(me.EventIdentification.EventId);
            retVal.OutcomeCode = MapOrCreateCode(me.EventIdentification.EventOutcome);
            retVal.EventTypeCodes = me.EventIdentification.EventType.Select(o => MapOrCreateCode(o)).ToList();
            retVal.EventTimestamp = me.EventIdentification.EventDateTime;

            // Source
            if (me.SourceIdentification != null && me.SourceIdentification.Count > 0)
            {
                var auditSourcePs = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AuditSource>>();
                int tr = 0;
                var currentSources = me.SourceIdentification.Select(o => auditSourcePs.Query(s => s.AuditSourceId == o.AuditSourceID && s.EnterpriseSiteId == o.AuditEnterpriseSiteID, 0, 1, out tr, AuthenticationContext.Current.Principal).FirstOrDefault()).Where(o => o != null).FirstOrDefault();
                if (currentSources == null)
                    currentSources = auditSourcePs.Insert(new AuditSource()
                    {
                        EnterpriseSiteId = me.SourceIdentification.First().AuditEnterpriseSiteID,
                        AuditSourceId = me.SourceIdentification.First().AuditSourceID,
                        SourceType = me.SourceIdentification.First().AuditSourceTypeCode.Select(o => MapOrCreateCode(o)).ToList()
                    }, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                retVal.AuditSource = currentSources;
            }

            // Participants
            if (me.Actors != null)
            {
                var actorPs = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AuditActor>>();
                int tr = 0;
                retVal.Participants = me.Actors?.Select(a =>
                {

                    AuditActor act = actorPs.Query(o => o.UserName == a.UserName && o.NetworkAccessPoint == a.NetworkAccessPointId && o.UserIdentifier == a.UserIdentifier, AuthenticationContext.Current.Principal).FirstOrDefault();

                    if (act == null)
                    {

                        Guid? sid = null;
                        if (!String.IsNullOrEmpty(a.UserName ?? a.UserIdentifier ?? a.AlternativeUserId))
                            sid = ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>().GetUser(a.UserName ?? a.UserIdentifier ?? a.AlternativeUserId)?.Key;
                        else if (!String.IsNullOrEmpty(a.NetworkAccessPointId))
                            sid = ApplicationServiceContext.Current.GetService<IRepositoryService<SecurityDevice>>().Find(o => o.Name == a.NetworkAccessPointId).FirstOrDefault()?.Key;

                        // Create necessary 
                        act = actorPs.Insert(new AuditActor()
                        {
                            NetworkAccessPoint = a.NetworkAccessPointId,
                            NetworkAccessPointType = (SanteGuard.Model.NetworkAccessPointType)((int)a.NetworkAccessPointType),
                            UserIdentifier = a.UserIdentifier,
                            UserName = a.UserName,
                            SecurityIdentifier = sid
                        }, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                    }

                    return new AuditParticipation() { Actor = act, IsRequestor = a.UserIsRequestor, Roles = a.ActorRoleCode.Select(r => MapOrCreateCode(r)).ToList() };
                }).ToList();
            }

            // Objects 
            if (me.AuditableObjects != null)
            {
                retVal.Objects = me.AuditableObjects.Select(o => new AuditObject()
                {
                    Key = Guid.NewGuid(),
                    ExternalIdentifier = o.ObjectId,
                    IdTypeCode = MapOrCreateCode(o.IDTypeCode),
                    LifecycleCode = o.LifecycleTypeSpecified ? MapOrCreateCode(o.LifecycleType) : null,
                    RoleCode = o.RoleSpecified ? MapOrCreateCode(o.Role) : null,
                    TypeCode = o.TypeSpecified ? MapOrCreateCode(o.Type) : null,
                    Details = o.ObjectDetail.Select(d => new AuditObjectDetail() { Key = Guid.NewGuid(), DetailKey = d.Type, Value = d.Value }).ToList(),
                    Specification = !String.IsNullOrEmpty(o.ObjectSpec) ? new List<AuditObjectSpecification>() { new AuditObjectSpecification() { Key = Guid.NewGuid(), Specification = o.ObjectSpec, SpecificationType = o.ObjectSpecChoice == ObjectDataChoiceType.ParticipantObjectQuery ? "Q" : "N" } } : null
                }).ToList();
            }

            return retVal;
        }

        /// <summary>
        /// Convert internal service core audit data to persistence data
        /// </summary>
        public static Audit ToAudit(this AuditData me)
        {
            Audit retVal = new Audit();
            retVal.ActionCode = MapOrCreateCode(me.ActionCode);
            retVal.EventIdCode = MapOrCreateCode(me.EventIdentifier);
            retVal.OutcomeCode = MapOrCreateCode(me.Outcome);
            retVal.EventTypeCodes = new List<AuditTerm>() { MapOrCreateCode(me.EventTypeCode) };
            retVal.EventTimestamp = me.Timestamp;

            // Source
            var auditSourcePs = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AuditSource>>();
            var enterpriseMetadata = me.Metadata.FirstOrDefault(o => o.Key == AuditMetadataKey.EnterpriseSiteID)?.Value;
            if (String.IsNullOrEmpty(enterpriseMetadata))
                enterpriseMetadata = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<SanteGuardConfiguration>().DefaultEnterpriseSiteID;

            int tr = 0;
            var currentSources = auditSourcePs.Query(s => s.EnterpriseSiteId == enterpriseMetadata, 0, 1, out tr, AuthenticationContext.Current.Principal).FirstOrDefault();
            if (currentSources == null)
                currentSources = auditSourcePs.Insert(new AuditSource()
                {
                    EnterpriseSiteId = enterpriseMetadata,
                    AuditSourceId = Dns.GetHostName(),
                    SourceType = new List<AuditTerm>()
                    {
                        MapOrCreateCode(AtnaApi.Model.AuditSourceType.ApplicationServerProcess)
                    }
                }, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            retVal.AuditSource = currentSources;

            // Participants
            if (me.Actors != null)
            {
                var actorPs = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AuditActor>>();
                retVal.Participants = me.Actors.Select(a =>
                {
                    AuditActor act = actorPs.Query(o => o.UserName == a.UserName && o.NetworkAccessPoint == a.NetworkAccessPointId && o.UserIdentifier == a.UserIdentifier, AuthenticationContext.Current.Principal).FirstOrDefault();

                    if (act == null)
                    {

                        Guid? sid = null;
                        if (!String.IsNullOrEmpty(a.UserName ?? a.UserIdentifier ?? a.AlternativeUserId))
                            sid = ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>().GetUser(a.UserName ?? a.UserIdentifier ?? a.AlternativeUserId)?.Key;
                        else if (!String.IsNullOrEmpty(a.NetworkAccessPointId))
                            sid = ApplicationServiceContext.Current.GetService<IRepositoryService<SecurityDevice>>().Find(o => o.Name == a.NetworkAccessPointId).FirstOrDefault()?.Key;

                        // Create necessary 
                        act = actorPs.Insert(new AuditActor()
                        {
                            NetworkAccessPoint = a.NetworkAccessPointId,
                            NetworkAccessPointType = (SanteGuard.Model.NetworkAccessPointType)((int)a.NetworkAccessPointType),
                            UserIdentifier = a.UserIdentifier,
                            UserName = a.UserName,
                            SecurityIdentifier = sid
                        }, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                    }

                    return new AuditParticipation() { Actor = act, IsRequestor = a.UserIsRequestor, Roles = a.ActorRoleCode.Select(r => MapOrCreateCode(r)).ToList() };
                }).ToList();
            }

            // Objects 
            if (me.AuditableObjects != null)
            {
                retVal.Objects = me.AuditableObjects.Select(o => new AuditObject()
                {
                    ExternalIdentifier = o.ObjectId,
                    IdTypeCode = o.IDTypeCode.HasValue ? MapOrCreateCode(o.IDTypeCode) : MapOrCreateCode(o.CustomIdTypeCode),
                    LifecycleCode = o.LifecycleType.HasValue ? MapOrCreateCode(o.LifecycleType.Value) : null,
                    RoleCode = o.Role.HasValue ? MapOrCreateCode(o.Role.Value) : null,
                    TypeCode = MapOrCreateCode(o.Type),
                    Details = o.ObjectData.Select(d => new AuditObjectDetail() { DetailKey = d.Key, Value = d.Value }).ToList(),
                    Specification = new List<AuditObjectSpecification>() {
                        new AuditObjectSpecification() { Specification = o.QueryData, SpecificationType = "Q" },
                        new AuditObjectSpecification() { Specification = o.NameData, SpecificationType = "N" }
                    }.Where(s => !string.IsNullOrEmpty(s.Specification)).ToList()
                }).ToList();
            }

            // Extended data?
            foreach (var m in me.Metadata)
            {
                switch (m.Key)
                {
                    case AuditMetadataKey.AuditSourceID:
                        retVal.AuditSource.AuditSourceId = m.Value;
                        break;
                    case AuditMetadataKey.PID:
                        retVal.ProcessId = m.Value;
                        break;
                    case AuditMetadataKey.ProcessName:
                        retVal.ProcessName = m.Value;
                        break;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Convert to audit data
        /// </summary>
        public static AuditData ToAuditData(this Audit me)
        {
            var retVal = new AuditData(me.EventTimestamp.DateTime,
                (SanteDB.Core.Auditing.ActionType)MapTerm<AtnaApi.Model.ActionType>(me.ActionCode).StrongCode,
                (SanteDB.Core.Auditing.OutcomeIndicator)MapTerm<AtnaApi.Model.OutcomeIndicator>(me.ActionCode).StrongCode,
                (SanteDB.Core.Auditing.EventIdentifierType)MapTerm<AtnaApi.Model.EventIdentifierType>(me.ActionCode).StrongCode,
                new AuditCode(me.EventTypeCodes.FirstOrDefault()?.Mnemonic, me.EventTypeCodes.FirstOrDefault()?.Domain)
            );

            // TODO: Map other properties

            return retVal;
        }

    }

}
