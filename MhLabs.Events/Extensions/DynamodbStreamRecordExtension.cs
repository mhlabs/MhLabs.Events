using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json.Linq;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

namespace MhLabs.Events.Extensions
{
    public static class DynamodbStreamRecordExtension
    {
        private static readonly Dictionary<string, string> _actionMap = new Dictionary<string, string> {
            {"INSERT", "create"},
            {"MODIFY", "update"},
            {"REMOVE", "delete"},
        };

        public static EventRequest<dynamic> ToEventRequest(this DynamodbStreamRecord record)
        {
            var request = new EventRequest<dynamic>();
            dynamic data = new ExpandoObject();
            var dict = (IDictionary<string, object>) data;

            if (record.EventName == "REMOVE" || record.EventName == "MODIFY")
            {
                dict["old"] = JToken.Parse(Document.FromAttributeMap(record.Dynamodb.OldImage).ToJson());
            }
            else
            {
                dict["old"] = JToken.Parse("{}");
            }

            if (record.EventName == "INSERT" || record.EventName == "MODIFY")
            {
                dict["new"] = JToken.Parse(Document.FromAttributeMap(record.Dynamodb.NewImage).ToJson());
            }
            else
            {
                dict["new"] = JToken.Parse("{}");
            }
            request.Metadata = request.Metadata ?? new ExpandoObject();
            request.Metadata.action = _actionMap[record.EventName];

            request.Data = data;
            return request;
        }
    }
}