using System;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.DynamoDBEvents;
using Newtonsoft.Json;
using Xunit;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

namespace MhLabs.Events.Tests
{
    public class EventRequestTests
    {

        [Fact]
        public void ItemWithoutMetadata_Should_Parse_To_Empty_Metadata_object()
        {
            var request = new EventRequest<TestEvent> { Data = new TestEvent { Id = "id", Name = "test" } };
            request.Prepare();
            Assert.Equal("{\"metadata\":{},\"data\":{\"Id\":\"id\",\"Name\":\"test\"}}", JsonConvert.SerializeObject(request));
        }

        [Fact]
        public void DynamoDBEvent_Should_Generate_Diff()
        {
        //     var request = new EventRequest<DynamodbStreamRecord> { Data = new DynamodbStreamRecord { Dynamodb = new StreamRecord { OldImage = Document.FromJson(JsonConvert.SerializeObject(new TestEvent { Id = "1", Name = "New" })).ToAttributeMap(), NewImage = Document.FromJson(JsonConvert.SerializeObject(new TestEvent { Id = "1", Name = "New" })).ToAttributeMap() } } };
        //     request.Prepare();
        //     Assert.Equal("{\"metadata\":{},\"data\":{\"Id\":\"id\",\"Name\":\"test\"}}", JsonConvert.SerializeObject(request));
        }
    }
}