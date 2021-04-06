using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace continuous_migration.DTO
{
    public class ActivityDTO
    {
        [JsonProperty(propertyName: "id")]
        public string Id { get; set; }

        [JsonProperty(propertyName: "activityName")]
        public string ActivityName { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }
    }
}
