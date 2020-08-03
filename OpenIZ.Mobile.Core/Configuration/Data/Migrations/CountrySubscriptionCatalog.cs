using OpenIZ.Core.Diagnostics;
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
    public class CountrySubscription : IDbMigration
    {

        private Tracer tracer = Tracer.GetTracer(typeof(SanteDBSubscription));

        /// <summary>
        /// Get the identity of the migration
        /// </summary>
        public string Id => "zz-santedb-subscription-cty";

        /// <summary>
        /// Get the description
        /// </summary>
        public string Description => "Adds subscription for countries";

        /// <summary>
        /// Install the extension
        /// </summary>
        public bool Install()
        {

            var syncSection = ApplicationContext.Current.Configuration.GetSection<SynchronizationConfigurationSection>();

            // Add corrected subscriptions
            syncSection.SynchronizationResources.Add(new SynchronizationResource()
            {
                Triggers = SynchronizationPullTriggerType.OnStart,
                Name = "locale.sync.countries",
                ResourceAqn = "Place",
                Filters = new List<string>()
                {
                    $"classConcept=48b2ffb3-07db-47ba-ad73-fc8fb8502471&_expand=relationship&_expand=participation"
                }
            });

            return true;
        }
    }
}
