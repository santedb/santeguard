using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanteGuard.Configuration
{
    /// <summary>
    /// Represents an STCP configuration
    /// </summary>
    [XmlType(nameof(StcpConfigurationElement), Namespace = "http://santedb.org/configuration/santeguard")]
    public class StcpConfigurationElement
    {
        /// <summary>
        /// Service certificates
        /// </summary>
        [XmlElement("serverCertificate")]
        public X509ConfigurationElement ServerCertificate { get; set; }

        /// <summary>
        /// Trusted client certificates
        /// </summary>
        [XmlElement("trustedClientCertificate")]
        public X509ConfigurationElement TrustedClientCertificates { get; set; }

    }
}
