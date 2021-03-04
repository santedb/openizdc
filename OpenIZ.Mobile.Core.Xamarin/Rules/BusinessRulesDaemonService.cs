/*
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
using OpenIZ.BusinessRules.JavaScript;
using OpenIZ.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenIZ.Mobile.Core.Xamarin.Resources;
using OpenIZ.Core.Diagnostics;
using OpenIZ.Core.Applets.ViewModel.Description;
using OpenIZ.Core;
using OpenIZ.Core.Applets.Services;

namespace OpenIZ.Mobile.Core.Xamarin.Rules
{
    /// <summary>
    /// Business rules service which adds javascript files
    /// </summary>
    public class BusinessRulesDaemonService : IDaemonService
    {

        private Tracer m_tracer = Tracer.GetTracer(typeof(BusinessRulesDaemonService));

        /// <summary>
        /// Indicates whether the service is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return true;
            }
        }

        public event EventHandler Started;
        public event EventHandler Starting;
        public event EventHandler Stopped;
        public event EventHandler Stopping;

        /// <summary>
        /// Start the service which will register items with the business rules handler
        /// </summary>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);
            ApplicationContext.Current.Started += (o, e) =>
            {
                try
                {
                    ApplicationServiceContext.Current = ApplicationContext.Current;
                    ApplicationServiceContext.HostType = OpenIZHostType.OtherClient;

                    if (ApplicationContext.Current.GetService<IDataReferenceResolver>() == null)
                    {
                        ApplicationContext.Current.AddServiceProvider(typeof(AppletDataReferenceResolver));
                    }

                    var appletManager = ApplicationServiceContext.Current.GetService(typeof(IAppletManagerService)) as IAppletManagerService;
                    JavascriptBusinessRulesEngine.InitializeGlobal();
                    foreach (var itm in appletManager.Applets.SelectMany(a => a.Assets).Where(a => a.Name.StartsWith("rules/")))
                        using (StreamReader sr = new StreamReader(new MemoryStream(appletManager.Applets.RenderAssetContent(itm))))
                        {
                            ApplicationContext.Current.SetProgress($"Init Rules {itm.Name}", 0.5f);

                            this.m_tracer.TraceWarning("++> Adding rules from {0}", itm.Name);
                            JavascriptBusinessRulesEngine.AddRulesGlobal(itm.Name, sr);
                            //OpenIZ.BusinessRules.JavaScript.JavascriptBusinessRulesEngine.Current.AddRules(itm.Name, sr);
                        }

                }
                catch (Exception ex)
                {
                    this.m_tracer.TraceError("Error starting up business rules daemon: {0}", ex);
                    throw new Exception("Unable to initialize business rules", ex);
                }
            };
            this.Started?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Stopping
        /// </summary>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);
            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}
