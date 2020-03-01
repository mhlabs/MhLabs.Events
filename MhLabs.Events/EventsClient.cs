using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using MhLabs.Events.Extensions;
using Newtonsoft.Json;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

namespace MhLabs.Events
{

    public class EventsClient : IEventsClient
    {
        private readonly IAmazonEventBridge _eventBridgeClient;
        private readonly string _eventBusName;
        private readonly string _source;

        public EventsClient(string eventBusName, string source = null, IAmazonEventBridge eventBridgeClient = null)
        {
            _eventBridgeClient = eventBridgeClient ?? new AmazonEventBridgeClient();
            _eventBusName = eventBusName;
            _source = source ?? Environment.GetEnvironmentVariable("StackName");
        }

        public async Task<EventResponse> Send<T>(string detailType, T item)
        {
            return await Send(detailType, (IEnumerable<T>) new List<T> { item });
        }

        public async Task<EventResponse> Send<T>(string detailType, IEnumerable<T> items)
        {
            return await Send(detailType, items.Select(p => new EventRequest<T> { Data = p }));
        }

        public async Task<EventResponse> Send(string detailType, IList<DynamodbStreamRecord> items)
        {
            return await Send(detailType, items.Select(p => p.ToEventRequest()));
        }
        public async Task<EventResponse> Send(string detailType, DynamodbStreamRecord item)
        {
            return await Send(detailType, (IList<DynamodbStreamRecord>) new List<DynamodbStreamRecord> { item });
        }

        public async Task<EventResponse> Send<T>(string detailType, IEnumerable<EventRequest<T>> items)
        {
            if (items.Count() > 10)
            {
                throw new ArgumentException("You can send max 10 items in one request");
            }

            var request = new PutEventsRequest
            {
                Entries = items.Select(p => new PutEventsRequestEntry
                {
                Detail = JsonConvert.SerializeObject(p.Prepare()),
                DetailType = detailType,
                Source = _source,
                EventBusName = _eventBusName
                }).ToList()
            };

            var response = await _eventBridgeClient.PutEventsAsync(request);

            return new EventResponse
            {
                FailedCount = response.FailedEntryCount,
                    FailedReasons = response.Entries.Where(p => string.IsNullOrEmpty(p.EventId)).Select(p => p.ErrorMessage).ToList()
            };
        }
    }
}