using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using continuous_migration.Commons;
using continuous_migration.DTO;
using continuous_migration.Model;
using continuous_migration.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace continuous_migration.EventHubFunction
{
    public class SyncActivity : SyncAbstractClass<ActivityDTO>
    {
        private IMapper _mapper { get; set; }
        private readonly string TYPE = "act-migration";
        private readonly string EVENT_HUB_NAME = "activity.migration";
        private readonly string SUBSCRIBER_NAME = "SyncActivity";

        public SyncActivity()
        {
            if (_mapper == null)
            {
                var config = new MapperConfiguration(cfg => {
                    cfg.CreateMap<Activity, Activity>();
                    cfg.CreateMap<ActivityDTO, Activity>();
                });
                _mapper = config.CreateMapper();
            }
        }

        [FunctionName("SyncActivity")]
        public async Task Run([EventHubTrigger("activity.migration", Connection = "EventHubConnectionString")] EventData[] events, ILogger log)
        {
            string cosmosBlConnString = Environment.GetEnvironmentVariable("cosmos-bl-tutorial-serverless");
            CosmosClient cosmosClient = new CosmosClient(cosmosBlConnString);
            await ReadEvents(events, cosmosClient, TYPE, _mapper, log, SUBSCRIBER_NAME, EVENT_HUB_NAME);
        }

        protected override async Task OnValidating(string messageBody, dynamic dto, EventArgs e)
        {
            await base.OnValidating(messageBody, (ActivityDTO)dto, e);

            if (string.IsNullOrEmpty(dto?.ActivityName)
                || string.IsNullOrEmpty(dto?.Description))
                throw new Exception("Some data needed for fetching Activity data (ActivityName, Description) are missing");
        }

        protected override async Task OnPreProcessing(CosmosClient client, string type, ActivityDTO dto, IMapper mapper, ILogger log, Dictionary<string, object> dict, EventArgs e)
        {
            using var repsActivity = new Repositories.ActivityRepository(client, "Migration");

            var currentActivity = await repsActivity.GetByIdAsync(dto.Id);

            if (currentActivity == null)
                //throw new Exception("No matching activity found");
                _mapper.Map(dto, currentActivity);

            dict.Add("currentActivity", currentActivity);
        }

        protected override async Task OnProcessing(CosmosClient client, string type, ActivityDTO dto, IMapper mapper, ILogger log, Dictionary<string, object> dict, EventArgs e)
        {
            using var repsActivity = new Repositories.ActivityRepository(client,"Migration");

            var currentActivity = (Activity)dict["currentActivity"];

            if (dto.ActionType?.ToLower() == "insert")
            {
                var existingActivities = (await repsActivity.GetAsync(p =>
                       p.Id == dto.Id
                    && p.ActivityName == dto.ActivityName
                )).Items;

                if (existingActivities.Count() > 1)
                {
                    await Utils.SaveLog(client, dto?.SourceId, "Duplicate existing classes. Processing only the first one.", TYPE);
                }

                var existingActivity = existingActivities.FirstOrDefault();

                var activityModel = new Activity();
                if (existingActivity == null)
                {
                    _mapper.Map(dto, activityModel);
                    await repsActivity.CreateAsync(activityModel);
                }
                else if (existingActivity?.ActiveFlag == "N")
                {
                    // update if previous flag is delete
                    _mapper.Map(existingActivity, activityModel);
                    activityModel.ActiveFlag = "Y";
                    activityModel.Description = currentActivity?.Description;
                    await repsActivity.UpdateAsync(existingActivity.Id, activityModel);
                }
            }
            else if (dto.ActionType?.ToLower() == "delete")
            {
                var existingActivity = (await repsActivity.GetAsync(p =>
                       p.Id == dto.Id
                    && p.ActivityName == dto.ActivityName
                )).Items.FirstOrDefault();

                if (existingActivity == null)
                {
                    throw new Exception("No existing class found");
                }

                if (existingActivity?.ActiveFlag == "Y")
                {
                    existingActivity.ActiveFlag = "N";
                    await repsActivity.UpdateAsync(existingActivity.Id, existingActivity);
                }
            }
            else if (dto.ActionType?.ToLower() == "update")
            {
                var existingActivity = (await repsActivity.GetAsync(p =>
                       p.Id == dto.Id
                    && p.ActivityName == dto.ActivityName
                )).Items.FirstOrDefault();

                if (existingActivity == null)
                {
                    throw new Exception("No existing class found");
                }

                if (existingActivity?.ActiveFlag == "Y")
                {
                    var activityModel = new Activity();
                    _mapper.Map(dto, activityModel);
                    await repsActivity.UpdateAsync(existingActivity.Id, activityModel);
                }
            }
        }
    }
}
