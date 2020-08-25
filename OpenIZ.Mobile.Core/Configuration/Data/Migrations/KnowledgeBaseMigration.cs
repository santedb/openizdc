using OpenIZ.Mobile.Core.Knowledgebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Configuration.Data.Migrations
{
    /// <summary>
    /// Knowledge base migration
    /// </summary>
    public class KnowledgeBaseMigration : IDbMigration
    {
        /// <summary>
        /// Knowledge base migration
        /// </summary>
        public string Id => "000-install-kb-service";

        /// <summary>
        /// Description
        /// </summary>
        public string Description => "Installs the knowledgebase service";

        /// <summary>
        /// Install KB service
        /// </summary>
        public bool Install()
        {
            ApplicationContext.Current.AddServiceProvider(typeof(KnowledgebaseService));
            ApplicationContext.Current.Configuration.GetSection<ApplicationConfigurationSection>().ServiceTypes.Add(typeof(KnowledgebaseService).AssemblyQualifiedName);
            ApplicationContext.Current.SaveConfiguration();
            return true;
        }
    }
}
