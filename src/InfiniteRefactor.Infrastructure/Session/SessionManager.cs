using System;
using System.Collections.Concurrent;

namespace InfiniteRefactor.Infrastructure.Session
{
    public class SessionManager
    {
        #region Constructor
        public static readonly SessionManager Instance = new SessionManager();

        private SessionManager()
        {
            Sessions = new SessionCollection();
        }

        #endregion

        public SessionCollection Sessions { get; set; }
        public event EventHandler<SessionRemoveEventArgs> SessionRemove;

        public void NewSession(ApplicationSession session)
        {
            if (!Sessions.ContainsKey(session.SessionId))
            {
                Sessions.TryAdd(session.SessionId, session);
            }
        }

        public void RemoveSession(string sessionId)
        {
            if (Sessions.ContainsKey(sessionId))
            {
                SessionRemove?.Invoke(this, new SessionRemoveEventArgs { Session = Sessions[sessionId] });

                Sessions[sessionId].Dispose();
                ApplicationSession session;
                //Sessions.Remove(sessionId, out session);
                Sessions.TryRemove(sessionId, out session);
            }
        }

        public ApplicationSession GetOrCreateSession(string sessionId, Func<string, ApplicationSession> creator)
        {
            return Sessions.ContainsKey(sessionId) ? Sessions[sessionId] : creator(sessionId);
        }
    }

    public class SessionRemoveEventArgs : EventArgs
    {
        public ApplicationSession Session { get; set; }
    }

    public class SessionCollection : ConcurrentDictionary<string, ApplicationSession>
    {
        //protected override string GetKeyForItem(ApplicationSession item)
        //{
        //    return item.SessionId;
        //}
    }
}
