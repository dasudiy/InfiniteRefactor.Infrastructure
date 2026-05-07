using System;
using System.Threading.Tasks;

namespace InfiniteRefactor.Infrastructure.Session
{
    public abstract class ApplicationSession : IDisposable
    {
        protected const string USER_SESSION_KEY = "user";
        private const string USER_SESSION_NAME_KEY = "username";
        private object _syncRoot = new object();

        public ApplicationSession(string sessionId)
        {
            this.SessionId = sessionId;
            CreatedTime = DateTime.Now;
        }

        public string SessionId { get; set; }

        public virtual IUser User
        {
            get { return this[USER_SESSION_KEY] as IUser; }
            set
            {
                this[USER_SESSION_KEY] = value;
                if (value != null)
                {
                    this[USER_SESSION_NAME_KEY] = value.Username;
                }
                else
                {
                    this[USER_SESSION_NAME_KEY] = null;
                }
            }
        }

        public abstract object this[string key] { get; set; }

        public abstract void Add(string key, object value);

        public abstract void Remove(string key);

        public abstract Task Abandon();

        public abstract void Clear();

        public DateTime CreatedTime { get; set; }

        public virtual void Dispose()
        {
            //_waitHandler.Dispose();
        }
    }
}
