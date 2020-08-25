using Newtonsoft.Json;
using OpenIZ.Mobile.Core.Knowledgebase;
using OpenIZ.Mobile.Core.Synchronization.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Xamarin.Services.Model
{
    /// <summary>
    /// Represents a queue view model 
    /// </summary>
    [JsonObject]
    public class SynchronizationQueueViewModel 
    {

        public SynchronizationQueueViewModel()
        {

        }

        /// <summary>
        /// Synchronization queue entry
        /// </summary>
        public SynchronizationQueueViewModel(SynchronizationQueueEntry queueEntry)
        {
            this.Id = queueEntry.Id;
            this.CreationTime = queueEntry.CreationTime;
            this.IsRetry = queueEntry.IsRetry;
            this.Operation = queueEntry.Operation;
            this.MessageType = Type.GetType(queueEntry.Type).Name ?? queueEntry.Type;
            if (queueEntry is DeadLetterQueueEntry dead) {
                this.Exception = Encoding.UTF8.GetString((queueEntry as DeadLetterQueueEntry)?.TagData);
                this.OriginalQueue = dead.OriginalQueue;
            }
            if(queueEntry is OutboundQueueEntry outbound)
            {
                this.RetryCount = outbound.RetryCount;
            }

        }


        /// <summary>
        /// Synchronization queue entry
        /// </summary>
        public SynchronizationQueueViewModel(SynchronizationQueueEntry queueEntry, byte[] data): this(queueEntry)
        {
            this.Payload = data;
        }

        /// <summary>
        /// The Structured exception object
        /// </summary>
        [JsonProperty("exception")]
        public String Exception { get; }

        /// <summary>
        /// Gets the original queue
        /// </summary>
        [JsonProperty("originalQueue")]
        public string OriginalQueue { get; }

        /// <summary>
        /// The knowledgebase entry
        /// </summary>
        [JsonProperty("kb")]
        public KnowledgeBaseEntry KbEntry
        {
            get
            {
                return ApplicationContext.Current.GetService<IKnowledgeBaseService>()?.GetEntry(this.Exception);
            }
        }

        /// <summary>
        /// Gets the identifier
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; }

        /// <summary>
        /// Get the creation time
        /// </summary>
        [JsonProperty("creationTime")]
        public DateTime CreationTime { get; }

        /// <summary>
        /// Get the payload
        /// </summary>
        [JsonProperty("payload")]
        public byte[] Payload { get; }

        /// <summary>
        /// True if this is a retry
        /// </summary>
        [JsonProperty("isRetry")]
        public bool IsRetry { get; }

        /// <summary>
        /// Operation
        /// </summary>
        [JsonProperty("operation")]
        public DataOperationType Operation { get; }

        /// <summary>
        /// Gets the type of message
        /// </summary>
        [JsonProperty("type")]
        public string MessageType { get; }

        /// <summary>
        /// Retry count
        /// </summary>
        [JsonProperty("count")]
        public int RetryCount { get; }
    }
}
