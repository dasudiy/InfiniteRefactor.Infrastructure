using System;
using System.Collections.Generic;
using InfiniteRefactor.Infrastructure.Extensions;

namespace InfiniteRefactor.Infrastructure.DataService.Old.WebSocket
{
    public class WebSocketMessage
    {
        public WebSocketMessage(object content, string messageId, string messageType)
        {
            //CreateTime = DateTime.Now;
            Content = content;
            MessageType = messageType;
            MessageId = messageId;
        }

        public virtual object Content { get; set; }
        public string MessageType { get; set; }
        //public DateTime CreateTime { get; set; }
        public string MessageId { get; set; }
        public long Timestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public Dictionary<string, string> Headers { get; set; }
    }

    public class WebSocketMessage<T> : WebSocketMessage
    {
        public WebSocketMessage(T content, string messageId, string messageType)
            : base(content, messageId, messageType)
        {
            Content = content;
        }

        public new T Content
        {
            get => base.Content.To<T>();
            set => base.Content = value;
        }
    }
}
