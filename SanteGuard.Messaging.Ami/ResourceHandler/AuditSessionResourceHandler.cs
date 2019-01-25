using SanteDB.Rest.AMI.Resources;
using SanteGuard.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteGuard.Messaging.Ami.ResourceHandler
{
    /// <summary>
    /// AMI Resource HAndler for audit sessions
    /// </summary>
    public class AuditSessionResourceHandler : ResourceHandlerBase<AuditSession>
    {

        /// <summary>
        /// Gets the name of the resource
        /// </summary>
        public override string ResourceName => "SanteGuard.AuditSession";
    }
}
