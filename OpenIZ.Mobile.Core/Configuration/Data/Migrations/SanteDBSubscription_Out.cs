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
 * Date: 2019-7-8
 */
using OpenIZ.Core.Diagnostics;
using OpenIZ.Core.Model.Acts;
using OpenIZ.Core.Model.Entities;
using OpenIZ.Mobile.Core.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Configuration.Data.Migrations
{
    /// <summary>
    /// SanteDB Subscription correction
    /// </summary>
    public class SanteDBSubscriptionOut : IDbMigration
    {

        private Tracer tracer = Tracer.GetTracer(typeof(SanteDBSubscription));

        /// <summary>
        /// Get the identity of the migration
        /// </summary>
        public string Id => "zz-santedb-subscription-v2";

        /// <summary>
        /// Get the description
        /// </summary>
        public string Description => "Adds performance enhanced subscription for acts to OpenIZ";

        /// <summary>
        /// Install the extension
        /// </summary>
        public bool Install()
        {

            var syncSection = ApplicationContext.Current.Configuration.GetSection<SynchronizationConfigurationSection>();

            var facility = syncSection.Facilities.FirstOrDefault();

            // Add corrected subscriptions
            syncSection.SynchronizationResources.Add(new SynchronizationResource()
            {
                Triggers = SynchronizationPullTriggerType.Always,
                Name = "locale.sync.entity.out",
                ResourceAqn = "Entity",
                Filters = new List<string>()
                {
                    $"_subscription=81b65812-c14e-4bb4-b7a1-ca7bcee83dbf&_placeid={facility}&_expand=relationship&_expand=participation"
                }
            });

            // Now get the synchronization log and translate the earliest value to this
            try
            {
                var lastSync = SynchronizationLog.Current.GetAll().Min(o => o.LastSync);
                SynchronizationLog.Current.Save(typeof(Entity), $"_subscription=81b65812-c14e-4bb4-b7a1-ca7bcee83dbf&_placeid={facility}&_expand=relationship_expand=participation", null, "locale.sync.entity.out", lastSync);
            }
            catch(Exception e)
            {
                this.tracer.TraceError("Cannot set last sync date: {0}", e);
            }
            return true;
        }
    }
}
