using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using continuous_migration.Model;
using System;
using System.Text;
using System.Threading.Tasks;

namespace continuous_migration.Commons
{
    public class EventHubHandler
    {
        public static async Task SendMessage(string type, string message)
        {
            EvhConnector evhConnector = EvhConnectionHandler.GetEvhConnector(type)?.Result;
            EventHubProducerClient client = evhConnector?.client;

            EventDataBatch eventDataBatch;
            try
            {
                eventDataBatch = await client.CreateBatchAsync();
                eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(message)));
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to create batch to event hub {evhConnector.EvhName}, detail: {e.Message}");
            }

            try
            {
                await client.SendAsync(eventDataBatch);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to send data to {evhConnector.EvhEndpoint}");
            }
        }
    }
}
