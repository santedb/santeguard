using SanteDB.Client.Configuration;
using SanteDB.Core.Configuration;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SanteGuard.Messaging.Syslog;
using SanteGuard.Messaging.Syslog.Action;
using SanteGuard.Messaging.Syslog.Configuration;
using SanteGuard.Messaging.Syslog.TransportProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SanteGuard.Client
{
    /// <summary>
    /// Allows the dCDR to configure a SYSLOG endpoint
    /// </summary>
    public class SyslogClientRestFeature : IClientConfigurationFeature
    {
        /// <summary>
        /// Enabled feature
        /// </summary>
        private const string ENABLED_SETTING = "enabled";
        /// <summary>
        /// Binding for the configuration
        /// </summary>
        private const string BINDING_SETTING = "binding";
        /// <summary>
        /// Default enterprise configuration
        /// </summary>
        private const string ENTERPRISE_SETTING = "enterpriseId";
        /// <summary>
        /// For forwarding SYSLOG messages
        /// </summary>
        private const string FORWARD_SETTING = "forwardTo";
        /// <summary>
        /// Maximum size
        /// </summary>
        private const string MAX_SIZE_SETTING = "maxSize";
        /// <summary>
        /// STCP certificate configuration
        /// </summary>
        private const string STCP_CERT_SETTING = "certificate";

        // Configuration reference
        private SanteGuardConfiguration m_configuration;

        /// <summary>
        /// DI constructor
        /// </summary>
        public SyslogClientRestFeature(IConfigurationManager configurationManager)
        {
            this.m_configuration = configurationManager.GetSection<SanteGuardConfiguration>();
        }

        /// <inheritdoc/>
        public int Order => Int32.MaxValue;

        /// <inheritdoc/>
        public string Name => "syslog";

        /// <inheritdoc/>
        public ConfigurationDictionary<string, object> Configuration => this.GetConfiguration();

        /// <summary>
        /// Get configuration data
        /// </summary>
        private ConfigurationDictionary<string, object> GetConfiguration()
        {
            var retVal = new ConfigurationDictionary<string, object>();
            var endpoint = this.m_configuration?.Endpoints.FirstOrDefault();
            retVal.Add(ENABLED_SETTING, this.m_configuration != null);
            retVal.Add(ENTERPRISE_SETTING, this.m_configuration?.DefaultEnterpriseSiteID);
            retVal.Add(BINDING_SETTING, endpoint?.AddressXml);
            retVal.Add(FORWARD_SETTING, endpoint?.Forward?.FirstOrDefault());
            retVal.Add(MAX_SIZE_SETTING, endpoint?.MaxSize);
            retVal.Add(STCP_CERT_SETTING, (endpoint?.TransportConfiguration as StcpConfigurationElement)?.ServerCertificate?.FindType);
            return retVal;
        }

        /// <inheritdoc/>
        public string ReadPolicy => PermissionPolicyIdentifiers.AccessClientAdministrativeFunction;

        /// <inheritdoc/>
        public string WritePolicy => PermissionPolicyIdentifiers.AccessClientAdministrativeFunction;

        /// <inheritdoc/>
        public bool Configure(SanteDBConfiguration configuration, IDictionary<string, object> featureConfiguration)
        {
            if(!featureConfiguration.TryGetValue(ENABLED_SETTING, out var enabled) && !(bool)enabled)
            {
                return true;
            }

            // Add the configuration section
            var section = configuration.GetSection<SanteGuardConfiguration>();
            if(section == null)
            {
                section = new SanteGuardConfiguration();
                configuration.AddSection(section);
            }

            if(featureConfiguration.TryGetValue(ENTERPRISE_SETTING, out var enterpriseSetting))
            {
                section.DefaultEnterpriseSiteID = enterpriseSetting.ToString();
            }

            var endpointConfiguration = section.Endpoints.FirstOrDefault();
            if(endpointConfiguration == null)
            {
                endpointConfiguration = new EndpointConfiguration();
                section.Endpoints.Add(endpointConfiguration);
            }

            if(!featureConfiguration.TryGetValue(BINDING_SETTING, out var bindingRaw) || !Uri.TryCreate(bindingRaw.ToString(), UriKind.Absolute, out var bindingUri))
            {
                throw new ArgumentOutOfRangeException(BINDING_SETTING);
            }
            else if(TransportUtil.Current.CreateTransport(bindingUri.Scheme) == null)
            {
                throw new ArgumentOutOfRangeException($"{bindingUri.Scheme} is not valid");
            }

            endpointConfiguration.Address = bindingUri;
            endpointConfiguration.Action.Clear();
            endpointConfiguration.Action.Add(new TypeReferenceConfiguration(typeof(LogAction)));
            endpointConfiguration.Action.Add(new TypeReferenceConfiguration(typeof(StorageAction)));

            int maxSize = 8096;
            if(featureConfiguration.TryGetValue(MAX_SIZE_SETTING, out var maxSizeRaw) && !int.TryParse(maxSizeRaw.ToString(), out maxSize))
            {
                throw new ArgumentOutOfRangeException(MAX_SIZE_SETTING);
            }
            endpointConfiguration.MaxSize = maxSize;

            if(featureConfiguration.TryGetValue(FORWARD_SETTING, out var forwardRaw))
            {
                endpointConfiguration.Action.Add(new TypeReferenceConfiguration(typeof(ForwardAction)));
                endpointConfiguration.Forward.Add(forwardRaw.ToString());
            }

            // is the scheme stcp? 
            if(endpointConfiguration.Address.Scheme.Equals("stcp", StringComparison.OrdinalIgnoreCase))
            {
                if (!featureConfiguration.TryGetValue(STCP_CERT_SETTING, out var certRaw))
                {
                    throw new InvalidOperationException("STCP requires certificate");
                }
                endpointConfiguration.TransportConfiguration = new StcpConfigurationElement()
                {
                    ServerCertificate = new SanteDB.Core.Security.Configuration.X509ConfigurationElement(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindByThumbprint, certRaw.ToString())
                };
            }

            // Add the service
            var appServiceConfig = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            appServiceConfig.ServiceProviders.RemoveAll(o => o.Type == typeof(SyslogMessageHandler));
            appServiceConfig.ServiceProviders.Add(new TypeReferenceConfiguration(typeof(SyslogMessageHandler)));
            this.m_configuration = section;
            return true;
        }
    }
}
