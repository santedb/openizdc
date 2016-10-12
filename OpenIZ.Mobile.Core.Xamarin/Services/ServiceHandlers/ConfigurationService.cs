﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenIZ.Core.Http.Description;
using OpenIZ.Core.Model.Constants;
using OpenIZ.Core.Model.Query;
using OpenIZ.Mobile.Core.Configuration;
using OpenIZ.Mobile.Core.Data;
using OpenIZ.Mobile.Core.Diagnostics;
using OpenIZ.Mobile.Core.Exceptions;
using OpenIZ.Mobile.Core.Interop.IMSI;
using OpenIZ.Mobile.Core.Security;
using OpenIZ.Mobile.Core.Synchronization;
using OpenIZ.Mobile.Core.Xamarin.Diagnostics;
using OpenIZ.Mobile.Core.Xamarin.Http;
using OpenIZ.Mobile.Core.Xamarin.Security;
using OpenIZ.Mobile.Core.Xamarin.Services.Attributes;
using OpenIZ.Mobile.Core.Xamarin.Services.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Xamarin.Services.ServiceHandlers
{
    /// <summary>
    /// Configuration view model
    /// </summary>
    [JsonObject]
    public class ConfigurationViewModel
    {
        public ConfigurationViewModel()
        {

        }

        /// <summary>
        /// Configuation
        /// </summary>
        /// <param name="config"></param>
        public ConfigurationViewModel(OpenIZConfiguration config)
        {
            this.RealmName = config.GetSection<SecurityConfigurationSection>().Domain;
            this.Security = config.GetSection<SecurityConfigurationSection>();
            this.Data = config.GetSection<DataConfigurationSection>();
            this.Applet = config.GetSection<AppletConfigurationSection>();
            this.Application = config.GetSection<ApplicationConfigurationSection>();
            this.Log = config.GetSection<DiagnosticsConfigurationSection>();
            this.Network = config.GetSection<ServiceClientConfigurationSection>();
        }
        /// <summary>
        /// Security section
        /// </summary>
        [JsonProperty("security")]
        public SecurityConfigurationSection Security { get; set; }
        /// <summary>
        /// Realm name
        /// </summary>
        [JsonProperty("realmName")]
        public String RealmName { get; set; }
        /// <summary>
        /// Data config
        /// </summary>
        [JsonProperty("data")]
        public DataConfigurationSection Data { get; set; }
        /// <summary>
        /// Gets or sets applet
        /// </summary>
        [JsonProperty("applet")]
        public AppletConfigurationSection Applet { get; set; }
        /// <summary>
        /// Gets or sets application
        /// </summary>
        [JsonProperty("application")]
        public ApplicationConfigurationSection Application { get; set; }
        /// <summary>
        /// Log
        /// </summary>
        [JsonProperty("log")]
        public DiagnosticsConfigurationSection Log { get; set; }
        /// <summary>
        /// Gets or sets the network
        /// </summary>
        [JsonProperty("network")]
        public ServiceClientConfigurationSection Network { get; set; }

    }

    /// <summary>
    /// Restful service
    /// </summary>
    [RestService("/__config")]
    public class ConfigurationService
    {

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(ConfigurationService));

        /// <summary>
        /// Gets the specified forecast
        /// </summary>
        [RestOperation(UriPath = "/all", Method = "GET", FaultProvider = nameof(ConfigurationFaultProvider))]
        [return: RestMessage(RestMessageFormat.Json)]
        public ConfigurationViewModel GetConfiguration()
        {
            return new ConfigurationViewModel(XamarinApplicationContext.Current.Configuration);
        }

        /// <summary>
        /// Save configuration
        /// </summary>
        [RestOperation(UriPath = "/all", Method = "POST", FaultProvider = nameof(ConfigurationFaultProvider))]
        [return: RestMessage(RestMessageFormat.Json)]
        public ConfigurationViewModel SaveConfiguration([RestMessage(RestMessageFormat.Json)]JObject optionObject)
        {
            // Demand the appropriate policy
            new PolicyPermission(PermissionState.Unrestricted, PolicyIdentifiers.AccessClientAdministrativeFunction).Demand();
           

            // Data mode
            switch (optionObject["data"]["mode"].Value<String>())
            {
                case "online":
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.RemoveAll(o => o == typeof(LocalPolicyInformationService).AssemblyQualifiedName);
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(AmiPolicyInformationService).AssemblyQualifiedName);
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(OAuthIdentityProvider).AssemblyQualifiedName);
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(ImsiPersistenceService).AssemblyQualifiedName);
                    break;
                case "offline":
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(LocalPersistenceService).AssemblyQualifiedName);
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(LocalIdentityService).AssemblyQualifiedName);
                    break;
                case "sync":
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(LocalPersistenceService).AssemblyQualifiedName);
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(OAuthIdentityProvider).AssemblyQualifiedName);
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(QueueManagerService).AssemblyQualifiedName);
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(RemoteSynchronizationService).AssemblyQualifiedName);
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(ImsiIntegrationService).AssemblyQualifiedName);

                    // Sync settings
                    var syncConfig = new SynchronizationConfigurationSection();
                    // TODO: Customize this
                    foreach (var res in new String[] { "ConceptSet", "AssigningAuthority", "IdentifierType", "ConceptClass", "Concept", "Material", "Place", "Organization", "SecurityRole", "UserEntity", "Provider", "ManufacturedMaterial" })
                    {
                        var syncSetting = new SynchronizationResource()
                        {
                            ResourceAqn = res,
                            Triggers = SynchronizationPullTriggerType.Always
                        };

                        var efield = typeof(EntityClassKeys).GetField(res);
                        if (efield != null && res != "Place")
                            syncSetting.Filters.Add("classConcept=" + efield.GetValue(null).ToString());

                        syncConfig.SynchronizationResources.Add(syncSetting);
                    }
                    ApplicationContext.Current.Configuration.Sections.Add(syncConfig);
                    break;
            }
            ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(LocalRoleProviderService).AssemblyQualifiedName);
            // Password hashing
            switch (optionObject["security"]["hasher"].Value<String>())
            {
                case "SHA256PasswordHasher":
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(SHA256PasswordHasher).AssemblyQualifiedName);
                    break;
                case "SHAPasswordHasher":
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(SHAPasswordHasher).AssemblyQualifiedName);
                    break;
                case "PlainTextPasswordHasher":
                    ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(PlainTextPasswordHasher).AssemblyQualifiedName);
                    break;
            }

            // Proxy
            if (optionObject["network"]["useProxy"].Value<Boolean>())
                ApplicationContext.Current.Configuration.GetSection<ServiceClientConfigurationSection>().ProxyAddress = optionObject["network"]["proxyAddress"].Value<String>();

            // Log settings
            var logSettings = ApplicationContext.Current.Configuration.GetSection<DiagnosticsConfigurationSection>();
            logSettings.TraceWriter = new System.Collections.Generic.List<TraceWriterConfiguration>()
            {
#if DEBUG
                new TraceWriterConfiguration () {
                        Filter = System.Diagnostics.Tracing.EventLevel.LogAlways,
                        InitializationData = "OpenIZ",
                        TraceWriter = new LogTraceWriter (System.Diagnostics.Tracing.EventLevel.LogAlways, "OpenIZ")
                    },
#endif
                new TraceWriterConfiguration()
                {
                    Filter = (EventLevel)Enum.Parse(typeof(EventLevel), optionObject["log"]["mode"].Value<String>()),
                    InitializationData = "OpenIZ",
                    TraceWriter = new FileTraceWriter((EventLevel)Enum.Parse(typeof(EventLevel), optionObject["log"]["mode"].Value<String>()), "OpenIZ")

                }

            };

            this.m_tracer.TraceInfo("Saving configuration options {0}", optionObject);
            XamarinApplicationContext.Current.ConfigurationManager.Save();
            
            return new ConfigurationViewModel(XamarinApplicationContext.Current.Configuration) ;
        }

        /// <summary>
        /// Join a realm
        /// </summary>
        /// <param name="realmData"></param>
        [RestOperation(UriPath = "/realm", Method = "POST", FaultProvider = nameof(ConfigurationFaultProvider))]
        [return: RestMessage(RestMessageFormat.Json)]
        public ConfigurationViewModel JoinRealm([RestMessage(RestMessageFormat.FormData)]NameValueCollection realmData)
        {
            String realmUri = realmData["realmUri"][0];
            String deviceName = realmData["deviceName"][0];
            this.m_tracer.TraceInfo("Joining {0}", realmUri);

            // Demand policy
            try
            {
                if (XamarinApplicationContext.Current.ConfigurationManager.IsConfigured)
                    new PolicyPermission(PermissionState.Unrestricted, PolicyIdentifiers.AccessAdministrativeFunction).Demand();


                // Configure the stuff to the appropriate realm info
                var serviceClientSection = XamarinApplicationContext.Current.Configuration.GetSection<ServiceClientConfigurationSection>();
                if (serviceClientSection == null)
                {
                    serviceClientSection = new ServiceClientConfigurationSection()
                    {
                        RestClientType = typeof(RestClient)
                    };
                    XamarinApplicationContext.Current.Configuration.Sections.Add(serviceClientSection);
                }

                // TODO: Actually contact the AMI for this information


                ApplicationContext.Current.Configuration.GetSection<SecurityConfigurationSection>().DeviceName = deviceName;
                ApplicationContext.Current.Configuration.GetSection<SecurityConfigurationSection>().Domain = realmUri;
                ApplicationContext.Current.Configuration.GetSection<SecurityConfigurationSection>().TokenAlgorithms = new System.Collections.Generic.List<string>() {
                    "RS256",
                    "HS256"
                };
                ApplicationContext.Current.Configuration.GetSection<SecurityConfigurationSection>().TokenType = "urn:ietf:params:oauth:token-type:jwt";


                String imsiUri = String.Format("http://{0}:8080/imsi", realmUri),
                    oauthUri = String.Format("http://{0}:8080/auth", realmUri),
                    amiUri = String.Format("http://{0}:8080/ami", realmUri);

                // Parse IMSI URI
                serviceClientSection.Client.Add(new ServiceClientDescription()
                {
                    Binding = new ServiceClientBinding()
                    {
                        Security = new ServiceClientSecurity()
                        {
                            AuthRealm = realmUri,
                            Mode = SecurityScheme.Bearer,
                            CredentialProvider = new TokenCredentialProvider(),
                            PreemptiveAuthentication = true
                        },
                        Optimize = true
                    },
                    Endpoint = new System.Collections.Generic.List<ServiceClientEndpoint>() {
                        new ServiceClientEndpoint() {
                            Address = imsiUri
                        }
                    },
                    Name = "imsi"
                });

                // Parse ACS URI
                serviceClientSection.Client.Add(new ServiceClientDescription()
                {
                    Binding = new ServiceClientBinding()
                    {
                        Security = new ServiceClientSecurity()
                        {
                            AuthRealm = realmUri,
                            Mode = SecurityScheme.Bearer,
                            CredentialProvider = new TokenCredentialProvider(),
                            PreemptiveAuthentication = true
                        },
                        Optimize = true
                    },
                    Endpoint = new System.Collections.Generic.List<ServiceClientEndpoint>() {
                        new ServiceClientEndpoint() {
                            Address = amiUri
                        }
                    },
                    Name = "ami"
                });

                // Parse ACS URI
                serviceClientSection.Client.Add(new ServiceClientDescription()
                {
                    Binding = new ServiceClientBinding()
                    {
                        Security = new ServiceClientSecurity()
                        {
                            AuthRealm = realmUri,
                            Mode = SecurityScheme.Basic,
                            CredentialProvider = new OAuth2CredentialProvider()
                        },
                        Optimize = false
                    },
                    Endpoint = new System.Collections.Generic.List<ServiceClientEndpoint>() {
                        new ServiceClientEndpoint() {
                            Address = oauthUri
                        }
                    },
                    Name = "acs"
                });

                return new ConfigurationViewModel(XamarinApplicationContext.Current.Configuration);

            }
            catch (PolicyViolationException e)
            {
                // TODO: Login permission
                this.m_tracer.TraceError("Error joining realm: {0}", e);
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Error joining context: {0}", e);

            }
            return null;
        }

        /// <summary>
        /// Handle a fault
        /// </summary>
        public ErrorResult ConfigurationFaultProvider(Exception e)
        {
            return new ErrorResult()
            {
                Error = e.Message,
                ErrorDescription = e.InnerException?.Message
            };
        }
    }
}