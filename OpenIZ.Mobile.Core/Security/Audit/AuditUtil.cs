﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * 
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: justi
 * Date: 2018-7-7
 */
using MARC.HI.EHRS.SVC.Auditing.Data;
using MARC.HI.EHRS.SVC.Auditing.Services;
using OpenIZ.Core.Model;
using OpenIZ.Core.Model.Acts;
using OpenIZ.Core.Model.Attributes;
using OpenIZ.Core.Model.Entities;
using OpenIZ.Core.Model.Interfaces;
using OpenIZ.Core.Model.Roles;
using OpenIZ.Core.Model.Security;
using OpenIZ.Mobile.Core.Configuration;
using OpenIZ.Mobile.Core.Data.Model;
using OpenIZ.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OpenIZ.Mobile.Core.Security.Audit
{
    /// <summary>
    /// Event type codes
    /// </summary>
    public enum EventTypeCodes
    {
        [XmlEnum("SecurityAuditCode-ApplicationActivity")]
        ApplicationActtivity,
        [XmlEnum("SecurityAuditCode-AuditLogUsed")]
        AuditLogUsed,
        [XmlEnum("SecurityAuditCode-Export")]
        Export,
        [XmlEnum("SecurityAuditCode-Import")]
        Import,
        [XmlEnum("SecurityAuditCode-NetworkActivity")]
        NetworkActivity,
        [XmlEnum("SecurityAuditCode-OrderRecord")]
        OrderRecord,
        [XmlEnum("SecurityAuditCode-PatientRecord")]
        PatientRecord,
        [XmlEnum("SecurityAuditCode-ProcedureRecord")]
        ProcedureRecord,
        [XmlEnum("SecurityAuditCode-Query")]
        Query,
        [XmlEnum("SecurityAuditCode-SecurityAlert")]
        SecurityAlert,
        [XmlEnum("SecurityAuditCode-UserAuthentication")]
        UserAuthentication,
        [XmlEnum("SecurityAuditCode-ApplicationStart")]
        ApplicationStart,
        [XmlEnum("SecurityAuditCode-ApplicationStop")]
        ApplicationStop,
        [XmlEnum("SecurityAuditCode-Login")]
        Login,
        [XmlEnum("SecurityAuditCode-Logout")]
        Logout,
        [XmlEnum("SecurityAuditCode-Attach")]
        Attach,
        [XmlEnum("SecurityAuditCode-Detach")]
        Detach,
        [XmlEnum("SecurityAuditCode-NodeAuthentication")]
        NodeAuthentication,
        [XmlEnum("SecurityAuditCode-EmergencyOverrideStarted")]
        EmergencyOverrideStarted,
        [XmlEnum("SecurityAuditCode-Useofarestrictedfunction")]
        UseOfARestrictedFunction,
        [XmlEnum("SecurityAuditCode-Securityattributeschanged")]
        SecurityAttributesChanged,
        [XmlEnum("SecurityAuditCode-Securityroleschanged")]
        SecurityRolesChanged,
        [XmlEnum("SecurityAuditCode-Usersecurityattributechanged")]
        UserSecurityChanged
    }

    /// <summary>
    /// Security utility
    /// </summary>
    public static class AuditUtil
    {

        /// <summary>
        /// Audit that the audit log was used
        /// </summary>
        /// <param name="action"></param>
        /// <param name="outcome"></param>
        /// <param name="query"></param>
        /// <param name="auditIds"></param>
        public static void AuditAuditLogUsed(ActionType action, OutcomeIndicator outcome, String query, params Guid[] auditIds)
        {
            AuditData audit = new AuditData(DateTime.Now, action, outcome, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.AuditLogUsed));

            // User actors
            //AddDeviceActor(audit);
            AddUserActor(audit);

            // Add objects to which the thing was done
            audit.AuditableObjects = auditIds.Select(o => new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.ReportNumber,
                LifecycleType = action == ActionType.Delete ? AuditableObjectLifecycle.PermanentErasure : AuditableObjectLifecycle.Disclosure,
                ObjectId = o.ToString(),
                Role = AuditableObjectRole.SecurityResource,
                Type = AuditableObjectType.SystemObject
            }).ToList();

            if (!String.IsNullOrEmpty(query))
            {
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.SearchCritereon,
                    LifecycleType = AuditableObjectLifecycle.Access,
                    QueryData = query,
                    Role = AuditableObjectRole.Query,
                    Type = AuditableObjectType.SystemObject
                });
            }
            AddAncillaryObject(audit);

            ApplicationContext.Current.GetService<LocalAuditRepositoryService>().Insert(audit);

        }

        /// <summary>
        /// Adds ancillary object information to the audit log
        /// </summary>
        private static void AddAncillaryObject(AuditData audit)
        {
            // Add audit actors for this device and for the current user
            var securityConfig = ApplicationContext.Current.Configuration.GetSection<SecurityConfigurationSection>();
            var subscriptionConfig = ApplicationContext.Current.Configuration.GetSection<SynchronizationConfigurationSection>();

            // Add auditable object which identifies the device
            audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Custom,
                CustomIdTypeCode = new AuditCode("Device","OpenIZTable"),
                ObjectId = securityConfig.DeviceName,
                Role = AuditableObjectRole.DataRepository,
                Type = AuditableObjectType.SystemObject,
                ObjectData = new List<ObjectDataExtension>()
                {
                    new ObjectDataExtension("versionCode", Encoding.UTF8.GetBytes(ApplicationContext.Current.GetType().GetTypeInfo().Assembly.GetName().Version.ToString()))
                }
            });

            // Add auditable object which identifies the facility
            var facilityId = subscriptionConfig.Facilities?.FirstOrDefault();
            if(!String.IsNullOrEmpty(facilityId))
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode("Place", "OpenIZTable"),
                    ObjectId = facilityId,
                    Role = AuditableObjectRole.Location,
                    Type = AuditableObjectType.Organization
                });

        }

        /// <summary>
        /// Audit the export of data 
        /// </summary>
        public static void AuditDataExport()
        {
            AuditCode eventTypeId = CreateAuditActionCode(EventTypeCodes.Export);
            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.SecurityAlert, eventTypeId);

            AddDeviceActor(audit);
            AddUserActor(audit);
            AddAncillaryObject(audit);

            SendAudit(audit);
        }

        /// <summary>
        /// A utility which can be used to send a data audit 
        /// </summary>
        public static void AuditDataAction<TData>(EventTypeCodes typeCode, ActionType action, AuditableObjectLifecycle lifecycle, EventIdentifierType eventType, OutcomeIndicator outcome, String queryPerformed, params TData[] data) where TData : IdentifiedData
        {
            AuditCode eventTypeId = CreateAuditActionCode(typeCode);
            AuditData audit = new AuditData(DateTime.Now, action, outcome, eventType, eventTypeId);

            AddDeviceActor(audit);
            AddUserActor(audit);

            // Objects
            audit.AuditableObjects = data.Select(o => {

                var idTypeCode = AuditableObjectIdType.Custom;
                var roleCode = AuditableObjectRole.Resource;
                var objType = AuditableObjectType.Other;

                if (o is Patient)
                {
                    idTypeCode = AuditableObjectIdType.PatientNumber;
                    roleCode = AuditableObjectRole.Patient;
                    objType = AuditableObjectType.Person;
                }
                else if (o is UserEntity || o is Provider)
                {
                    idTypeCode = AuditableObjectIdType.UserIdentifier;
                    objType = AuditableObjectType.Person;
                    roleCode = AuditableObjectRole.Provider;
                }
                else if (o is Entity) 
                    idTypeCode = AuditableObjectIdType.EnrolleeNumber;
                else if (o is Act)
                {
                    idTypeCode = AuditableObjectIdType.EncounterNumber;
                    roleCode = AuditableObjectRole.Report;
                }
                else if (o is SecurityUser)
                {
                    idTypeCode = AuditableObjectIdType.UserIdentifier;
                    roleCode = AuditableObjectRole.SecurityUser;
                    objType = AuditableObjectType.SystemObject;
                }

                return new AuditableObject()
                {
                    IDTypeCode = idTypeCode,
                    CustomIdTypeCode = idTypeCode == AuditableObjectIdType.Custom ? new AuditCode(o.GetType().Name, "OpenIZTable") : null,
                    LifecycleType = lifecycle,
                    ObjectId = o.Key?.ToString(),
                    Role = roleCode,
                    Type = objType
                };
            }).ToList();

            // Query performed
            if (!String.IsNullOrEmpty(queryPerformed))
            {
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.SearchCritereon,
                    LifecycleType = AuditableObjectLifecycle.Access,
					ObjectId = typeof(TData).Name,
                    QueryData = queryPerformed,
                    Role = AuditableObjectRole.Query,
                    Type = AuditableObjectType.SystemObject
                });
            }
            AddAncillaryObject(audit);

            SendAudit(audit);
        }

        /// <summary>
        /// Create a security attribute action audit
        /// </summary>
        public static void AuditSecurityAttributeAction(IEnumerable<Object> objects, bool success, IEnumerable<string> changedProperties)
        {
            var audit = new AuditData(DateTime.Now, ActionType.Update, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.SecurityAttributesChanged));
            //AddDeviceActor(audit);
            AddUserActor(audit);
            
            audit.AuditableObjects = objects.Select(obj => new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Custom,
                CustomIdTypeCode = new AuditCode(obj.GetType().Name, "OpenIZTable"),
                ObjectId = ((obj as DbIdentified)?.Key ?? (obj as IIdentifiedEntity)?.Key ?? Guid.Empty).ToString(),
                LifecycleType = AuditableObjectLifecycle.Amendment,
                ObjectData = changedProperties.Select(
                    kv => new ObjectDataExtension(
                        kv.Contains("=") ? kv.Substring(0, kv.IndexOf("=")) : kv, 
                        kv.Contains("=") ? Encoding.UTF8.GetBytes(kv.Substring(kv.IndexOf("=") + 1)) : new byte[0]
                    )
                ).ToList(),
                Role = AuditableObjectRole.SecurityResource,
                Type = AuditableObjectType.SystemObject
            }).ToList();
            AddAncillaryObject(audit);

            SendAudit(audit);
        }

        /// <summary>
        /// Send specified audit
        /// </summary>
        internal static void SendAudit(AuditData audit)
        {
            ApplicationContext.Current.GetService<IAuditorService>()?.SendAudit(audit);
        }

        /// <summary>
        /// Add user actor
        /// </summary>
        internal static void AddUserActor(AuditData audit)
        {
            var configService = ApplicationContext.Current.Configuration.GetSection<SecurityConfigurationSection>();

            // For the user
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = configService.DeviceName,
                NetworkAccessPointType = NetworkAccessPointType.MachineName,
                UserName = AuthenticationContext.Current.Principal.Identity.Name,
                AlternativeUserId = AuthenticationContext.Current.Session?.Key?.ToString(),
                UserIdentifier = AuthenticationContext.Current.Session?.SecurityUser?.Key?.ToString(),
                UserIsRequestor = true
            });
        }

        /// <summary>
        /// Add device actor
        /// </summary>
        internal static void AddDeviceActor(AuditData audit)
        {
            // Add audit actors for this device and for the current user
            var networkService = ApplicationContext.Current.GetService<INetworkInformationService>();
            var configService = ApplicationContext.Current.Configuration.GetSection<SecurityConfigurationSection>();

            // For the current device name
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = configService.DeviceName,
                NetworkAccessPointType = NetworkAccessPointType.MachineName,
                UserName = configService.DeviceName,
                ActorRoleCode = new List<AuditCode>() {
                    new  AuditCode("110153", "DCM") { DisplayName = "Source" }
                }
            });

            
        }

        /// <summary>
        /// Create audit action code
        /// </summary>
        internal static AuditCode CreateAuditActionCode(EventTypeCodes typeCode)
        {
            var typeCodeWire = typeof(EventTypeCodes).GetRuntimeField(typeCode.ToString()).GetCustomAttribute<XmlEnumAttribute>();
            return new AuditCode(typeCodeWire.Name, "SecurityAuditCode");
        }

        /// <summary>
        /// Audit application start or stop
        /// </summary>
        public static void AuditApplicationStartStop(EventTypeCodes eventType)
        {
            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.ApplicationActivity, CreateAuditActionCode(eventType));
            AddDeviceActor(audit);
            AddAncillaryObject(audit);

            SendAudit(audit);
        }

        /// <summary>
        /// Audit a login of a principal
        /// </summary>
        public static void AuditLogin(IPrincipal principal, String identityName, IIdentityProviderService identityProvider, bool successfulLogin =true)
        {
            if ((principal?.Identity?.Name ?? identityName) == ApplicationContext.Current.Configuration.GetSection<SecurityConfigurationSection>().DeviceName) return; // don't worry about this
            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, successfulLogin ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.UserAuthentication, CreateAuditActionCode(EventTypeCodes.Login));
            var configService = ApplicationContext.Current.Configuration.GetSection<SecurityConfigurationSection>();
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointType = NetworkAccessPointType.MachineName,
                NetworkAccessPointId = configService.DeviceName,
                UserName = principal?.Identity?.Name ?? identityName,
                UserIsRequestor = true,
                ActorRoleCode = (principal as ClaimsPrincipal)?.Claims.Where(o=>o.Type == ClaimsIdentity.DefaultRoleClaimType).Select(o=>new AuditCode(o.Value, "OizRoles")).ToList()
            });

            AddDeviceActor(audit);

            audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                NameData = identityProvider.GetType().AssemblyQualifiedName,
                ObjectId = $"http://openiz.org/mobile/auth/{identityProvider.GetType().FullName.Replace(".", "/")}",
                Type = AuditableObjectType.SystemObject,
                Role = AuditableObjectRole.Job
            });

            AddAncillaryObject(audit);

            SendAudit(audit);
        }

        /// <summary>
        /// Audit a login of a principal
        /// </summary>
        public static void AuditLogout(IPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.UserAuthentication, CreateAuditActionCode(EventTypeCodes.Logout));
            var configService = ApplicationContext.Current.Configuration.GetSection<SecurityConfigurationSection>();
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = configService.DeviceName,
                NetworkAccessPointType = NetworkAccessPointType.MachineName,
                UserName = principal.Identity.Name,
                UserIsRequestor = true
            });
            AddDeviceActor(audit);
            AddAncillaryObject(audit);

            SendAudit(audit);
        }

        /// <summary>
        /// Audit the use of a restricted function
        /// </summary>
        public static void AuditRestrictedFunction(UnauthorizedAccessException ex, Uri url)
        {
            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.UseOfARestrictedFunction));
            AddUserActor(audit);
            AddDeviceActor(audit);
            audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                LifecycleType = AuditableObjectLifecycle.Access,
                ObjectId = url.ToString(),
                Role = AuditableObjectRole.Resource,
                Type = AuditableObjectType.SystemObject
            });
            AddAncillaryObject(audit);

            SendAudit(audit);
        }
    }
}
