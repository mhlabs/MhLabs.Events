using System;
using System.Collections.Generic;
using System.Dynamic;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

namespace MhLabs.Events
{
    public class EventRequest<T>
    {
        private static readonly JsonDiffPatch _jsonDiffer = new JsonDiffPatch();

        [JsonProperty("metadata")]
        public dynamic Metadata { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        public EventRequest<T> Prepare()
        {
            if (Metadata == null)
            {
                Metadata = new ExpandoObject();
            }

            dynamic expando = Data as ExpandoObject;

            if (expando != null)
            {
                var dict = (IDictionary<string, object>) expando;
                if (dict.ContainsKey("new") && dict.ContainsKey("old"))
                {
                    var oldJson = JsonConvert.SerializeObject(dict["old"]);
                    var newJson = JsonConvert.SerializeObject(dict["new"]);
                    if (oldJson == "{}" || newJson == "{}") {
                        return this;
                    }
                    var oldImage = JToken.Parse(oldJson);
                    var newImage = JToken.Parse(newJson);

                    var diff = _jsonDiffer.Diff(oldImage, newImage);

                    Metadata.diff = diff;
                }
            }
            return this;
        }
    }
}