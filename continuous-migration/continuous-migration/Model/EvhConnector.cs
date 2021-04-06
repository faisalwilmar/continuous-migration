using Azure.Messaging.EventHubs.Producer;
using Newtonsoft.Json;
using Nexus.Base.CosmosDBRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace continuous_migration.Model
{
    public class EvhConnector : ModelBase
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("evhName")]
        public string EvhName { get; set; }

        [JsonProperty("evhEndpoint")]
        public string EvhEndpoint { get; set; }

        [JsonProperty("expiryTimestamp")]
        public DateTime expiryTimestamp { get; set; }

        [JsonProperty("client")]
        public EventHubProducerClient client { get; set; }
    }
}
