//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Principal;
//using System.Text;
//using System.Web;

//namespace AirMaster.Infrastructure.Session
//{
//    public class AspNetSession : ApplicationSession
//    {
//        private HttpContext context;

//        public static AspNetSession Current
//        {
//            get
//            {
//                return new AspNetSession(HttpContext.Current);
//            }
//        }

//        public AspNetSession(HttpContext context)
//            : base(context.Session.SessionID)
//        {
//            this.context = context;
//        }

//        public AspNetSession() :
//            base(HttpContext.Current.Session.SessionID)
//        {
//            this.context = HttpContext.Current;
//        }

//        public override object this[string key]
//        {
//            get
//            {
//                return context.Session[key];
//            }
//            set
//            {
//                context.Session[key] = value;
//            }
//        }

//        public override void Add(string key, object value)
//        {
//            context.Session.Add(key, value);
//        }

//        public override void Remove(string key)
//        {
//            context.Session.Remove(key);
//        }

//        public override void Abandon()
//        {
//            context.Session.Abandon();
//        }

//        public override void Clear()
//        {
//            context.Session.Clear();
//        }

//        public override IUser User
//        {
//            get
//            {
//                if (base.User == null || !context.User.Identity.IsAuthenticated)
//                {
//                    if (context.User.Identity.IsAuthenticated && context.User.Identity != null && AspNetSession.GetUserByUsernameFn != null)
//                    {
//                        try
//                        {
//                            var u = GetUserByUsernameFn(context.User.Identity.Name);
//                            base.User = u;
//                            return u;
//                        }
//                        catch (Exception)
//                        {
//                            context.Session.Abandon();
//                            FormsAuthentication.SignOut();
//                            return null;
//                        }
//                    }
//                    else
//                    {
//                        return null;
//                    }
//                }
//                else
//                {
//                    return base.User;
//                }
//            }
//            set
//            {
//                if (value == null)
//                {
//                    context.Session.Abandon();
//                    FormsAuthentication.SignOut();
//                }
//                else
//                {
//                    try
//                    {
//                        FormsAuthentication.SetAuthCookie(value.Username, false);
//                    }
//                    catch
//                    {
//                        context.Response.Cookies.Add(FormsAuthentication.GetAuthCookie(value.Username, false));
//                    }
//                }
//                base.User = value;
//            }
//        }

//        public static Func<string, IUser> GetUserByUsernameFn { get; set; }
//    }
//}
