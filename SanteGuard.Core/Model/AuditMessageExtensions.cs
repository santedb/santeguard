using AtnaApi.Model;
using MARC.HI.EHRS.SVC.Auditing.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Core.Model
{
    /// <summary>
    /// Audit message extensions
    /// </summary>
    public static class AuditMessageExtensions
    {

        /// <summary>
        /// Map code
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        private static AuditCode MapCode<T>(CodeValue<T> code)
        {
            return new AuditCode(code.Code, code.CodeSystem) { DisplayName = code.DisplayName };
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
        public static AuditData ToAuditData(this AuditMessage me)
        {
            // Construct the basic node
            var evtCode = me.EventIdentification.EventType?.FirstOrDefault();

            var retVal = new AuditData(me.EventIdentification.EventDateTime,
                MapSimple<MARC.HI.EHRS.SVC.Auditing.Data.ActionType>(me.EventIdentification.ActionCode).Value,
                MapSimple<MARC.HI.EHRS.SVC.Auditing.Data.OutcomeIndicator>(me.EventIdentification.EventOutcome).Value,
                MapSimple<MARC.HI.EHRS.SVC.Auditing.Data.EventIdentifierType>(me.EventIdentification.EventId).Value,
                MapCode(evtCode)
            );

            // Source identification
            var si = me.SourceIdentification?.FirstOrDefault();
            if (si != null)
            {
                retVal.AddMetadata(AuditMetadataKey.AuditSourceID, si.AuditSourceID);
                retVal.AddMetadata(AuditMetadataKey.EnterpriseSiteID, si.AuditEnterpriseSiteID);
                retVal.AddMetadata(AuditMetadataKey.AuditSourceType, si.AuditSourceTypeCode.FirstOrDefault()?.StrongCode.ToString());
            }

            // Actors
            foreach(var ptcpt in me.Actors)
            {
                var actDat = new MARC.HI.EHRS.SVC.Auditing.Data.AuditActorData()
                {
                    NetworkAccessPointId = ptcpt.NetworkAccessPointId,
                    NetworkAccessPointType = MapSimple<MARC.HI.EHRS.SVC.Auditing.Data.NetworkAccessPointType>(ptcpt.NetworkAccessPointType).Value,
                    AlternativeUserId = ptcpt.AlternativeUserId,
                    UserIdentifier = ptcpt.UserIdentifier,
                    UserIsRequestor = ptcpt.UserIsRequestor,
                    UserName = ptcpt.UserName
                };

                foreach (var rc in ptcpt.ActorRoleCode)
                    actDat.ActorRoleCode.Add(MapCode(rc));
                retVal.Actors.Add(actDat);
            }

            // Objects
            foreach(var obj in me.AuditableObjects)
            {

                // Try to parse code
                MARC.HI.EHRS.SVC.Auditing.Data.AuditableObjectIdType objIdType = MARC.HI.EHRS.SVC.Auditing.Data.AuditableObjectIdType.Custom;
                var hasCustomObjIdType = !Enum.TryParse<MARC.HI.EHRS.SVC.Auditing.Data.AuditableObjectIdType>(obj.IDTypeCode.Code, out objIdType);

                var audObj = new MARC.HI.EHRS.SVC.Auditing.Data.AuditableObject()
                {
                    IDTypeCode = hasCustomObjIdType ? MARC.HI.EHRS.SVC.Auditing.Data.AuditableObjectIdType.Custom : objIdType,
                    CustomIdTypeCode = hasCustomObjIdType ? MapCode(obj.IDTypeCode) : null,
                    LifecycleType = MapSimple<MARC.HI.EHRS.SVC.Auditing.Data.AuditableObjectLifecycle>(obj.LifecycleType, obj.LifecycleTypeSpecified),
                    ObjectId = obj.ObjectId,
                    Role = MapSimple<MARC.HI.EHRS.SVC.Auditing.Data.AuditableObjectRole>(obj.Role, obj.RoleSpecified),
                    Type = MapSimple<MARC.HI.EHRS.SVC.Auditing.Data.AuditableObjectType>(obj.Type, obj.TypeSpecified) ?? (MARC.HI.EHRS.SVC.Auditing.Data.AuditableObjectType)0,
                };

                if (!String.IsNullOrEmpty(obj.ObjectSpec)) {
                    if (obj.ObjectSpecChoice == ObjectDataChoiceType.ParticipantObjectName)
                        audObj.NameData = obj.ObjectSpec;
                    else
                        audObj.QueryData = obj.ObjectSpec;
                }

                // Object data
                foreach (var kv in obj.ObjectDetail)
                    if (!String.IsNullOrEmpty(kv.Type))
                        audObj.ObjectData.Add(new ObjectDataExtension(kv.Type, kv.Value));

                retVal.AuditableObjects.Add(audObj);
            }

            return retVal;
        }

    }
}
