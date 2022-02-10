using SanteDB.Core.Services;
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
    /// Represents an audit resource handler for audit source
    /// </summary>
    public class AuditSourceResourceHandler : ResourceHandlerBase<AuditSource>
    {
        /// <summary>
        /// DI constructor
        /// </summary>
        public AuditSourceResourceHandler(ILocalizationService localizationService) : base(localizationService)
        {
        }

        /// <summary>
        /// Gets the name of the resource
        /// </summary>
        public override string ResourceName => "SanteGuard.AuditSource";
    }
}
