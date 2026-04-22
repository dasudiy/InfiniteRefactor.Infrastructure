using System;
using System.Net;

namespace AirMaster.Infrastructure.DataService
{
    public class DataServiceException : Exception
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;
        public string DataServiceSource { get; set; }
        public string ErrorMessage { get; set; }
        
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public override string Message => ErrorMessage;

        public DataServiceException()
        {

        }

        public DataServiceException(string message) : base(message)
        {
            ErrorMessage = message;
        }

        public DataServiceException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorMessage = message;
        }
    }
}
