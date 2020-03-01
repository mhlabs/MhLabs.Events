using System.Collections.Generic;

namespace MhLabs.Events
{
    public class EventResponse
    {
        public int FailedCount { get; set; }
        public List<string> FailedReasons { get; set; }
    }
}