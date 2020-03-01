using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Moq;
using Newtonsoft.Json;
using Xunit;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

namespace MhLabs.Events.Tests
{
    public class EventClientTests
    {
        private Mock<IAmazonEventBridge> _eventBridgeClientMock = new Mock<IAmazonEventBridge>(MockBehavior.Loose);
        private EventsClient _client;

        public EventClientTests()
        {
            _client = new EventsClient("testbus", "testsource", _eventBridgeClientMock.Object);
        }

        private void Validateupdate(PutEventsRequest per)
        {
            var detail = JsonConvert.DeserializeObject<EventRequest<dynamic>>(per.Entries[0].Detail);
            var diff = detail.Metadata.diff;
            Assert.Equal("{\"metadata\":{\"action\":\"update\",\"diff\":{\"Name\":[\"Old\",\"New\"]}},\"data\":{\"old\":{\"Id\":\"1\",\"Name\":\"Old\"},\"new\":{\"Id\":\"1\",\"Name\":\"New\"}}}", per.Entries[0].Detail);
        }

        private void ValidateInsert(PutEventsRequest per)
        {
            var detail = JsonConvert.DeserializeObject<EventRequest<dynamic>>(per.Entries[0].Detail);
            var diff = detail.Metadata.diff;
            Assert.Equal("{\"metadata\":{\"action\":\"create\"},\"data\":{\"old\":{},\"new\":{\"Id\":\"1\",\"Name\":\"New\"}}}", per.Entries[0].Detail);
        }

        private void ValidateRemove(PutEventsRequest per)
        {
            var detail = JsonConvert.DeserializeObject<EventRequest<dynamic>>(per.Entries[0].Detail);
            var diff = detail.Metadata.diff;
            Assert.Equal("{\"metadata\":{\"action\":\"delete\"},\"data\":{\"old\":{\"Id\":\"1\",\"Name\":\"Old\"},\"new\":{}}}", per.Entries[0].Detail);
        }

        [Fact]
        public async Task DynamoDBEvent_should_get_parsed_and_diffed_When_modified()
        {
            _eventBridgeClientMock.Setup(p => p.PutEventsAsync(It.IsAny<PutEventsRequest>(), It.IsAny<CancellationToken>())).Callback<PutEventsRequest, CancellationToken>((per, ct) => Validateupdate(per)).ReturnsAsync(new PutEventsResponse());
            var record = new DynamodbStreamRecord { EventName = "MODIFY", Dynamodb = new StreamRecord { OldImage = Document.FromJson(JsonConvert.SerializeObject(new TestEvent { Id = "1", Name = "Old" })).ToAttributeMap(), NewImage = Document.FromJson(JsonConvert.SerializeObject(new TestEvent { Id = "1", Name = "New" })).ToAttributeMap() } };
            await _client.Send("test", record);
        }

        [Fact]
        public async Task DynamoDBEvent_should_get_parsed_not_diffed_when_inserted()
        {
            _eventBridgeClientMock.Setup(p => p.PutEventsAsync(It.IsAny<PutEventsRequest>(), It.IsAny<CancellationToken>())).Callback<PutEventsRequest, CancellationToken>((per, ct) => ValidateInsert(per)).ReturnsAsync(new PutEventsResponse());
            var record = new DynamodbStreamRecord { EventName = "INSERT", Dynamodb = new StreamRecord { OldImage = Document.FromJson(JsonConvert.SerializeObject(new TestEvent { Id = "1", Name = "Old" })).ToAttributeMap(), NewImage = Document.FromJson(JsonConvert.SerializeObject(new TestEvent { Id = "1", Name = "New" })).ToAttributeMap() } };
            await _client.Send("test", record);
        }

        [Fact]
        public async Task DynamoDBEvent_should_get_parsed_not_diffed_When_removed()
        {
            _eventBridgeClientMock.Setup(p => p.PutEventsAsync(It.IsAny<PutEventsRequest>(), It.IsAny<CancellationToken>())).Callback<PutEventsRequest, CancellationToken>((per, ct) => ValidateRemove(per)).ReturnsAsync(new PutEventsResponse());
            var record = new DynamodbStreamRecord { EventName = "REMOVE", Dynamodb = new StreamRecord { OldImage = Document.FromJson(JsonConvert.SerializeObject(new TestEvent { Id = "1", Name = "Old" })).ToAttributeMap(), NewImage = Document.FromJson(JsonConvert.SerializeObject(new TestEvent { Id = "1", Name = "New" })).ToAttributeMap() } };
            await _client.Send("test", record);
        }
    }
}