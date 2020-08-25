using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Knowledgebase
{
    /// <summary>
    /// Knowledgebase service
    /// </summary>
    public interface IKnowledgeBaseService
    {

        /// <summary>
        /// Get the entry 
        /// </summary>
        KnowledgeBaseEntry GetEntry(Exception e);

        /// <summary>
        /// Get entry by message
        /// </summary>
        KnowledgeBaseEntry GetEntry(String exceptionMessage);

    }
}
