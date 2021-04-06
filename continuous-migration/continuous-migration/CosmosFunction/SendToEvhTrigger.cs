using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using continuous_migration.Commons;
using continuous_migration.Model;
using continuous_migration.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace continuous_migration.CosmosFunction
{
    public static class SendToEvhTrigger
    {
        [FunctionName("SendToEvhTrigger")]
        public static async Task RunAsync([CosmosDBTrigger(
            databaseName: "Migration",
            collectionName: "Staging",
            ConnectionStringSetting = "cosmos-bl-tutorial-serverless",
            LeaseCollectionName = "leases",
            MaxItemsPerInvocation = 10,
            FeedPollDelay = 1000,
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents, ILogger log)
        {
            string cosmosBlConnString = Environment.GetEnvironmentVariable("cosmos-bl-tutorial-serverless");
            CosmosClient cosmosClient = new CosmosClient(cosmosBlConnString);

            List<Task> tasklist = new List<Task>();
            List<Staging> stagings = new List<Staging>();

            foreach (var document in documents)
            {
                Staging data = document;
                stagings.Add(data);
            }

            var orderedStagings = stagings.OrderBy(p => p.Timestamp).ToList();
            foreach (var document in orderedStagings)
            {
                Staging data = document;
                tasklist.Add(ProcessDocument(data, cosmosClient, log));
            }
            Task.WaitAll(tasklist.ToArray());
        }

        private static async Task ProcessDocument(Staging data, CosmosClient cosmos, ILogger log)
        {
            if (data.Status == "" || data.Status == null)
            {
                try
                {
                    // status to processing
                    data.Status = "Processing";
                    data.ProcessedDate = DateTime.Now;
                    using var actRepo = new Repositories.StagingRepository(cosmos, "Migration");
                    await actRepo.UpdateAsync(data.Id, data);

                    // send to evh
                    dynamic messageData = data.Message;
                    messageData.sentEvhTs = DateTime.Now;
                    data.Message = messageData;
                    String message = JsonConvert.SerializeObject(data.Message);
                    await EventHubHandler.SendMessage(data.Type, message);

                    // update status to done
                    data.Status = "Processed";
                    data.ProcessedDate = DateTime.Now;
                    await actRepo.UpdateAsync(data.Id, data);
                }
                catch (Exception e)
                {
                    // status to processing
                    data.Status = "Error";
                    data.ErrorMessage = $"{e.Message}, {e.StackTrace}";
                    using var actRepo = new Repositories.StagingRepository(cosmos, "Migration");
                    await actRepo.UpdateAsync(data.Id, data);

                    log.LogError($"{e.Message}, {e.StackTrace}");
                }
            }
        }
    }
}
