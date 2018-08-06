using System;
using System.Linq;
using MARC.HI.EHRS.SVC.Core;
using MARC.HI.EHRS.SVC.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SanteDB.Core.Security;
using SanteGuard.Model;

namespace SanteGuard.Test
{
    [TestClass]
    public class AuditPersistenceTest : TestBase
    {

        /// <summary>
        /// Set context variables
        /// </summary>
        [ClassInitialize]
        public static void AuditPersistenceTestInit(TestContext context)
        {
            TestBase.SetContextVars(context);
        }

        /// <summary>
        /// Test that the persistence layer persists basic audits
        /// </summary>
        [TestMethod]
        public void TestCanLookupTerm()
        {
            var termService = ApplicationContext.Current.GetService<IDataPersistenceService<AuditTerm>>();
            Assert.IsNotNull(termService);

            // Lookup a term
            var codes = termService.Query(o => o.Mnemonic == "1" && o.Domain == "AuditableObjectType", AuthenticationContext.SystemPrincipal);
            Assert.AreEqual(1, codes.Count());
            Assert.AreEqual("person", codes.First().DisplayName.ToLower());

        }

        /// <summary>
        /// Tests that a simple audit can be inserted
        /// </summary>
        [TestMethod]
        public void TestCanInsertAudit()
        {
            var termService = ApplicationContext.Current.GetService<IDataPersistenceService<AuditTerm>>();
            Assert.IsNotNull(termService);

            AuditTerm actionCode = termService.Query(o => o.Mnemonic == "E" && o.Domain == "ActionType", AuthenticationContext.SystemPrincipal).First(),
                outcomeCode = termService.Query(o => o.Mnemonic == "0" && o.Domain == "OutcomeIndicator", AuthenticationContext.SystemPrincipal).First(),
                eventCode = termService.Query(o => o.Mnemonic == "110100", AuthenticationContext.SystemPrincipal).First(),
                objectType = termService.Query(o => o.Mnemonic == "1" && o.Domain == "AuditableObjectType", AuthenticationContext.SystemPrincipal).First(),
                objectRole = termService.Query(o => o.Mnemonic == "1" && o.Domain == "AuditableObjectRole", AuthenticationContext.SystemPrincipal).First(),
                objectLifecycle = termService.Query(o => o.Mnemonic == "1" && o.Domain == "AuditableObjectLifecycle", AuthenticationContext.SystemPrincipal).First(),
                objectIdType = termService.Query(o => o.Mnemonic == "1" && o.Domain == "AuditableObjectIdType", AuthenticationContext.SystemPrincipal).First();
            var auditUnderTest = new Audit()
            {
                ActionCode = actionCode,
                EventIdCode = eventCode,
                OutcomeCode = outcomeCode,
                AuditSource = new AuditSource()
                {
                    EnterpriseSiteId = "TEST",
                    AuditSourceId = "TEST"
                },
                EventTypeCodes = new System.Collections.Generic.List<AuditTerm>()
                {
                    new AuditTerm()
                    {
                        Mnemonic = "FOO", DisplayName = "BAR", Domain = "FOO CODES"
                    }
                },
                CorrelationToken = Guid.Empty,
                EventTimestamp = DateTimeOffset.Now,
                Objects = new System.Collections.Generic.List<AuditObject>()
                {
                    new AuditObject()
                    {
                        ExternalIdentifier = "blahblahtest",
                        IdTypeCode = objectIdType,
                        LifecycleCode = objectLifecycle,
                        RoleCode = objectRole,
                        TypeCode = objectType,
                        Specification = new System.Collections.Generic.List<AuditObjectSpecification>()
                        {
                            new AuditObjectSpecification()
                            {
                                Specification = "FOOFOO",
                                SpecificationType = "N"
                            }
                        },
                        Details = new System.Collections.Generic.List<AuditObjectDetail>()
                        {
                            new AuditObjectDetail()
                            {
                                Value = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                                DetailKey = "random"
                            }
                        }
                    }
                },
                Participants = new System.Collections.Generic.List<AuditParticipation>()
                {
                    new AuditParticipation()
                    {
                        IsRequestor = true,
                        Roles = new System.Collections.Generic.List<AuditTerm>()
                        {
                            new AuditTerm() { DisplayName = "Tester User Group", Domain = "GroupType", Mnemonic = "TESTERS"}
                        },
                        Actor = new AuditActor()
                        {
                            NetworkAccessPoint = "http://google.com",
                            NetworkAccessPointType = NetworkAccessPointType.Uri,
                            UserIdentifier = AuthenticationContext.SystemUserSid,
                            UserName = "SYSTEM"
                        }
                    }
                },
                ProcessId = "2039",
                ProcessName = "TestProcess.exe"
            };

            // Insert
            var audService = ApplicationContext.Current.GetService<IDataPersistenceService<Audit>>();
            Assert.IsNotNull(audService);
            var inserted = audService.Insert(auditUnderTest, AuthenticationContext.SystemPrincipal, TransactionMode.Commit);
            Assert.IsTrue(inserted.Key.HasValue);
            Assert.AreEqual(Guid.Empty, inserted.CorrelationToken);

        }
    }
}
