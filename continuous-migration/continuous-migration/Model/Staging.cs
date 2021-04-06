using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Nexus.Base.CosmosDBRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace continuous_migration.Model
{
    public class Staging : ModelBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("errorMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        [JsonProperty("processedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ProcessedDate { get; set; }

        [JsonProperty("message")]
        public object Message { get; set; }

        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        public static implicit operator Staging(Document v)
        {
            Staging data = new Staging();
            data.Id = v.GetPropertyValue<String>("id");
            data.Type = v.GetPropertyValue<String>("type");
            data.Timestamp = v.GetPropertyValue<String>("timestamp");
            data.Status = v.GetPropertyValue<String>("status");
            data.ErrorMessage = v.GetPropertyValue<String>("errorMessage");
            data.ProcessedDate = v.GetPropertyValue<DateTime>("processedDate");
            data.Message = v.GetPropertyValue<Object>("message");
            data.PartitionKey = v.GetPropertyValue<String>("partitionKey");
            return data;
        }
    }
}
