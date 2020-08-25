using OpenIZ.Core.Applets.Model;
using OpenIZ.Core.Applets.Services;
using OpenIZ.Mobile.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Knowledgebase
{
    /// <summary>
    /// Knowledgebase service
    /// </summary>
    public class KnowledgebaseService : IKnowledgeBaseService
    {

        // Entries
        private IEnumerable<KnowledgeBaseEntry> m_entries = null;

        /// <summary>
        /// Get entries
        /// </summary>
        /// <returns></returns>
        private IEnumerable<KnowledgeBaseEntry> GetEntries()
        {

            if (this.m_entries == null)
            {
                var applets = ApplicationContext.Current.GetService<IAppletManagerService>().Applets;
                this.m_entries = applets.Select(a => a.Assets.FirstOrDefault(s => s.Name == "kb.xml"))
                    .OfType<AppletAsset>()
                    .SelectMany(asset =>
                    {
                        using (var ms = new MemoryStream(applets.RenderAssetContent(asset)))
                            return KnowledgeBaseCollection.Load(ms).Entry;
                    })
                    .ToList();
            }
            return this.m_entries;
        }

        /// <summary>
        /// Get the
        /// </summary>
        public KnowledgeBaseEntry GetEntry(Exception e)
        {
            if (e == null)
                return null;
            return this.GetEntries().FirstOrDefault(o => o.Matches(e.Message));
        }

        /// <summary>
        /// Get entry from exception message
        /// </summary>
        public KnowledgeBaseEntry GetEntry(string exceptionMessage)
        {
            if (String.IsNullOrEmpty(exceptionMessage))
                return null;
            return this.GetEntries().FirstOrDefault(o => o.Matches(exceptionMessage));
        }
    }
}
