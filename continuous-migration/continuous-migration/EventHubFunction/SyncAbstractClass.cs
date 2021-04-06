using AutoMapper;

using continuous_migration.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace continuous_migration.EventHubFunction
{
    public abstract class SyncAbstractClass<T>
    {
        protected event EventHandler PreProcessing;
        protected event EventHandler Processing;
        protected event EventHandler PostProcessing;
        protected event EventHandler ErrorProcessing;
        protected event EventHandler Finished;

        protected virtual async Task ReadEvents(EventData[] events, CosmosClient client, string type, IMapper mapper, ILogger log,
            string subscriberName = "", string eventHubName = "")
        {
            var exceptions = new List<Exception>();
            foreach (var eventData in events)
            {
                T dto = default;
                var dict = new Dictionary<string, Object>();
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    dto = JsonConvert.DeserializeObject<T>(messageBody);

                    await OnValidating(messageBody, dto, EventArgs.Empty);

                    await OnPreProcessing(client, type, dto, mapper, log, dict, EventArgs.Empty);

                    await OnProcessing(client, type, dto, mapper, log, dict, EventArgs.Empty);

                    await OnPostProcessing(client, type, dto, mapper, log, dict, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    await OnErrorProcessing(client, type, log, dto, e, exceptions, EventArgs.Empty, subscriberName, eventHubName);
                }
            }
            await OnFinished(exceptions, EventArgs.Empty);
        }

        protected virtual async Task ReadRequests(HttpRequest req, CosmosClient client, string type, IMapper mapper, ILogger log,
            string subscriberName = "", string eventHubName = "")
        {
            var exceptions = new List<Exception>();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var dtoList = JsonConvert.DeserializeObject<List<T>>(requestBody);

            foreach (var dto in dtoList)
            {
                var dict = new Dictionary<string, Object>();
                try
                {
                    await OnValidating(requestBody, dto, EventArgs.Empty);

                    await OnPreProcessing(client, type, dto, mapper, log, dict, EventArgs.Empty);

                    await OnProcessing(client, type, dto, mapper, log, dict, EventArgs.Empty);

                    await OnPostProcessing(client, type, dto, mapper, log, dict, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    await OnErrorProcessing(client, type, log, dto, e, exceptions, EventArgs.Empty, subscriberName, eventHubName);
                }
            }
            await OnFinished(exceptions, EventArgs.Empty);
        }

        /// <summary>
        /// Default validation that supposed to be used in general
        /// </summary>
        /// <param name="messageBody">String parsed from the eventBody</param>
        /// <param name="dto">The receiving DTO of this subscriber, for current iteration</param>
        /// <param name="e">Eventargs (not used yet)</param>
        /// <returns>Task</returns>
        protected virtual async Task OnValidating(string messageBody, dynamic dto, EventArgs e)
        {
            var errorMessage = "";

            if (messageBody == "")
            {
                errorMessage = "Message is empty";
                throw new Exception(errorMessage);
            }

            if (dto?.ActionType == "" || dto?.ActionType == null)
            {
                errorMessage = "Action Type is empty";
                throw new Exception(errorMessage);
            }

        }

        /// <summary>
        /// Preprocessing, can be used to create custom validation
        /// </summary>
        /// <param name="client">A CosmosClient used to bind repository with CosmosDB</param>
        /// <param name="type">Type of eventHubTrigger</param>,
        /// <param name="dto">The receiving DTO of this subscriber, for current iteration</param>
        /// <param name="mapper">An AutoMapper object used to map from one object to another</param>
        /// <param name="log">Object for console logging</param>
        /// <param name="dict">A dictionary that can be used to pass object to other functions</param>
        /// <param name="e">Eventargs (not used yet)</param>
        /// <returns>Task</returns>
        protected virtual async Task OnPreProcessing(CosmosClient client, string type, T dto, IMapper mapper, ILogger log, Dictionary<string, object> dict, EventArgs e)
        {

        }

        /// <summary>
        /// Sync Processing
        /// </summary>
        /// <param name="client">A CosmosClient used to bind repository with CosmosDB</param>
        /// <param name="type">Type of eventHubTrigger</param>,
        /// <param name="dto">The receiving DTO of this subscriber, for current iteration</param>
        /// <param name="mapper">An AutoMapper object used to map from one object to another</param>
        /// <param name="log">Object for console logging</param>
        /// <param name="dict">A dictionary that can be used to pass object to other functions</param>
        /// <param name="e">Eventargs (not used yet)</param>
        /// <returns>Task</returns>
        protected virtual async Task OnProcessing(CosmosClient client, string type, T dto, IMapper mapper, ILogger log, Dictionary<string, object> dict, EventArgs e)
        {

        }

        /// <summary>
        /// Sync post processing
        /// </summary>
        /// <param name="client">A CosmosClient used to bind repository with CosmosDB</param>
        /// <param name="type">Type of eventHubTrigger</param>,
        /// <param name="dto">The receiving DTO of this subscriber, for current iteration</param>
        /// <param name="mapper">An AutoMapper object used to map from one object to another</param>
        /// <param name="log">Object for console logging</param>
        /// <param name="dict">A dictionary that can be used to pass object to other functions</param>
        /// <param name="e">Eventargs (not used yet)</param>
        /// <returns>Task</returns>
        protected virtual async Task OnPostProcessing(CosmosClient client, string type, T dto, IMapper mapper, ILogger log, Dictionary<string, object> dict, EventArgs e)
        {

        }

        /// <summary>
        /// Error in processing
        /// </summary>
        /// <param name="client">A CosmosClient used to bind repository with CosmosDB</param>
        /// <param name="type">Type of eventHubTrigger</param>
        /// <param name="log">Object for console logging</param>
        /// <param name="dto">The receiving DTO of this subscriber, for current iteration</param>
        /// <param name="ex">Exception catched for current eventData iteration</param>
        /// <param name="exceptions">List of exceptions</param>
        /// <param name="e">Eventargs (not used yet)</param>
        /// <returns></returns>
        protected virtual async Task OnErrorProcessing(CosmosClient client, string type, ILogger log, dynamic dto, Exception ex, List<Exception> exceptions, EventArgs e,
            string subscriberName = "", string eventHubName = "")
        {
            string errorString = $"{ex.Message}, {ex.StackTrace}";
            log.LogError(errorString);
            await Utils.SaveLog(client, dto?.SourceId, errorString, type, subscriberName, eventHubName);
            exceptions.Add(ex);
        }

        /// <summary>
        /// Finished reading all events
        /// </summary>
        /// <param name="exceptions">List of all exceptions gathered whilst this subscriber is running</param>
        /// <param name="e">Eventargs (not used yet)</param>
        /// <returns></returns>
        protected virtual async Task OnFinished(List<Exception> exceptions, EventArgs e)
        {
            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
