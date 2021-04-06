using System;
using System.IO;
using System.Threading.Tasks;
using continuous_migration.DTO;
using continuous_migration.Model;
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
    public class CreateActivity
    {
        private readonly CosmosClient _cosmosClient;

        public CreateActivity(CosmosClient client)
        {
            _cosmosClient = client;
        }

        [FunctionName("CreateActivity")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = "activity")] ActivityDTO req,
            ILogger log)
        {
            try
            {
                var act = new Activity()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActivityName = req.ActivityName,
                    Description = req.Description
                };

                using var actRepo = new Repositories.ActivityRepository(_cosmosClient, "Migration");
                var result = await actRepo.CreateAsync(act);

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                return new BadRequestResult();
            }
        }
    }
}

