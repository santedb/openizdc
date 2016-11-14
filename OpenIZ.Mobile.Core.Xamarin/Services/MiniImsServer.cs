/*
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
 * Date: 2016-11-12
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenIZ.Mobile.Core.Services;
using System.Net;
using OpenIZ.Mobile.Core.Xamarin.Threading;
using System.Threading;
using System.Net.Sockets;
using System.Globalization;
using OpenIZ.Mobile.Core.Configuration;
using System.IO;
using OpenIZ.Mobile.Core.Xamarin.Security;
using System.Security;
using OpenIZ.Core.Applets.Model;
using OpenIZ.Mobile.Core.Diagnostics;
using System.Reflection;
using OpenIZ.Mobile.Core.Xamarin.Services.Attributes;
using OpenIZ.Core.Http;
using OpenIZ.Mobile.Core.Xamarin.Resources;
using OpenIZ.Core.Applets.ViewModel;
using OpenIZ.Core.Model;
using OpenIZ.Core.Services;
using OpenIZ.Core.Applets.ViewModel.Description;
using System.Diagnostics;
using OpenIZ.Mobile.Core.Exceptions;
using OpenIZ.Core.Applets.ViewModel.Json;

namespace OpenIZ.Mobile.Core.Xamarin.Services
{
    /// <summary>
    /// Represents a mini IMS server that the web-view can access
    /// </summary>
    public class MiniImsServer : IDaemonService
    {

        // Default view model
        private ViewModelDescription m_defaultViewModel;

        // JSON View model serializer
        private JsonViewModelSerializer m_serializer;

        // Current context
        [ThreadStatic]
        public static HttpListenerContext CurrentContext;

        // Mini-listener
        private HttpListener m_listener;
        private Thread m_acceptThread;
        private Tracer m_tracer = Tracer.GetTracer(typeof(MiniImsServer));
        private Dictionary<String, AppletAsset> m_cacheApplets = new Dictionary<string, AppletAsset>();
        private object m_lockObject = new object();
        private Dictionary<String, InvokationInformation> m_services = new Dictionary<string, InvokationInformation>();
        private IContentTypeMapper m_contentTypeHandler = new DefaultContentTypeMapper();
        private IThreadPoolService m_threadPool = null;

        /// <summary>
        /// Returns true if the service is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this.m_listener?.IsListening == true;
            }
        }

        public event EventHandler Started;
        public event EventHandler Starting;
        public event EventHandler Stopped;
        public event EventHandler Stopping;

        public bool Start()
        {
            try
            {
                this.Starting?.Invoke(this, EventArgs.Empty);

                this.m_tracer.TraceInfo("Starting internal IMS services...");
                this.m_threadPool = ApplicationContext.Current.GetService<IThreadPoolService>();

                XamarinApplicationContext.Current.SetProgress("IMS Service Bus", 0);
                this.m_listener = new HttpListener();
                this.m_defaultViewModel = ViewModelDescription.Load(typeof(MiniImsServer).Assembly.GetManifestResourceStream("OpenIZ.Mobile.Core.Xamarin.Resources.ViewModel.xml"));
                this.m_serializer = new JsonViewModelSerializer();
                this.m_serializer.ViewModel = this.m_defaultViewModel;

                // Scan for services
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    try
                    {
                        foreach (var t in a.DefinedTypes.Where(o => o.GetCustomAttribute<RestServiceAttribute>() != null))
                        {
                            var serviceAtt = t.GetCustomAttribute<RestServiceAttribute>();
                            object instance = Activator.CreateInstance(t);
                            foreach (var mi in t.GetRuntimeMethods().Where(o => o.GetCustomAttribute<RestOperationAttribute>() != null))
                            {
                                var operationAtt = mi.GetCustomAttribute<RestOperationAttribute>();
                                var faultMethod = operationAtt.FaultProvider != null ? t.GetRuntimeMethod(operationAtt.FaultProvider, new Type[] { typeof(Exception) }) : null;
                                String pathMatch = String.Format("{0}:{1}{2}", operationAtt.Method, serviceAtt.BaseAddress, operationAtt.UriPath);
                                if (!this.m_services.ContainsKey(pathMatch))
                                    lock (this.m_lockObject)
                                        this.m_services.Add(pathMatch, new InvokationInformation()
                                        {
                                            BindObject = instance,
                                            Method = mi,
                                            FaultProvider = faultMethod,
                                            Demand = (mi.GetCustomAttributes<DemandAttribute>().Union(t.GetCustomAttributes<DemandAttribute>())).Select(o=>o.PolicyId).ToList(),
                                            Anonymous = (mi.GetCustomAttribute<AnonymousAttribute>() ?? t.GetCustomAttribute<AnonymousAttribute>()) != null
                                        });

                            }

                        }
                    }
                    catch(Exception e)
                    {
                        this.m_tracer.TraceWarning("Could not load assembly {0} : {1}", a, e);
                    }

                // Get loopback
                var loopback = GetLocalIpAddress();

                // Core always on 9200
                this.m_listener.Prefixes.Add(String.Format("http://{0}:9200/", loopback));

                this.m_acceptThread = new Thread(() =>
                {
                    // Process the request
                    while (this.m_listener != null)
                    {
                        try
                        {
                            //var iAsyncResult = this.m_listener.BeginGetContext(null, null);
                            //iAsyncResult.AsyncWaitHandle.WaitOne();
                            var context = this.m_listener.GetContext(); //this.m_listener.EndGetContext(iAsyncResult);
                            //new Thread(this.HandleRequest).Start(context);
                            this.m_threadPool.QueueUserWorkItem(this.HandleRequest, context);
                        }
                        catch (Exception e)
                        {
                            this.m_tracer.TraceError("Listener Error: {0}", e);
                        }
                    }
                });

                this.m_listener.Start();
                this.m_acceptThread.IsBackground = true;
                this.m_acceptThread.Start();
                this.m_acceptThread.Name = "MiniIMS";
                this.m_tracer.TraceInfo("Started internal IMS services...");
                this.Started?.Invoke(this, EventArgs.Empty);

                return true;
            }
            catch (Exception ex)
            {
                this.m_tracer.TraceError("Error starting IMS : {0}", ex);
                return false;
            }
        }

        /// <summary>
        /// Don't know why but the device doesn't have a loopback interface by default?
        /// </summary>
        /// <returns></returns>
        private static string GetLocalIpAddress()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            try
            {
                listener.Start();
                string address = ((IPEndPoint)listener.LocalEndpoint).Address.ToString();
                return address;
            }
            finally
            {
                listener.Stop();
            }
        }

        /// <summary>
        /// Handles a request
        /// </summary>
        private void HandleRequest(Object state)
        {

            HttpListenerContext context = state as HttpListenerContext;
            var request = context.Request;
            var response = context.Response;

#if DEBUG
            Stopwatch perfTimer = new Stopwatch();
            perfTimer.Start();
#endif

            try
            {
                MiniImsServer.CurrentContext = context;

                // Session cookie?
                if (request.Cookies["_s"] != null)
                {
                    var cookie = request.Cookies["_s"];
                    if (!cookie.Expired)
                    {
                        var smgr = ApplicationContext.Current.GetService<ISessionManagerService>();
                        var session = smgr.Get(Guid.Parse(cookie.Value));
                        if (session != null)
                                AuthenticationContext.Current = new AuthenticationContext(session);
                    }
                }

                // Authorization header
                if (request.Headers["Authorization"] != null)
                {
                    var authHeader = request.Headers["Authorization"].Split(' ');
                    switch (authHeader[0].ToLowerInvariant()) // Type / scheme
                    {
                        case "basic":
                            {
                                var idp = ApplicationContext.Current.GetService<IIdentityProviderService>();
                                var authString = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader[1])).Split(':');
                                var principal = idp.Authenticate(authString[0], authString[1]);
                                if (principal == null)
                                    throw new UnauthorizedAccessException();
                                else
                                    AuthenticationContext.Current = new AuthenticationContext(principal);
                                break;
                            }
                    }
                } 

                // Attempt to find a service which implements the path
                var rootPath = String.Format("{0}:{1}", request.HttpMethod.ToUpper(), request.Url.AbsolutePath);
                InvokationInformation invoke = null;

                if (this.m_services.TryGetValue(rootPath, out invoke))
                {

                    // Services require magic
                    if (!invoke.Anonymous && (request.Headers["X-OIZMagic"] == null ||
                        request.Headers["X-OIZMagic"] != ApplicationContext.Current.ExecutionUuid.ToString()))
                        throw new UnauthorizedAccessException();

                    this.m_tracer.TraceVerbose("Matched path {0} to handler {1}.{2}", rootPath, invoke.BindObject.GetType().FullName, invoke.Method.Name);


                    // Get the method information 
                    var parmInfo = invoke.Method.GetParameters();
                    object result = null;

                    try
                    {
                        // Method demand?
                        foreach (var itm in invoke.Demand)
                            new PolicyPermission(System.Security.Permissions.PermissionState.Unrestricted, itm).Demand();

                        // Invoke method
                        if (parmInfo.Length == 0)
                            result = invoke.Method.Invoke(invoke.BindObject, new object[] { });
                        else
                        {
                            if (parmInfo[0].GetCustomAttribute<RestMessageAttribute>()?.MessageFormat == RestMessageFormat.SimpleJson)
                            {
                                using (StreamReader sr = new StreamReader(request.InputStream))
                                {
                                    var pValue = this.m_serializer.DeSerialize(sr, parmInfo[0].ParameterType);
                                    result = invoke.Method.Invoke(invoke.BindObject, new object[] { pValue });
                                }
                            }
                            else
                            {
                                var serializer = this.m_contentTypeHandler.GetSerializer(request.ContentType, parmInfo[0].ParameterType);
                                var pValue = serializer.DeSerialize(request.InputStream);
                                result = invoke.Method.Invoke(invoke.BindObject, new object[] { pValue });
                            }
                        }
                        response.StatusCode = 200;
                    }
                    catch(Exception e)
                    {
                        result = this.HandleServiceException(e, invoke, response);
                        if (result == null)
                            throw;
                    }

                    // Serialize the response
                    if (request.Headers["Accept"] != null && invoke.Method.ReturnParameter.GetCustomAttribute<RestMessageAttribute>()?.MessageFormat != RestMessageFormat.Raw &&
                        invoke.Method.ReturnParameter.GetCustomAttribute<RestMessageAttribute>()?.MessageFormat != RestMessageFormat.SimpleJson)
                    {
                        var serializer = this.m_contentTypeHandler.GetSerializer(request.Headers["Accept"].Split(',')[0], result?.GetType() ?? typeof(IdentifiedData));
                        if (serializer != null)
                        {
                            response.ContentType = request.Headers["Accept"].Split(',')[0];
                            serializer.Serialize(response.OutputStream, result);
                        }
                        else
                            throw new ArgumentOutOfRangeException(Strings.err_invalid_accept);
                    }
                    else // Use the contract values
                        switch (invoke.Method.ReturnParameter.GetCustomAttribute<RestMessageAttribute>().MessageFormat)
                        {
                            case RestMessageFormat.Raw:
                                if (result is Stream)
                                    (result as Stream).CopyTo(response.OutputStream);
                                else
                                {
                                    var br = result as Byte[];
                                    response.OutputStream.Write(br, 0, br.Length);
                                }
                                break;
                            case RestMessageFormat.SimpleJson:
                                response.ContentType = "application/json";
                                if (result is IdentifiedData)
                                {

                                    using (StreamWriter sw = new StreamWriter(response.OutputStream))
                                    {
#if DEBUG
                                        Stopwatch serializerWatch = new Stopwatch();
                                        serializerWatch.Start();
#endif
                                        this.m_serializer.Serialize(sw, result as IdentifiedData);
#if DEBUG
                                        serializerWatch.Stop();
                                        this.m_tracer.TraceVerbose("PERF: Serializer >>>> {0} took {1}", result, serializerWatch.ElapsedMilliseconds);
#endif
                                    }
                                }
                                else if(result != null)
                                    this.m_contentTypeHandler.GetSerializer("application/json", result.GetType()).Serialize(response.OutputStream, result);

                                break;
                            case RestMessageFormat.Json:
                                response.ContentType = "application/json";
                                this.m_contentTypeHandler.GetSerializer("application/json", invoke.Method.ReturnType).Serialize(response.OutputStream, result);
                                break;
                            case RestMessageFormat.Xml:
                                response.ContentType = "application/xml";
                                this.m_contentTypeHandler.GetSerializer("application/xml", invoke.Method.ReturnType).Serialize(response.OutputStream, result);
                                break;
                        }
                }
                else
                    this.HandleAssetRenderRequest(request, response);
            }
            catch (UnauthorizedAccessException ex)
            {
                this.m_tracer.TraceError(ex.ToString());
                response.StatusCode = 403;
                var errAsset = XamarinApplicationContext.Current.LoadedApplets.ResolveAsset("/org.openiz.core/views/errors/403.html");
                var buffer = XamarinApplicationContext.Current.LoadedApplets.RenderAssetContent(errAsset, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                response.OutputStream.Write(buffer, 0, buffer.Length);

            }
            catch (SecurityException ex)
            {
                this.m_tracer.TraceError(ex.ToString());
                if (AuthenticationContext.Current.Principal == AuthenticationContext.AnonymousPrincipal)
                {
                    string redirectLocation = String.Format("{0}",
                        XamarinApplicationContext.Current.Configuration.GetSection<AppletConfigurationSection>().AuthenticationAsset
                        , request.RawUrl);
                    response.Redirect(redirectLocation);
                }
                else
                {
                    response.StatusCode = 403;

                    var errAsset = XamarinApplicationContext.Current.LoadedApplets.ResolveAsset("/org.openiz.core/views/errors/403.html");
                    var buffer = XamarinApplicationContext.Current.LoadedApplets.RenderAssetContent(errAsset, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }

            }
            catch (FileNotFoundException ex)
            {
                this.m_tracer.TraceError(ex.ToString());
                response.StatusCode = 404;
                var errAsset = XamarinApplicationContext.Current.LoadedApplets.ResolveAsset("/org.openiz.core/views/errors/404.html");
                var buffer = XamarinApplicationContext.Current.LoadedApplets.RenderAssetContent(errAsset, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                response.OutputStream.Write(buffer, 0, buffer.Length);

            }
            catch (Exception ex)
            {
                this.m_tracer.TraceError(ex.ToString());
                response.StatusCode = 500;
                var errAsset = XamarinApplicationContext.Current.LoadedApplets.ResolveAsset("/org.openiz.core/views/errors/500.html");
                var buffer = XamarinApplicationContext.Current.LoadedApplets.RenderAssetContent(errAsset, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                buffer = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(buffer).Replace("{{ exception }}", ex.ToString()));
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                try
                {
#if DEBUG
                    perfTimer.Stop();
                    this.m_tracer.TraceVerbose("PERF : MiniIMS >>>> {0} took {1} ms to service", request.Url, perfTimer.ElapsedMilliseconds);
#endif 
                    response.Close();
                }
                catch { }

                MiniImsServer.CurrentContext = null;
            }

        }

        /// <summary>
        /// Handles a service exception
        /// </summary>
        private object HandleServiceException(Exception e, InvokationInformation invoke, HttpListenerResponse response)
        {
            this.m_tracer.TraceError("{0} - {1}", invoke.Method.Name, e);

            response.StatusCode = 500;
            if(e is SecurityException)
            {
                response.StatusCode = 401;
                return invoke.FaultProvider?.Invoke(invoke.BindObject, new object[] { e });
            }
            else if (e is UnauthorizedAccessException)
            {
                response.StatusCode = 403;
                return invoke.FaultProvider?.Invoke(invoke.BindObject, new object[] { e });
            }
            else if (e is TargetInvocationException) 
                return this.HandleServiceException(e.InnerException, invoke, response);
            else
            {
                return invoke.FaultProvider?.Invoke(invoke.BindObject, new object[] { e });
            }
        }

        /// <summary>
        /// Handles the process of rendering an asset.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="response">The HTTP response.</param>
        private void HandleAssetRenderRequest(HttpListenerRequest request, HttpListenerResponse response)
		{
            // Try to demand policy 

            // Navigate asset
            AppletAsset navigateAsset = null;
            String appletPath = request.Url.AbsolutePath.ToLower();
            if (!this.m_cacheApplets.TryGetValue(appletPath, out navigateAsset))
            {

                navigateAsset = XamarinApplicationContext.Current.LoadedApplets.ResolveAsset(appletPath);

                if (navigateAsset == null)
				{
					throw new FileNotFoundException(request.RawUrl);
				}

                lock (m_lockObject)
				{
					if (!this.m_cacheApplets.ContainsKey(appletPath))
					{
						this.m_cacheApplets.Add(appletPath, navigateAsset);
					}
				}           
            }
#if DEBUG
            response.AddHeader("Cache-Control", "no-cache");
#endif

            // Navigate policy?
            if (navigateAsset.Policies != null)
			{
				foreach (var policy in navigateAsset.Policies)
				{
					new PolicyPermission(System.Security.Permissions.PermissionState.Unrestricted, policy).Demand();
				}
			}

            response.ContentType = navigateAsset.MimeType;

            // Write asset
            var content = XamarinApplicationContext.Current.LoadedApplets.RenderAssetContent(navigateAsset, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
            response.OutputStream.Write(content, 0, content.Length);

        }

        /// <summary>
        /// Stop the listener
        /// </summary>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);
            this.m_tracer.TraceInfo("Stopping IMS services...");
            this.m_listener.Stop();
            this.m_listener = null;
            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Represents service invokation information
        /// </summary>
        private class InvokationInformation
        {
            /// <summary>
            /// Allow anonymous access
            /// </summary>
            public bool Anonymous { get; internal set; }

            /// <summary>
            /// Bind object
            /// </summary>
            public Object BindObject { get; set; }

            /// <summary>
            /// Gets the demand for the overall object
            /// </summary>
            public List<String> Demand { get; internal set; }

            /// <summary>
            /// Fault provider
            /// </summary>
            public MethodInfo FaultProvider { get; set; }

            /// <summary>
            /// The method for the specified URL template
            /// </summary>
            public MethodInfo Method { get; set; }

        }
    }
}