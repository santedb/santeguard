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
using SanteDB.Core.Model;
using SanteDB.Core.Diagnostics;

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
            var retVal = auditTermService?.GetTerm(code.Code, code.CodeSystem, typeof(T).Name);
            if (retVal == null)
                retVal = auditTermService?.Register(code.Code, code.CodeSystem ?? typeof(T).Name, code.DisplayName);
            if (retVal == null)
                retVal = new AuditTerm()
                {
                    Domain = code.CodeSystem,
                    DisplayName = code.DisplayName,
                    LoadState = SanteDB.Core.Model.LoadState.New,
                    Mnemonic = code.Code
                };
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
            if (!specified || other == null) return null;

            if (other is AtnaApi.Model.CodeValue<T>)
            {
                if (Enum.TryParse<T>((other as AtnaApi.Model.CodeValue<T>).StrongCode.ToString(), out T retVal))
                    return retVal;
                else
                    return null;
            }
            else if (Enum.TryParse<T>(other.ToString(), out T retVal))
                return retVal;
            else
                return null;
        }

        /// <summary>
        /// Process the audit data
        /// </summary>
        public static AuditData ToAuditData(this AuditMessage me)
        {
            if (me == null)
                throw new ArgumentNullException("Audit message cannot be null");
            Tracer traceSource = Tracer.GetTracer(typeof(AuditMessageExtensions));
            AuditData retVal = new AuditData();
            retVal.ActionCode = MapSimple<SanteDB.Core.Auditing.ActionType>(me.EventIdentification.ActionCode).GetValueOrDefault();
            retVal.EventIdentifier = MapSimple<SanteDB.Core.Auditing.EventIdentifierType>(me.EventIdentification.EventId).GetValueOrDefault();
            retVal.Outcome = MapSimple<SanteDB.Core.Auditing.OutcomeIndicator>(me.EventIdentification.EventOutcome).GetValueOrDefault();
            retVal.EventTypeCode = me.EventIdentification.EventType.Select(o => new AuditCode(o.Code, o.CodeSystem) { DisplayName = o.DisplayName }).FirstOrDefault();
            retVal.Timestamp = me.EventIdentification.EventDateTime;

            // Source
            if (me.SourceIdentification != null && me.SourceIdentification.Count > 0)
            {
                var sourceId = me.SourceIdentification.FirstOrDefault();
                if (sourceId != null)
                {
                    retVal.AddMetadata(AuditMetadataKey.AuditSourceID, sourceId.AuditSourceID);
                    retVal.AddMetadata(AuditMetadataKey.AuditSourceType, sourceId.AuditSourceTypeCode.FirstOrDefault()?.Code);
                    retVal.AddMetadata(AuditMetadataKey.EnterpriseSiteID, sourceId.AuditEnterpriseSiteID);
                }
            }

            // Participants
            if (me.Actors != null)
            {

                retVal.Actors = me.Actors?.Select(a => new SanteDB.Core.Auditing.AuditActorData()
                {
                    NetworkAccessPointId = a.NetworkAccessPointId,
                    NetworkAccessPointType = MapSimple<SanteDB.Core.Auditing.NetworkAccessPointType>(a.NetworkAccessPointType).GetValueOrDefault(),
                    UserIdentifier = a.UserIdentifier,
                    UserName = a.UserName,
                    ActorRoleCode = a.ActorRoleCode.Select(o => new AuditCode(o.Code, o.CodeSystem) { DisplayName = o.DisplayName }).ToList(),
                    UserIsRequestor = a.UserIsRequestor,
                    AlternativeUserId = a.AlternativeUserId
                }).ToList();
            }

            // Objects 
            if (me.AuditableObjects != null)
            {
                retVal.AuditableObjects = me.AuditableObjects.Select(o => new SanteDB.Core.Auditing.AuditableObject()
                {
                    ObjectId = o.ObjectId,
                    IDTypeCode = MapSimple<SanteDB.Core.Auditing.AuditableObjectIdType>(o.IDTypeCode),
                    LifecycleType = MapSimple<SanteDB.Core.Auditing.AuditableObjectLifecycle>(o.LifecycleType, o.LifecycleTypeSpecified),
                    Role = MapSimple<SanteDB.Core.Auditing.AuditableObjectRole>(o.Role, o.RoleSpecified),
                    Type = MapSimple<SanteDB.Core.Auditing.AuditableObjectType>(o.Type, o.TypeSpecified) ?? SanteDB.Core.Auditing.AuditableObjectType.Other,
                    CustomIdTypeCode = new AuditCode(o.IDTypeCode.Code, o.IDTypeCode.CodeSystem) { DisplayName = o.IDTypeCode.DisplayName },
                    ObjectData = o.ObjectDetail.Select(d => new ObjectDataExtension(d.Type, d.Value)).ToList(),
                    NameData = o.ObjectSpecChoice == ObjectDataChoiceType.ParticipantObjectName ? o.ObjectSpec : null,
                    QueryData = o.ObjectSpecChoice == ObjectDataChoiceType.ParticipantObjectQuery ? o.ObjectSpec : null
                }).ToList();
            }

            traceSource.TraceInfo("Successfully processed audit: {0}", retVal.ToDisplay());
            return retVal;
        }

        /// <summary>
        /// Convert to audit data from ATNA message
        /// </summary>
        public static Audit ToAudit(this AuditMessage me)
        {
            if (me == null)
                throw new ArgumentNullException("Audit message cannot be null");
            Tracer traceSource = Tracer.GetTracer(typeof(AuditMessageExtensions));
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

                if (auditSourcePs != null)
                {
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
                else
                    retVal.AuditSource = new AuditSource()
                    {
                        EnterpriseSiteId = me.SourceIdentification.First().AuditEnterpriseSiteID,
                        AuditSourceId = me.SourceIdentification.First().AuditSourceID,
                        SourceType = me.SourceIdentification.First().AuditSourceTypeCode.Select(o => MapOrCreateCode(o)).ToList()
                    };
            }

            // Participants
            if (me.Actors != null)
            {
                var actorPs = ApplicationServiceContext.Current.GetService<IDataPersistenceService<AuditActor>>();

                retVal.Participants = me.Actors?.Select(a =>
                {
                    AuditActor act = null;

                    // No persistence service just translate
                    if (actorPs != null)
                    {
                        act = actorPs.Query(o => o.UserName == a.UserName && o.NetworkAccessPoint == a.NetworkAccessPointId && o.UserIdentifier == a.UserIdentifier, AuthenticationContext.Current.Principal).FirstOrDefault();

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
                    }
                    else
                        act = new AuditActor()
                        {
                            NetworkAccessPoint = a.NetworkAccessPointId,
                            NetworkAccessPointType = (SanteGuard.Model.NetworkAccessPointType)((int)a.NetworkAccessPointType),
                            UserIdentifier = a.UserIdentifier,
                            UserName = a.UserName,
                        };

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

            traceSource.TraceInfo("Successfully processed audit: {0}", retVal.ToDisplay());
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
                (SanteDB.Core.Auditing.ActionType)MapTerm<AtnaApi.Model.ActionType>(me.LoadProperty<AuditTerm>(nameof(Audit.ActionCode))).StrongCode,
                (SanteDB.Core.Auditing.OutcomeIndicator)MapTerm<AtnaApi.Model.OutcomeIndicator>(me.LoadProperty<AuditTerm>(nameof(Audit.OutcomeCode))).StrongCode,
                (SanteDB.Core.Auditing.EventIdentifierType)MapTerm<AtnaApi.Model.EventIdentifierType>(me.LoadProperty<AuditTerm>(nameof(me.EventIdCode))).StrongCode,
                new AuditCode(me.EventTypeCodes.FirstOrDefault()?.Mnemonic, me.EventTypeCodes.FirstOrDefault()?.Domain)
            )
            { Key = me.Key };

            // Map event
            retVal.Actors = me.Participants.Select(o => new SanteDB.Core.Auditing.AuditActorData()
            {
                ActorRoleCode = o.Roles.Select(r => MapTermToCode(r)).ToList(),
                UserIsRequestor = o.IsRequestor,
                UserIdentifier = o.Actor?.UserIdentifier,
                NetworkAccessPointId = o.Actor?.NetworkAccessPoint,
                NetworkAccessPointType = (SanteDB.Core.Auditing.NetworkAccessPointType)(int)(o.Actor?.NetworkAccessPointType ?? SanteGuard.Model.NetworkAccessPointType.Uri),
                UserName = o.Actor?.UserName,
            }).ToList();

            // TODO: Map additional fields

            return retVal;
        }

        /// <summary>
        /// Map term to code
        /// </summary>
        private static AuditCode MapTermToCode(AuditTerm r)
        {
            return new AuditCode(r.Mnemonic, r.Domain)
            {
                DisplayName = r.DisplayName
            };
        }
    }

}
