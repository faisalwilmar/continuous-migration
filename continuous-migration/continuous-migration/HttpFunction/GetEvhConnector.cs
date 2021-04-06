using System;
using System.IO;
using System.Threading.Tasks;
using continuous_migration.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace continuous_migration.HttpFunction
{
    public class GetEvhConnector
    {
        private readonly CosmosClient _cosmosClient;

        public GetEvhConnector(CosmosClient client)
        {
            _cosmosClient = client;
        }

        [FunctionName("GetEvhConnector")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "evhconnector")] HttpRequest req,
            ILogger log)
        {
            try
            {
                using var actRepo = new Repositories.EvhConnectorRepository(_cosmosClient, "Migration");
                var result = await actRepo.GetAsync();

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                return new BadRequestResult();
            }
        }
    }
}

