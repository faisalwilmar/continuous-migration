using Newtonsoft.Json;
using Nexus.Base.CosmosDBRepository;
using System;

namespace continuous_migration.Model
{
    public class MigrationLog : ModelBase
    {
        [JsonProperty("sourceId")]
        public string SourceId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("processingDate")]
        public DateTime ProcessingDate { get; set; }

        [JsonProperty("subscriberName")]
        public string SubscriberName { get; set; }

        [JsonProperty("eventHubName")]
        public string EventHubName { get; set; }
    }
}
