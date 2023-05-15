using SanteDB.Core.Configuration;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Security;
using SanteDB.Docker.Core;
using SanteGuard.Messaging.Syslog;
using SanteGuard.Messaging.Syslog.Action;
using SanteGuard.Messaging.Syslog.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace SanteGuard.Docker
{
    /// <summary>
    /// Docker feature
    /// </summary>
    public class SanteGuardDockerFeature : IDockerFeature
    {

        /// <summary>
        /// Binding for the configuration
        /// </summary>
        public const string BINDING_CONFIG_NAME = "BIND";
        /// <summary>
        /// Default enterprise configuration
        /// </summary>
        public const string ENTERPRISE_CONFIG_NAME = "ENTID";
        /// <summary>
        /// For forwarding SYSLOG messages
        /// </summary>
        public const string FORWARD_ADDRESS_CONFIG = "FORWARD";
        /// <summary>
        /// Maximum size
        /// </summary>
        public const string MAX_SIZE_CONFIG = "MAX_SIZE";
        /// <summary>
        /// STCP certificate configuration
        /// </summary>
        public const string STCP_CERT_CONFIG = "STCP_CERT";
        /// <inheritdoc/>
        public string Id => "SG";

        /// <inheritdoc/>
        public IEnumerable<string> Settings => new string[] { BINDING_CONFIG_NAME, ENTERPRISE_CONFIG_NAME };

        /// <inheritdoc/>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {
            var sgConfig = configuration.GetSection<SanteGuardConfiguration>();
            if(sgConfig == null)
            {
                sgConfig = new SanteGuardConfiguration();
                configuration.AddSection(sgConfig);
            }

            if(settings.TryGetValue(ENTERPRISE_CONFIG_NAME, out var entId))
            {
                sgConfig.DefaultEnterpriseSiteID = entId;
            }
            else
            {
                sgConfig.DefaultEnterpriseSiteID = $"{Environment.MachineName}^^^SanteDB.Docker.Server";
            }

            if(!settings.TryGetValue(BINDING_CONFIG_NAME, out var bindingRaw) || !Uri.TryCreate(bindingRaw, UriKind.Absolute, out var binding))
            {
                throw new ConfigurationException($"Configuration {BINDING_CONFIG_NAME} must be a URI and must be provided", configuration);
            }
            int maxSize = 8096;
            if(settings.TryGetValue(MAX_SIZE_CONFIG, out var maxSizeRaw) && !Int32.TryParse(maxSizeRaw, out maxSize))
            {
                throw new ConfigurationException($"Configuration {MAX_SIZE_CONFIG} must integer", configuration);
            }

            sgConfig.Endpoints = sgConfig.Endpoints ?? new List<EndpointConfiguration>();
            var dockerEp = new EndpointConfiguration()
            {
                Address = binding,
                MaxSize = maxSize,
                Name = "DOCKER_CONFING",
                LogFileLocation = "/var/log/santedb.syslog.log"
            };
            dockerEp.Action.Add(new TypeReferenceConfiguration(typeof(LogAction)));
            dockerEp.Action.Add(new TypeReferenceConfiguration(typeof(StorageAction)));
            if(settings.TryGetValue(FORWARD_ADDRESS_CONFIG, out var forwardAddressRaw))
            {
                dockerEp.Forward.Add(forwardAddressRaw);
                dockerEp.Action.Add(new TypeReferenceConfiguration(typeof(ForwardAction)));
            }

            if (settings.TryGetValue(STCP_CERT_CONFIG, out var stcpCertificate)) {
                var stcpConfiguration = new StcpConfigurationElement()
                {
                    ServerCertificate = new SanteDB.Core.Security.Configuration.X509ConfigurationElement(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindByThumbprint, stcpCertificate)
                };
                dockerEp.TransportConfiguration = stcpConfiguration;
            }

            sgConfig.Endpoints.Add(dockerEp);

            var appServiceConfig = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            appServiceConfig.ServiceProviders.Add(new TypeReferenceConfiguration(typeof(SyslogMessageHandler)));

        }
    }
}
