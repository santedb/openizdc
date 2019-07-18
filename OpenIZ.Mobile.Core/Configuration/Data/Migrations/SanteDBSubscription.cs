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
    public class SanteDBSubscription : IDbMigration
    {

        private Tracer tracer = Tracer.GetTracer(typeof(SanteDBSubscription));

        /// <summary>
        /// Get the identity of the migration
        /// </summary>
        public string Id => "zz-santedb-subscription";

        /// <summary>
        /// Get the description
        /// </summary>
        public string Description => "Migrates OpenIZ 1.x to SanteDB 2.x subscription system";

        /// <summary>
        /// Install the extension
        /// </summary>
        public bool Install()
        {

            var syncSection = ApplicationContext.Current.Configuration.GetSection<SynchronizationConfigurationSection>();

            var facility = syncSection.Facilities.FirstOrDefault();

            var removed = syncSection.SynchronizationResources.RemoveAll(s =>
                s.Filters.Any()  &&  
                s.Filters.All(f => !f.Contains("id=") && f.Contains(facility)) &&
                s.Name != "locale.sync.resource.Place.outOfState" &&
                s.Name != "locale.sync.resource.Place.state"
            );
            this.tracer.TraceInfo("Removed {0} subscriptions...", removed);
            foreach(var s in syncSection.SynchronizationResources.Where(s=> s.Name != "locale.sync.resource.Place.outOfState" &&
                s.Name != "locale.sync.resource.Place.state"))
                s.Filters.RemoveAll(f => !f.Contains("id=") && f.Contains(facility));
            syncSection.SynchronizationResources.RemoveAll(s => 
                s.Filters.Any() && 
                s.Filters.All(f => f.Contains("_subscription")) &&
                s.Name != "locale.sync.resource.Place.outOfState" &&
                s.Name != "locale.sync.resource.Place.state");

            // Add corrected subscriptions
            syncSection.SynchronizationResources.Add(new SynchronizationResource()
            {
                Triggers = SynchronizationPullTriggerType.Always,
                Name = "locale.sync.entity.my",
                ResourceAqn = "Entity",
                Filters = new List<string>()
                {
                    $"_subscription=81b65812-c14e-4bb4-b7a1-ca7bcee83dbc&_placeid={facility}&_expand=relationship&_expand=participation"
                }
            });
            syncSection.SynchronizationResources.Add(new SynchronizationResource()
            {
                Triggers = SynchronizationPullTriggerType.Always,
                Name = "locale.sync.act.my",
                ResourceAqn = "Act",
                Filters = new List<string>()
                {
                    $"_subscription=decad40c-a232-482f-b93d-317b25c1ef0d&_placeid={facility}&_expand=relationship&_expand=participation"
                }
            });


            // Now get the synchronization log and translate the earliest value to this
            try
            {
                var lastSync = SynchronizationLog.Current.GetAll().Min(o => o.LastSync);
                SynchronizationLog.Current.Save(typeof(Act), $"_subscription=decad40c-a232-482f-b93d-317b25c1ef0d&_placeid={facility}&_expand=relationship_expand=participation", null, "locale.sync.act.my", lastSync);
                SynchronizationLog.Current.Save(typeof(Entity), $"_subscription=81b65812-c14e-4bb4-b7a1-ca7bcee83dbc&_placeid={facility}&_expand=relationship_expand=participation", null, "locale.sync.entity.my", lastSync);
            }
            catch(Exception e)
            {
                this.tracer.TraceError("Cannot set last sync date: {0}", e);
            }
            return true;
        }
    }
}
