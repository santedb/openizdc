﻿/*
 * Copyright 2015-2016 Mohawk College of Applied Arts and Technology
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
 * Date: 2016-7-30
 */
using OpenIZ.Core.Diagnostics;
using OpenIZ.Core.Http;
using OpenIZ.Core.Model;
using OpenIZ.Core.Model.Collection;
using OpenIZ.Core.Model.Interfaces;
using OpenIZ.Core.Model.Patch;
using OpenIZ.Core.Model.Query;
using OpenIZ.Messaging.IMSI.Client;
using OpenIZ.Mobile.Core.Configuration;
using OpenIZ.Mobile.Core.Exceptions;
using OpenIZ.Mobile.Core.Security;
using OpenIZ.Mobile.Core.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace OpenIZ.Mobile.Core.Interop.IMSI
{
    /// <summary>
    /// Represents an IMSI integration service which sends and retrieves data from the IMS.
    /// </summary>
    public class ImsiIntegrationService : IIntegrationService
    {

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(ImsiIntegrationService));

        /// <summary>
        /// Throws an exception if the specified service client has the invalid version
        /// </summary>
        private bool IsValidVersion(ImsiServiceClient client)
        {
            var expectedVersion = typeof(IdentifiedData).GetTypeInfo().Assembly.GetName().Version;
            var options = client.Options();
            if (options == null) return false;
            var version = new Version(options.InterfaceVersion);
            // Major version must match & minor version must match. Example:
            // Server           Client          Result
            // 0.6.14.*         0.6.14.*        Compatible
            // 0.7.0.*          0.6.14.*        Not compatible (server newer)
            // 0.7.0.*          0.9.0.0         Compatible (client newer)
            // 0.8.0.*          1.0.0.0         Not compatible (major version mis-match)
            this.m_tracer.TraceVerbose("IMSI server indicates version {0}", options.InterfaceVersion);
            return (expectedVersion.Major == version.Major &&
                expectedVersion.Minor >= version.Minor);
        }

        /// <summary>
        /// Gets current credentials
        /// </summary>
        private Credentials GetCredentials(IRestClient client)
        {
            var appConfig = ApplicationContext.Current.Configuration.GetSection<SecurityConfigurationSection>();

            // TODO: Clean this up - Login as device account
            if (!AuthenticationContext.Current.Principal.Identity.IsAuthenticated ||
                ((AuthenticationContext.Current.Principal as ClaimsPrincipal)?.FindClaim(ClaimTypes.Expiration)?.AsDateTime().ToLocalTime() ?? DateTime.MinValue) < DateTime.Now)
                AuthenticationContext.Current = new AuthenticationContext(ApplicationContext.Current.GetService<IIdentityProviderService>().Authenticate(appConfig.DeviceName, appConfig.DeviceSecret));
            return client.Description.Binding.Security.CredentialProvider.GetCredentials(AuthenticationContext.Current.Principal);
        }
        /// <summary>
        /// Gets the specified model object
        /// </summary>
        public Bundle Find(Type modelType, NameValueCollection filter, int offset, int? count, IntegrationQueryOptions options = null)
        {
            try
            {
                var method = this.GetType().GetRuntimeMethod("Find", new Type[] { typeof(NameValueCollection), typeof(int), typeof(int?), typeof(IntegrationQueryOptions) }).MakeGenericMethod(new Type[] { modelType });
                return method.Invoke(this, new object[] { filter, offset, count, options }) as Bundle;
            }
            catch (TargetInvocationException e)
            {
                throw Activator.CreateInstance(e.InnerException.GetType(), "Error performing action", e) as Exception;
            }

        }

        /// <summary>
        /// Finds the specified model
        /// </summary>
        public Bundle Find<TModel>(NameValueCollection filter, int offset, int? count, IntegrationQueryOptions options = null) where TModel : IdentifiedData
        {
            var predicate = QueryExpressionParser.BuildLinqExpression<TModel>(filter);
            return this.Find<TModel>(predicate, offset, count, options);
        }

        /// <summary>
        /// Finds the specified model
        /// </summary>
        public Bundle Find<TModel>(Expression<Func<TModel, bool>> predicate, int offset, int? count, IntegrationQueryOptions options = null) where TModel : IdentifiedData
        {
            try
            {
                ImsiServiceClient client = new ImsiServiceClient(ApplicationContext.Current.GetRestClient("imsi"));
                client.Client.Requesting += (o, e) =>
                {
                    if (options == null) return;
                    else if (options.IfModifiedSince.HasValue)
                        e.AdditionalHeaders[HttpRequestHeader.IfModifiedSince] = options.IfModifiedSince.Value.ToString();
                    else if (!String.IsNullOrEmpty(options.IfNoneMatch))
                        e.AdditionalHeaders[HttpRequestHeader.IfNoneMatch] = options.IfNoneMatch;
                };
                client.Client.Credentials = this.GetCredentials(client.Client);
                if (options.Timeout.HasValue)
                    client.Client.Description.Endpoint[0].Timeout = options.Timeout.Value;

                this.m_tracer.TraceVerbose("Performing IMSI query ({0}):{1}", typeof(TModel).FullName, predicate);

                var retVal = client.Query<TModel>(predicate, offset, count);
                //retVal?.Reconstitute();
                return retVal;
            }
            catch (TargetInvocationException e)
            {
                throw Activator.CreateInstance(e.InnerException.GetType(), "Error performing action", e) as Exception;
            }

        }

        /// <summary>
        /// Gets the specified model object
        /// </summary>
        public IdentifiedData Get(Type modelType, Guid key, Guid? version, IntegrationQueryOptions options = null)
        {
            try
            {
                var method = this.GetType().GetRuntimeMethod("Get", new Type[] { typeof(Guid), typeof(Guid?), typeof(IntegrationQueryOptions) }).MakeGenericMethod(new Type[] { modelType });
                return method.Invoke(this, new object[] { key, version, options }) as IdentifiedData;
            }
            catch (TargetInvocationException e)
            {
                throw Activator.CreateInstance(e.InnerException.GetType(), "Error performing action", e) as Exception;
            }

        }

        /// <summary>
        /// Gets a specified model.
        /// </summary>
        /// <typeparam name="TModel">The type of model data to retrieve.</typeparam>
        /// <param name="key">The key of the model.</param>
        /// <param name="versionKey">The version key of the model.</param>
        /// <param name="options">The integrations query options.</param>
        /// <returns>Returns a model.</returns>
        public TModel Get<TModel>(Guid key, Guid? versionKey, IntegrationQueryOptions options = null) where TModel : IdentifiedData
        {
            try
            {
                ImsiServiceClient client = new ImsiServiceClient(ApplicationContext.Current.GetRestClient("imsi"));
                client.Client.Requesting += (o, e) =>
                {
                    if (options == null) return;
                    else if (options.IfModifiedSince.HasValue)
                        e.AdditionalHeaders[HttpRequestHeader.IfModifiedSince] = options.IfModifiedSince.Value.ToString();
                    else if (!String.IsNullOrEmpty(options.IfNoneMatch))
                        e.AdditionalHeaders[HttpRequestHeader.IfNoneMatch] = options.IfNoneMatch;
                };
                client.Client.Credentials = this.GetCredentials(client.Client);
                this.m_tracer.TraceVerbose("Performing IMSI GET ({0}):{1}v{2}", typeof(TModel).FullName, key, versionKey);
                var retVal = client.Get<TModel>(key, versionKey);

                if (retVal is Bundle)
                {
                    (retVal as Bundle)?.Reconstitute();
                    return (retVal as Bundle).Entry as TModel;
                }
                else
                    return retVal as TModel;
            }
            catch (TargetInvocationException e)
            {
                throw Activator.CreateInstance(e.InnerException.GetType(), "Error performing action", e) as Exception;
            }

        }

        /// <summary>
        /// Inserts specified data.
        /// </summary>
        /// <param name="data">The data to be inserted.</param>
        public void Insert(IdentifiedData data)
        {
            try
            {
                ImsiServiceClient client = new ImsiServiceClient(ApplicationContext.Current.GetRestClient("imsi"));
                client.Client.Credentials = this.GetCredentials(client.Client);

                var method = typeof(ImsiServiceClient).GetRuntimeMethods().FirstOrDefault(o => o.Name == "Create" && o.GetParameters().Length == 1);
                method = method.MakeGenericMethod(new Type[] { data.GetType() });
                this.m_tracer.TraceVerbose("Performing IMSI INSERT {0}", data);
                var iver = method.Invoke(client, new object[] { data }) as IVersionedEntity;
                if (iver != null)
                    this.UpdateToServerCopy(iver);
            }
            catch (TargetInvocationException e)
            {
                throw Activator.CreateInstance(e.InnerException.GetType(), "Error performing action", e) as Exception;
            }
        }

        /// <summary>
        /// Determines whether the network is available.
        /// </summary>
        /// <returns>Returns true if the network is available.</returns>
        public bool IsAvailable()
        {
            try
            {
                var restClient = ApplicationContext.Current.GetRestClient("imsi");

                ImsiServiceClient client = new ImsiServiceClient(restClient);
                client.Client.Credentials = this.GetCredentials(client.Client);

                var networkInformationService = ApplicationContext.Current.GetService<INetworkInformationService>();

                return networkInformationService.IsNetworkAvailable &&
                    this.IsValidVersion(client);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Obsoletes specified data.
        /// </summary>
        /// <param name="data">The data to be obsoleted.</param>
        public void Obsolete(IdentifiedData data)
        {
            try
            {
                ImsiServiceClient client = new ImsiServiceClient(ApplicationContext.Current.GetRestClient("imsi"));
                client.Client.Credentials = this.GetCredentials(client.Client);

                var method = typeof(ImsiServiceClient).GetRuntimeMethods().FirstOrDefault(o => o.Name == "Obsolete" && o.GetParameters().Length == 1);
                method.MakeGenericMethod(new Type[] { data.GetType() });
                this.m_tracer.TraceVerbose("Performing IMSI OBSOLETE {0}", data);

                var iver = method.Invoke(this, new object[] { data }) as IVersionedEntity;
                if (iver != null)
                    this.UpdateToServerCopy(iver);
            }
            catch (TargetInvocationException e)
            {
                throw Activator.CreateInstance(e.InnerException.GetType(), "Error performing action", e) as Exception;
            }

        }

        /// <summary>
        /// Updates specified data.
        /// </summary>
        /// <param name="data">The data to be updated.</param>
        public void Update(IdentifiedData data)
        {
            try
            {
                ImsiServiceClient client = new ImsiServiceClient(ApplicationContext.Current.GetRestClient("imsi"));
                client.Client.Credentials = this.GetCredentials(client.Client);

                if (data is Patch)
                {
                    var patch = data as Patch;
                    this.m_tracer.TraceVerbose("Performing IMSI UPDATE (PATCH) {0}", data);
                    var newUuid = client.Patch(patch);
                    (patch.AppliesTo as IVersionedEntity).VersionKey = newUuid;
                    this.UpdateToServerCopy(patch.AppliesTo as IVersionedEntity);
                }
                else // regular update 
                {
                    var method = typeof(ImsiServiceClient).GetRuntimeMethods().FirstOrDefault(o => o.Name == "Update" && o.GetParameters().Length == 1);
                    method.MakeGenericMethod(new Type[] { data.GetType() });
                    this.m_tracer.TraceVerbose("Performing IMSI UPDATE (FULL) {0}", data);

                    var iver = method.Invoke(this, new object[] { data }) as IVersionedEntity;
                    if (iver != null)
                        this.UpdateToServerCopy(iver);
                }
            }
            catch (TargetInvocationException e)
            {
                throw Activator.CreateInstance(e.InnerException.GetType(), "Error performing action", e) as Exception;
            }

        }

        /// <summary>
        /// Update the version identifier to the server identifier
        /// </summary>
        public void UpdateToServerCopy(IVersionedEntity newData)
        {
            this.m_tracer.TraceVerbose("Updating to remote version {0}", newData);
            // Update the ETag of the current version
            var idp = typeof(IDataPersistenceService<>).MakeGenericType(newData.GetType());
            var idpService = ApplicationContext.Current.GetService(idp);
            if (idpService != null)
                idp.GetRuntimeMethod("Update", new Type[] { newData.GetType() }).Invoke(idpService, new object[] { newData });
        }
    }
}