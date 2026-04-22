using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.Session
{
    public class CommonSession : ApplicationSession
    {
        private Dictionary<string, object> dict = new Dictionary<string, object>();

        public CommonSession(string sessionId)
            : base(sessionId)
        {
            SessionManager.Instance.NewSession(this);
        }

        public override object this[string key]
        {
            get
            {
                object value = null;
                dict.TryGetValue(key, out value);
                return value;
            }
            set
            {
                dict[key] = value;
            }
        }

        public override Task Abandon()
        {
            SessionManager.Instance.RemoveSession(base.SessionId);
            return Task.CompletedTask;
        }

        public override void Add(string key, object value)
        {
            dict.Add(key, value);
        }

        public override void Clear()
        {
            dict.Clear();
        }

        public override void Remove(string key)
        {
            dict.Remove(key);
        }
    }
}
