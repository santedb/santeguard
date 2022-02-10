﻿using SanteDB.Core.Services;
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
    /// Audit terminology resource handler
    /// </summary>
    public class AuditTermResourceHandler : ResourceHandlerBase<AuditTerm>
    {
        /// <summary>
        /// DI constructor
        /// </summary>
        public AuditTermResourceHandler(ILocalizationService localizationService) : base(localizationService)
        {
        }


        /// <summary>
        /// Audit term
        /// </summary>
        public override string ResourceName => "SanteGuard.AuditTerm";
    }
}
