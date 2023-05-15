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
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using SanteDB.Core.Model;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Audit;

namespace SanteGuard.Messaging.Syslog
{
    /// <summary>
    /// Audit message extensions
    /// </summary>
    public static class AuditMessageExtensions
    {
        /// <summary>
        /// Map or create a simple code
        /// </summary>
        private static SanteDB.Core.Model.Audit.AuditCode MapOrCreateCode<T>(T simpleCode)
        {
            return MapOrCreateCode(new CodeValue<T>(simpleCode));
        }

        /// <summary>
        /// Map or create a simple code
        /// </summary>
        private static SanteDB.Core.Model.Audit.AuditCode MapOrCreateCode(AtnaApi.Model.AuditCode code)
        {
            if (code == null) return null;
            return MapOrCreateCode(new CodeValue<string>(code.Code, code.CodeSystem, code.DisplayName));
        }

        /// <summary>
        /// Map code
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        private static SanteDB.Core.Model.Audit.AuditCode MapOrCreateCode<T>(CodeValue<T> code)
        {
            if (code == null) return null;

            return new SanteDB.Core.Model.Audit.AuditCode()
            {
                CodeSystem = code.CodeSystem,
                DisplayName = code.DisplayName,
                Code = code.Code
            };
        }


        /// <summary>
        /// Map simple code
        /// </summary>
        private static T? MapSimple<T>(object other, bool specified = true) where T : struct
        {
            if (!specified || other == null) return null;

            if (other is CodeValue<T>)
            {
                if (Enum.TryParse((other as CodeValue<T>).StrongCode.ToString(), out T retVal))
                    return retVal;
                else
                    return null;
            }
            else if (Enum.TryParse(other.ToString(), out T retVal))
                return retVal;
            else
                return null;
        }

        /// <summary>
        /// Process the audit data
        /// </summary>
        public static AuditEventData ToAuditData(this AuditMessage me)
        {
            if (me == null)
                throw new ArgumentNullException("Audit message cannot be null");
            Tracer traceSource = Tracer.GetTracer(typeof(AuditMessageExtensions));
            AuditEventData retVal = new AuditEventData();
            retVal.ActionCode = MapSimple<SanteDB.Core.Model.Audit.ActionType>(me.EventIdentification.ActionCode).GetValueOrDefault();
            retVal.EventIdentifier = MapSimple<SanteDB.Core.Model.Audit.EventIdentifierType>(me.EventIdentification.EventId).GetValueOrDefault();
            retVal.Outcome = MapSimple<SanteDB.Core.Model.Audit.OutcomeIndicator>(me.EventIdentification.EventOutcome).GetValueOrDefault();
            retVal.EventTypeCode = me.EventIdentification.EventType.Select(o => new SanteDB.Core.Model.Audit.AuditCode(o.Code, o.CodeSystem) { DisplayName = o.DisplayName }).FirstOrDefault();
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

                retVal.Actors = me.Actors?.Select(a => new SanteDB.Core.Model.Audit.AuditActorData()
                {
                    NetworkAccessPointId = a.NetworkAccessPointId,
                    NetworkAccessPointType = MapSimple<SanteDB.Core.Model.Audit.NetworkAccessPointType>(a.NetworkAccessPointType).GetValueOrDefault(),
                    UserIdentifier = a.UserIdentifier,
                    UserName = a.UserName,
                    ActorRoleCode = a.ActorRoleCode.Select(o => new SanteDB.Core.Model.Audit.AuditCode(o.Code, o.CodeSystem) { DisplayName = o.DisplayName }).ToList(),
                    UserIsRequestor = a.UserIsRequestor,
                    AlternativeUserId = a.AlternativeUserId
                }).ToList();
            }

            // Objects 
            if (me.AuditableObjects != null)
            {
                retVal.AuditableObjects = me.AuditableObjects.Select(o => new SanteDB.Core.Model.Audit.AuditableObject()
                {
                    ObjectId = o.ObjectId,
                    IDTypeCode = MapSimple<SanteDB.Core.Model.Audit.AuditableObjectIdType>(o.IDTypeCode),
                    LifecycleType = MapSimple<SanteDB.Core.Model.Audit.AuditableObjectLifecycle>(o.LifecycleType, o.LifecycleTypeSpecified),
                    Role = MapSimple<SanteDB.Core.Model.Audit.AuditableObjectRole>(o.Role, o.RoleSpecified),
                    Type = MapSimple<SanteDB.Core.Model.Audit.AuditableObjectType>(o.Type, o.TypeSpecified) ?? SanteDB.Core.Model.Audit.AuditableObjectType.Other,
                    CustomIdTypeCode = new SanteDB.Core.Model.Audit.AuditCode(o.IDTypeCode.Code, o.IDTypeCode.CodeSystem) { DisplayName = o.IDTypeCode.DisplayName },
                    ObjectData = o.ObjectDetail.Select(d => new ObjectDataExtension(d.Type, d.Value)).ToList(),
                    NameData = o.ObjectSpecChoice == ObjectDataChoiceType.ParticipantObjectName ? o.ObjectSpec : null,
                    QueryData = o.ObjectSpecChoice == ObjectDataChoiceType.ParticipantObjectQuery ? o.ObjectSpec : null
                }).ToList();
            }

            traceSource.TraceInfo("Successfully processed audit: {0}", retVal.ToDisplay());
            return retVal;
        }

       
    }

}
