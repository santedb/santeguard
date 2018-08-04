using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Map;
using SanteGuard.Model;
using SanteGuard.Persistence.Ado.Data.Model;

namespace SanteGuard.Test
{
    [TestClass]
    public class ModelMapTest
    {

        static ModelMapper m_mapper = null;
        
        /// <summary>
        /// Model map test
        /// </summary>
        static ModelMapTest() {
            try
            {
                m_mapper = new ModelMapper(typeof(DbAudit).Assembly.GetManifestResourceStream("SanteGuard.Persistence.Ado.Data.Map.ModelMap.xml"));
            }
            catch(ModelMapValidationException e)
            {
                foreach (var d in e.ValidationDetails)
                    Trace.TraceError("{0}: {1} @ {2}", d.Level, d.Message, d.Location);
            }
        }

        /// <summary>
        /// Test that you can generate a simple map for audit
        /// </summary>
        [TestMethod]
        public void TestGenerateMapForSimpleAudit()
        {
            var mapped = m_mapper.MapModelExpression<Audit, DbAuditVersion>(o => o.Status == AuditStatusType.New);
            Assert.AreEqual(mapped, "o => (o.Status == 0)");
        }

        /// <summary>
        /// Simple test of SQL
        /// </summary>
        [TestMethod]
        public void TestGenerateSqlForAuditSimple()
        {
            var qbuilder = new SanteDB.OrmLite.QueryBuilder(m_mapper, new SanteDB.OrmLite.Providers.FirebirdSQLProvider());
            var sql = qbuilder.CreateQuery<Audit>(o => o.Status == AuditStatusType.New);
            Assert.AreEqual("SELECT aud_vrsn_tbl.aud_id,aud_vrsn_tbl.aud_vrsn_id,aud_vrsn_tbl.replc_vrsn_id,aud_vrsn_tbl.sts_cd_id,aud_vrsn_tbl.is_alrt,aud_vrsn_tbl.crt_usr_id,aud_vrsn_tbl.obslt_usr_id,aud_vrsn_tbl.crt_utc,aud_vrsn_tbl.obslt_utc,aud_tbl.aud_seq_id,aud_tbl.corr_id,aud_tbl.act_cd_id,aud_tbl.out_cd_id,aud_tbl.evt_cd_id,aud_tbl.evt_utc,aud_tbl.ses_id,aud_tbl.ps_name,aud_tbl.ps_id  FROM aud_vrsn_tbl AS aud_vrsn_tbl INNER JOIN aud_tbl AS aud_tbl ON (aud_vrsn_tbl.aud_id = aud_tbl.aud_id )WHERE (aud_vrsn_tbl.sts_cd_id = ? )", sql.Build().SQL);
        }

        /// <summary>
        /// Simple SQL should join tables
        /// </summary>
        [TestMethod]
        public void TestGenerateSqlForAuditWithJoin()
        {
            var e = Guid.Empty;
            var qbuilder = new SanteDB.OrmLite.QueryBuilder(m_mapper, new SanteDB.OrmLite.Providers.FirebirdSQLProvider());
            var sql = qbuilder.CreateQuery<Audit>(o => o.Status == AuditStatusType.New && o.EventIdCodeKey == e);
            Assert.AreEqual("SELECT aud_vrsn_tbl.aud_id,aud_vrsn_tbl.aud_vrsn_id,aud_vrsn_tbl.replc_vrsn_id,aud_vrsn_tbl.sts_cd_id,aud_vrsn_tbl.is_alrt,aud_vrsn_tbl.crt_usr_id,aud_vrsn_tbl.obslt_usr_id,aud_vrsn_tbl.crt_utc,aud_vrsn_tbl.obslt_utc,aud_tbl.aud_seq_id,aud_tbl.corr_id,aud_tbl.act_cd_id,aud_tbl.out_cd_id,aud_tbl.evt_cd_id,aud_tbl.evt_utc,aud_tbl.ses_id,aud_tbl.ps_name,aud_tbl.ps_id  FROM aud_vrsn_tbl AS aud_vrsn_tbl INNER JOIN aud_tbl AS aud_tbl ON (aud_vrsn_tbl.aud_id = aud_tbl.aud_id )WHERE (aud_vrsn_tbl.sts_cd_id = ? ) AND (aud_tbl.evt_cd_id = ? )", sql.Build().SQL);
        }

        /// <summary>
        /// Simple SQL should join tables for audit term
        /// </summary>
        [TestMethod]
        public void TestGenerateSqlForAuditWithJoinForTerm()
        {
            var e = Guid.Empty;
            var qbuilder = new SanteDB.OrmLite.QueryBuilder(m_mapper, new SanteDB.OrmLite.Providers.PostgreSQLProvider());
            var sql = qbuilder.CreateQuery<Audit>(o => o.Status == AuditStatusType.New && o.EventIdCodeKey == e && o.OutcomeCode.Mnemonic == "SUCCESS");
            Assert.AreEqual("SELECT *  FROM aud_vrsn_tbl AS aud_vrsn_tbl INNER JOIN aud_tbl AS aud_tbl ON (aud_vrsn_tbl.aud_id = aud_tbl.aud_id )WHERE (aud_vrsn_tbl.sts_cd_id = ? ) AND (aud_tbl.evt_cd_id = ? ) AND EXISTS (SELECT sq0aud_cd_tbl.cd_id  FROM aud_cd_tbl AS sq0aud_cd_tbl WHERE (sq0aud_cd_tbl.mnemonic = ? ) AND aud_tbl.out_cd_id = sq0aud_cd_tbl.cd_id)", sql.Build().SQL);
        }

