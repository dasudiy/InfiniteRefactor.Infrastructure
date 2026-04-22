using System.Collections.Generic;

namespace AirMaster.Infrastructure.DataService.Internal
{
    public class DataServiceResult
    {
        public bool success { get; set; }

        public Dictionary<string, object> output { get; set; }

        public object result { get; set; }
        public string message { get; set; }
        public Dictionary<string, object> errors { get; set; }
    }

    public class DataServiceResult<T>
    {
        public bool success { get; set; }

        public Dictionary<string, object> output { get; set; }

        public T result { get; set; }
        public string message { get; set; }

        public Dictionary<string, object> errors { get; set; }
    }
}
