using Newtonsoft.Json;
using Nexus.Base.CosmosDBRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace continuous_migration.Model
{
    public class Activity : ModelBase
    {
        [JsonProperty(propertyName: "id")]
        public string Id { get; set; }

        [JsonProperty(propertyName: "activityName")]
        public string ActivityName { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }
    }
}