        /// <summary>
        /// Simple SQL should join tables for audit term
        /// </summary>
        [TestMethod]
        public void TestGenerateSqlForAuditWithJoinForParticipant()
        {
            var e = Guid.Empty;
            var qbuilder = new SanteDB.OrmLite.QueryBuilder(m_mapper, new SanteDB.OrmLite.Providers.PostgreSQLProvider());
            var sql = qbuilder.CreateQuery<Audit>(o => o.OutcomeCode.Mnemonic == "SUCCESS" && o.Participants.Any(p=>p.Actor.UserName == "fyfej"));
            Assert.AreEqual("SELECT *  FROM aud_vrsn_tbl AS aud_vrsn_tbl INNER JOIN aud_tbl AS aud_tbl ON (aud_vrsn_tbl.aud_id = aud_tbl.aud_id )WHERE EXISTS (SELECT sq0aud_cd_tbl.cd_id  FROM aud_cd_tbl AS sq0aud_cd_tbl WHERE (sq0aud_cd_tbl.mnemonic = ? ) AND aud_tbl.out_cd_id = sq0aud_cd_tbl.cd_id) AND  EXISTS (SELECT sq0aud_ptcpt_aud_assoc_tbl.aud_id  FROM aud_ptcpt_aud_assoc_tbl AS sq0aud_ptcpt_aud_assoc_tbl WHERE EXISTS (SELECT sq1aud_ptcpt_tbl.ptcpt_id  FROM aud_ptcpt_tbl AS sq1aud_ptcpt_tbl WHERE (sq1aud_ptcpt_tbl.usr_name = ? ) AND sq0aud_ptcpt_aud_assoc_tbl.ptcpt_id = sq1aud_ptcpt_tbl.ptcpt_id) AND aud_vrsn_tbl.aud_id = sq0aud_ptcpt_aud_assoc_tbl.aud_id)", sql.Build().SQL);

        }

        /// <summary>
        /// Test complex mapping through associative tables
        /// </summary>
        [TestMethod]
        public void TestGenerateSqlForAuditSourceWithSimpleSource()
        {
            var e = Guid.Empty;
            var qbuilder = new SanteDB.OrmLite.QueryBuilder(m_mapper, new SanteDB.OrmLite.Providers.PostgreSQLProvider());
            var sql = qbuilder.CreateQuery<Audit>(o => o.AuditSource.EnterpriseSiteId == "FOO");
            Assert.AreEqual("SELECT *  FROM aud_vrsn_tbl AS aud_vrsn_tbl INNER JOIN aud_tbl AS aud_tbl ON (aud_vrsn_tbl.aud_id = aud_tbl.aud_id )INNER JOIN aud_src_tbl AS aud_src_tbl ON (aud_tbl.src_id = aud_src_tbl.aud_src_id )WHERE EXISTS (SELECT sq0aud_src_tbl.aud_src_id  FROM aud_src_tbl AS sq0aud_src_tbl WHERE sq0aud_src_tbl.aud_src_id IN (SELECT sq1aud_src_typ_tbl.aud_src_id FROM aud_src_typ_tbl AS sq1aud_src_typ_tbl WHERE  EXISTS (SELECT sq1aud_cd_tbl.cd_id  FROM aud_cd_tbl AS sq1aud_cd_tbl WHERE (sq1aud_cd_tbl.mnemonic = ? ) AND sq1aud_src_typ_tbl.cd_id = sq1aud_cd_tbl.cd_id)) AND aud_tbl.src_id = sq0aud_src_tbl.aud_src_id)", sql.Build().SQL);
        }

        /// <summary>
        /// Test complex mapping through associative tables
        /// </summary>
        [TestMethod]
        public void TestGenerateSqlForAuditSourceWithRole()
        {
            var e = Guid.Empty;
            var qbuilder = new SanteDB.OrmLite.QueryBuilder(m_mapper, new SanteDB.OrmLite.Providers.PostgreSQLProvider());
            var sql = qbuilder.CreateQuery<Audit>(o => o.AuditSource.SourceType.Any(r=>r.Mnemonic == "FOOD TRUCK"));
            Assert.AreEqual("SELECT *  FROM aud_vrsn_tbl AS aud_vrsn_tbl INNER JOIN aud_tbl AS aud_tbl ON (aud_vrsn_tbl.aud_id = aud_tbl.aud_id )INNER JOIN aud_src_tbl AS aud_src_tbl ON (aud_tbl.src_id = aud_src_tbl.aud_src_id )WHERE EXISTS (SELECT sq0aud_src_tbl.aud_src_id  FROM aud_src_tbl AS sq0aud_src_tbl WHERE sq0aud_src_tbl.aud_src_id IN (SELECT sq1aud_src_typ_tbl.aud_src_id FROM aud_src_typ_tbl AS sq1aud_src_typ_tbl WHERE  EXISTS (SELECT sq1aud_cd_tbl.cd_id  FROM aud_cd_tbl AS sq1aud_cd_tbl WHERE (sq1aud_cd_tbl.mnemonic = ? ) AND sq1aud_src_typ_tbl.cd_id = sq1aud_cd_tbl.cd_id)) AND aud_tbl.src_id = sq0aud_src_tbl.aud_src_id)", sql.Build().SQL);
        }
    }
}
