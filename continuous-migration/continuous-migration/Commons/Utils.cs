using continuous_migration.Model;
using continuous_migration.Repository;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace continuous_migration.Commons
{
    public class Utils
    {
        public static async Task SaveLog(CosmosClient client, string sourceId, string message, string type = "", string subscriberName = "", string eventHubName = "")
        {
            var migrationLog = new MigrationLog
            {
                SourceId = sourceId,
                Message = message,
                Type = type,
                ProcessingDate = DateTime.UtcNow,
                SubscriberName = subscriberName,
                EventHubName = eventHubName
            };

            var migrationLogRepo = new Repositories.MigrationLogRepository(client, "Migration");
            await migrationLogRepo.CreateAsync(migrationLog);
        }
    }
}
