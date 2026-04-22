using AirMaster.Infrastructure.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace AirMaster.Infrastructure.Net
{
    public class CustomContainer
    {
        public List<Cookie> Cookies { get; private set; }

        public CustomContainer()
        {
            Cookies = new List<Cookie>();
        }

        public void Add(string cookie, string currentDomain = null)
        {
            var type = typeof(System.Net.Cookie).Assembly.GetType("System.Net.CookieParser");
            var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string) }, null);
            var parser = ctor.Invoke(new object[] { cookie });
            var c = type.InvokeMember("Get", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, parser, new object[] { }) as Cookie;
            if (string.IsNullOrWhiteSpace(c.Domain) && currentDomain != null) { c.Domain = currentDomain; }
            Add(c);
        }

        public void Add(Cookie cookie)
        {
            var oldC = Cookies.FirstOrDefault(c => c.Name == cookie.Name && c.Domain == cookie.Domain);
            Cookies.Remove(oldC);
            if (!cookie.Expired)
            {
                Cookies.Add(cookie);
            }
        }

        public void Clear()
        {
            Cookies.Clear();
        }

        public string GetCookieHeader(string url)
        {
            var uri = new Uri(url);
            var q = Cookies.Where(c => !string.IsNullOrWhiteSpace(c.Name) && !c.Expired && uri.PathAndQuery.StartsWith(c.Path) && uri.Host.EndsWith(c.Domain));
            string text = string.Empty;
            string str = string.Empty;
            foreach (Cookie cookie in q)
            {
                text = text + str + cookie.Name + "=" + cookie.Value;
                str = "; ";
            }
            return text;
        }

        public string Save()
        {
            return SerializerFactory.Serialize("json", Cookies);
        }

        public void Load(string json)
        {
            this.Cookies = SerializerFactory.Deserialize<List<Cookie>>("json", json);
        }
    }
}
