namespace AirMaster.Infrastructure.DataService.Bindings.Http
{
    //public class DataServiceHttpSession : ApplicationSession
    //{
    //    public const string COOKIE_KEY = "DS_COOKIE";
    //    private string _sessionId;
    //    private Dictionary<string, object> _dict;

    //    internal DataServiceHttpSession(string sessionId) : base(sessionId)
    //    {
    //        var cachePolicy = new CacheItemPolicy();
    //        cachePolicy.SlidingExpiration = new TimeSpan(0, 30, 0);

    //        _dict = MemoryCache.Default.AddOrGetExisting(sessionId,
    //            new Dictionary<string, object>(),
    //                cachePolicy) as Dictionary<string, object>;
    //    }

    //    public override object this[string key]
    //    {
    //        get
    //        {
    //            return _dict[key];
    //        }
    //        set
    //        {
    //            _dict[key] = value;
    //        }
    //    }

    //    public override void Add(string key, object value)
    //    {
    //        _dict.Add(key, value);
    //    }

    //    public override void Remove(string key)
    //    {
    //        _dict.Remove(key);
    //    }

    //    public override void Abandon()
    //    {
    //        this.Clear();
    //        MemoryCache.Default.Remove("Session-" + this.SessionId);
    //    }

    //    public override void Clear()
    //    {
    //        _dict.Clear();
    //    }
    //}
}
