using Azure.Messaging.EventHubs.Producer;
using continuous_migration.Model;
using continuous_migration.Repository;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace continuous_migration.Commons
{
    public class EvhConnectionHandler
    {
        private static Dictionary<string, EvhConnector> _connections = new Dictionary<string, EvhConnector>();

        public static async Task<EvhConnector> GetEvhConnector(string type)
        {
            if (_connections.ContainsKey(type))
            {
                var item = _connections[type];
                if (item?.expiryTimestamp <= DateTime.UtcNow)
                {
                    _connections.Remove(type);
                    var NewItem = await GetEvhConnectorFromDB(type);
                    _connections.TryAdd(type, NewItem);
                }
                return _connections[type];
            }
            else
            {
                var item = await GetEvhConnectorFromDB(type);
                _connections.TryAdd(type, item);
                return item;
            }
        }

        private static async Task<EvhConnector> GetEvhConnectorFromDB(string type)
        {
            var refreshTime = Environment.GetEnvironmentVariable("EvhConnectorRefreshMinutes");


            var tmpRefreshTime = 30.0;
            Double.TryParse(refreshTime, out tmpRefreshTime);

            string cosmosBlConnString = Environment.GetEnvironmentVariable("cosmos-bl-tutorial-serverless");
            CosmosClient cosmosClient = new CosmosClient(cosmosBlConnString);
            //Biar bisa ambil singleton yang ada di Startup.cs gimana ya?

            using var actRepo = new Repositories.EvhConnectorRepository(cosmosClient, "Migration");
            var conns = await actRepo.GetAsync($"SELECT * FROM c WHERE c.type='{type}'",usePaging: false);

            var item = new List<EvhConnector>();
            foreach (var connection in conns.Items)
            {
                item.Add(connection);
            }

            if (item?.Count > 0)
            {
                var evhItem = item[0];

                evhItem.expiryTimestamp = DateTime.UtcNow.AddMinutes(tmpRefreshTime);
                evhItem.client = new EventHubProducerClient(evhItem.EvhEndpoint, evhItem.EvhName);

                return item[0];
            }
            else
            {
                throw new Exception($"Type '{type}' is not available in 'Migration/EvhConnector'.");
            }


        }
    }
}
