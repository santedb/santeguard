using SanteDB.Core.Services;
using SanteDB.Rest.AMI;
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
    /// Represents the audit resource handler
    /// </summary>
    public class AuditResourceHandler : ResourceHandlerBase<Audit>
    {
        /// <summary>
        /// DI constructor
        /// </summary>
        public AuditResourceHandler(ILocalizationService localizationService) : base(localizationService)
        {
        }

        /// <summary>
        /// Audit resource name
        /// </summary>
        public override string ResourceName => "SanteGuard.Audit";
    }
}
