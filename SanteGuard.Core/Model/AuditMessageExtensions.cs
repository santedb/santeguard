using AtnaApi.Model;
using MARC.HI.EHRS.SVC.Auditing.Data;
using MARC.HI.EHRS.SVC.Core;
using SanteDB.Core.Services;
using SanteGuard.Model;
using SanteGuard.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

            var auditTermService = ApplicationContext.Current.GetService<IAuditTermLookupService>();
            if (auditTermService == null)
                throw new InvalidOperationException("Cannot locate audit term lookup");

            var retVal = auditTermService.GetTerm(code.Code, code.CodeSystem, typeof(T).Name);
            if (retVal != null)
                retVal = auditTermService.Register(code.Code, code.CodeSystem ?? typeof(T).Name, code.DisplayName);
            return retVal;
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
            Audit retVal = new Audit();
            retVal.ActionCode = MapOrCreateCode(me.EventIdentification.ActionCode);
            retVal.EventIdCode = MapOrCreateCode(me.EventIdentification.EventId);
            retVal.OutcomeCode = MapOrCreateCode(me.EventIdentification.EventOutcome);
            retVal.EventTypeCodes = me.EventIdentification.EventType.Select(o => MapOrCreateCode(o)).ToList();
            retVal.EventTimestamp = me.EventIdentification.EventDateTime;

            // Source
            if (me.SourceIdentification != null && me.SourceIdentification.Count > 0) {
                var auditSourcePs = ApplicationContext.Current.GetService<IRepositoryService<AuditSource>>();
                var currentSources = me.SourceIdentification.Select(o => auditSourcePs.Find(s => s.AuditSourceId == o.AuditSourceID && s.EnterpriseSiteId == o.AuditEnterpriseSiteID).FirstOrDefault()).Where(o=>o!= null).FirstOrDefault();
                if (currentSources == null)
                    currentSources = auditSourcePs.Insert(new AuditSource()
                    {
                        EnterpriseSiteId = me.SourceIdentification.First().AuditEnterpriseSiteID,
                        AuditSourceId = me.SourceIdentification.First().AuditSourceID,
                        SourceType = me.SourceIdentification.First().AuditSourceTypeCode.Select(o => MapOrCreateCode(o)).ToList()
                    });
                retVal.AuditSource = currentSources;
            }

            // Participants
            if(me.Actors != null)
            {
                var actorPs = ApplicationContext.Current.GetService<IRepositoryService<AuditActor>>();
                retVal.Participants = me.Actors.Select(a => {
                    var act = actorPs.Find(o => o.UserIdentifier == a.UserIdentifier && o.UserName == a.UserName && o.NetworkAccessPoint == a.NetworkAccessPointId).FirstOrDefault();
                    if (act == null) {
                        // Create necessary 
                        act = actorPs.Insert(new AuditActor()
                        {
                            NetworkAccessPoint = a.NetworkAccessPointId,
                            NetworkAccessPointType = (int)a.NetworkAccessPointType,
                            UserIdentifier = a.UserIdentifier,
                            UserName = a.UserName,
                            SecurityIdentifier = ApplicationContext.Current.GetService<ISecurityRepositoryService>().GetUser(a.UserName ?? a.UserIdentifier ?? a.AlternativeUserId).Key ??
                                ApplicationContext.Current.GetService<ISecurityRepositoryService>().FindDevices(o => o.Name == a.NetworkAccessPointId).FirstOrDefault()?.Key,
                        });
                    }

                    return new AuditParticipation() { Actor = act, IsRequestor = a.UserIsRequestor, Roles = a.ActorRoleCode.Select(r => MapOrCreateCode(r)).ToList() }; 
                }).ToList();
            }

            // Objects 
            if(me.AuditableObjects != null)
            {
                retVal.Objects = me.AuditableObjects.Select(o => new AuditObject()
                {
                    ExternalIdentifier = o.ObjectId,
                    IdTypeCode = MapOrCreateCode(o.IDTypeCode),
                    LifecycleCode = o.LifecycleTypeSpecified ? MapOrCreateCode(o.LifecycleType) : null,
                    RoleCode = o.RoleSpecified ? MapOrCreateCode(o.Role) : null,
                    TypeCode = o.TypeSpecified ? MapOrCreateCode(o.Type) : null,
                    Details = o.ObjectDetail.Select(d=>new AuditObjectDetail() {  DetailKey = d.Type, Value = d.Value }).ToList(),
                    Specification = !String.IsNullOrEmpty(o.ObjectSpec) ? new List<AuditObjectSpecification>() { new AuditObjectSpecification() { Specification = o.ObjectSpec, SpecificationType = o.ObjectSpecChoice == ObjectDataChoiceType.ParticipantObjectQuery ? 'Q' : 'N' } } : null
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
            var auditSourcePs = ApplicationContext.Current.GetService<IRepositoryService<AuditSource>>();
            var enterpriseMetadata = me.Metadata.FirstOrDefault(o => o.Key == AuditMetadataKey.EnterpriseSiteID)?.Value;
            if (String.IsNullOrEmpty(enterpriseMetadata))
                enterpriseMetadata = String.Format("{1}^^^&{0}&ISO", ApplicationContext.Current.Configuration.DeviceIdentifier, ApplicationContext.Current.Configuration.DeviceName);

            var currentSources = auditSourcePs.Find(s => s.EnterpriseSiteId == enterpriseMetadata).FirstOrDefault();
            if (currentSources == null)
                currentSources = auditSourcePs.Insert(new AuditSource()
                {
                    EnterpriseSiteId = enterpriseMetadata,
                    AuditSourceId = Dns.GetHostName(),
                    SourceType = new List<AuditTerm>()
                    {
                        MapOrCreateCode(AtnaApi.Model.AuditSourceType.ApplicationServerProcess)
                    }
                });
            retVal.AuditSource = currentSources;

            // Participants
            if (me.Actors != null)
            {
                var actorPs = ApplicationContext.Current.GetService<IRepositoryService<AuditActor>>();
                retVal.Participants = me.Actors.Select(a => {
                    var act = actorPs.Find(o => o.UserIdentifier == a.UserIdentifier && o.UserName == a.UserName && o.NetworkAccessPoint == a.NetworkAccessPointId).FirstOrDefault();
                    if (act == null)
                    {
                        // Create necessary 
                        act = actorPs.Insert(new AuditActor()
                        {
                            NetworkAccessPoint = a.NetworkAccessPointId,
                            NetworkAccessPointType = (int)a.NetworkAccessPointType,
                            UserIdentifier = a.UserIdentifier,
                            UserName = a.UserName,
                            SecurityIdentifier = ApplicationContext.Current.GetService<ISecurityRepositoryService>().GetUser(a.UserName ?? a.UserIdentifier ?? a.AlternativeUserId).Key ??
                                ApplicationContext.Current.GetService<ISecurityRepositoryService>().FindDevices(o => o.Name == a.NetworkAccessPointId).FirstOrDefault()?.Key,
                        });
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
                        new AuditObjectSpecification() { Specification = o.QueryData, SpecificationType = 'Q' },
                        new AuditObjectSpecification() { Specification = o.NameData, SpecificationType = 'N' }
                    }.Where(s=>!string.IsNullOrEmpty(s.Specification)).ToList()
                }).ToList();
            }

            // Extended data?
            foreach(var m in me.Metadata)
            {
                switch(m.Key)
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
    }
}
