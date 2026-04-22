using AirMaster.Infrastructure.Serializer;
using AirMaster.Infrastructure.Session;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Bindings.AspNet
{
    public class AspNetSession : ApplicationSession
    {
        private HttpContext context;

        public AspNetSession(HttpContext context)
            : base(context.Session.Id)
        {
            this.context = context;
        }

        public override object this[string key]
        {
            get
            {
                if (context.Session.Keys.Contains(key))
                {
                    //use this,please add pro in project.
                    //<EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>
                    //return BinarySerializer.Instance.DeserializeFromBytes(context.Session.Get(key), typeof(object));

                    //use this,.net6 unsupport BinaryFormatter
                    var text = System.Text.Encoding.UTF8.GetString(context.Session.Get(key));
                    var splitIndex = text.IndexOf(';');
                    var typename = text.Substring(0, splitIndex);
                    var json = text.Substring(splitIndex + 1);
                    return SerializerFactory.Deserialize("json", json, Type.GetType(typename));
                }
                return null;
            }
            set
            {
                Add(key, value);
            }
        }

        public override async Task Abandon()
        {
            context.Session.Clear();
            await context.SignOutAsync();
            //AuthenticationHttpContextExtensions.SignOutAsync(context);
        }

        public override void Add(string key, object value)
        {
            if (value == null) { return; }
            //use this,please add pro in project.
            //<EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>
            //context.Session.Set(key, BinarySerializer.Instance.SerializeToBytes(value));

            //use this,.net6 unsupport BinaryFormatter
            context.Session.Set(key, System.Text.Encoding.UTF8.GetBytes($"{value.GetType().AssemblyQualifiedName};{SerializerFactory.Serialize("json", value)}"));
        }

        public override void Clear()
        {
            context.Session.Clear();
        }

        public override void Remove(string key)
        {
            context.Session.Remove(key);
        }

        public override IUser User
        {
            get
            {
                var user = this[ApplicationSession.USER_SESSION_KEY] as IUser;
                if (user == null || !context.User.Identity.IsAuthenticated)
                {
                    if (context.User.Identity != null && context.User.Identity.IsAuthenticated && AspNetSession.GetUserByUsernameFn != null)
                    {
                        try
                        {
                            base.User = GetUserByUsernameFn(context.User.Identity.Name);
                            return base.User;
                        }
                        catch
                        {
                            //context.Session.Clear();
                            return null;
                        }
                    }
                }
                return user;
            }
            set
            {
                const string Issuer = "https://gov.uk";
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, value.Username, ClaimValueTypes.String, Issuer) };
                var userIdentity = new ClaimsIdentity(claims, "UserIdentity");
                var userPrincipal = new ClaimsPrincipal(userIdentity);

                context.SignInAsync(
                    scheme: "Automatic",
                    principal: userPrincipal,
                    properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(120),
                        IsPersistent = false,
                        AllowRefresh = false
                    }).Wait();
                base.User = value;
            }
        }

        public static Func<string, IUser> GetUserByUsernameFn { get; set; }
    }
}