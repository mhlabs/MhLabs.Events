using System.Collections.Generic;
using System.Threading.Tasks;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

namespace MhLabs.Events
{
    public interface IEventsClient
    {
        Task<EventResponse> Send<T>(string detailType, T item);
        Task<EventResponse> Send<T>(string detailType, IEnumerable<T> items);
        Task<EventResponse> Send(string detailType, IList<DynamodbStreamRecord> items);
        Task<EventResponse> Send<T>(string detailType, IEnumerable<EventRequest<T>> items);
    }
}