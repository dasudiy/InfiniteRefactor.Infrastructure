using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using InfiniteRefactor.Infrastructure.Extensions;

namespace InfiniteRefactor.Infrastructure.Net
{
    public static class HttpUtil
    {
        // System.Web.HttpValueCollection
        public static NameValueCollection LoadForm(byte[] bytes, Encoding encoding)
        {
            var form = new NameValueCollection();
            int num = (bytes != null) ? bytes.Length : 0;
            for (int i = 0; i < num; i++)
            {
                //this.ThrowIfMaxHttpCollectionKeysExceeded();
                int num2 = i;
                int num3 = -1;
                while (i < num)
                {
                    byte b = bytes[i];
                    if (b == 61)
                    {
                        if (num3 < 0)
                        {
                            num3 = i;
                        }
                    }
                    else
                    {
                        if (b == 38)
                        {
                            break;
                        }
                    }
                    i++;
                }
                string name;
                string value;
                if (num3 >= 0)
                {
                    name = HttpUtility.UrlDecode(bytes, num2, num3 - num2, encoding).Trim(new char[] { '\uFEFF', '\u200B' });
                    value = HttpUtility.UrlDecode(bytes, num3 + 1, i - num3 - 1, encoding);
                }
                else
                {
                    name = null;
                    value = HttpUtility.UrlDecode(bytes, num2, i - num2, encoding);
                }
                form.Add(name, value);
                if (i == num - 1 && bytes[i] == 38)
                {
                    form.Add(null, string.Empty);
                }
            }
            return form;
        }
        public static string ToQueryString(IDictionary<string, object> dict, Encoding encoding = null)
        {
            if (dict != null)
            {
                var array = (from i in dict
                             select string.Format("{0}={1}", HttpUtility.UrlEncode(i.Key, encoding ?? Encoding.UTF8), HttpUtility.UrlEncode(i.Value.To<string>(string.Empty), encoding ?? Encoding.UTF8)))
                    .ToArray();
                return string.Join("&", array);
            }
            else
            {
                return string.Empty;
            }
        }

        public static string ToQueryString(NameValueCollection collection, Encoding encoding = null)
        {
            if (collection != null)
            {
                var list = new List<string>();
                for (int i = 0; i < collection.Count; i++)
                {
                    foreach (var item in collection.GetValues(i))
                    {
                        list.Add(string.Format("{0}={1}", HttpUtility.UrlEncode(collection.Keys[i], encoding ?? Encoding.UTF8), HttpUtility.UrlEncode(item, encoding ?? Encoding.UTF8)));
                    }
                }
                return string.Join("&", list);
            }
            else
            {
                return string.Empty;
            }
        }

        //public static string UrlDecode(string str, Encoding encoding = null)
        //{
        //    return HttpUtility.UrlDecode(str, encoding ?? Encoding.UTF8);
        //}

        //public static string UrlEncode(string str, Encoding encoding = null)
        //{
        //    return HttpUtility.UrlEncode(str, encoding ?? Encoding.UTF8);
        //}
    }
}
