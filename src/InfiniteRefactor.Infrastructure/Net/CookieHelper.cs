using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using InfiniteRefactor.Infrastructure.Extensions;

namespace InfiniteRefactor.Infrastructure.Net
{
    public static class CookieHelper
    {
        public static string Save(this CustomContainer cookieContainer)
        {
            using (var writer = new StringWriter())
            {
                foreach (var c in cookieContainer.Cookies)
                {
                    writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                        c.Domain,
                        c.HttpOnly.ToString().ToUpper(),
                        c.Path,
                        c.Secure.ToString().ToUpper(),
                        c.Expires == DateTime.MinValue ? string.Empty : c.Expires.GetUnixTime().ToString(),
                        c.Name,
                        c.Value);
                }

                return writer.ToString();
            }
        }
        public static void Save(this CustomContainer cookieContainer, string filename)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                foreach (var c in cookieContainer.Cookies)
                {
                    writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                        c.Domain,
                        c.HttpOnly.ToString().ToUpper(),
                        c.Path,
                        c.Secure.ToString().ToUpper(),
                        c.Expires == DateTime.MinValue ? string.Empty : c.Expires.GetUnixTime().ToString(),
                        c.Name,
                        c.Value);
                }
            }
        }

        public static void Save(this CookieContainer cookieContainer, string filename)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                Hashtable table = (Hashtable)cookieContainer.GetType().InvokeMember("m_domainTable",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField |
                                                                    BindingFlags.Instance,
                                                                    null,
                                                                    cookieContainer,
                                                                    new object[] { });

                foreach (var tableKey in table.Keys)
                {
                    String str_tableKey = (string)tableKey;

                    if (str_tableKey[0] == '.')
                    {
                        str_tableKey = str_tableKey.Substring(1);
                    }

                    SortedList list = (SortedList)table[tableKey].GetType().InvokeMember("m_list",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField |
                                                                    BindingFlags.Instance,
                                                                    null,
                                                                    table[tableKey],
                                                                    new object[] { });
                    foreach (var listKey in list.Keys)
                    {
                        var item = list[listKey] as CookieCollection;

                        foreach (Cookie c in item)
                        {
                            writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                c.Domain,
                                c.HttpOnly.ToString().ToUpper(),
                                c.Path,
                                c.Secure.ToString().ToUpper(),
                                c.Expires == DateTime.MinValue ? string.Empty : c.Expires.GetUnixTime().ToString(),
                                c.Name,
                                c.Value);
                        }
                    }
                }
            }
        }

        public static CustomContainer LoadCustomContainer(string filename)
        {
            using (var reader = new StreamReader(filename, Encoding.UTF8))
            {
                string line = null;
                var container = new CustomContainer();

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#")) { continue; }

                    try
                    {
                        var arr = line.Split('\t');
                        var cookie = new Cookie();
                        cookie.Domain = arr[0];
                        cookie.HttpOnly = arr[1].To<bool>(false);
                        cookie.Path = arr[2];
                        cookie.Secure = arr[3].To<bool>(false);
                        var unixTime = arr[4].To<double?>(null);
                        if (unixTime.HasValue)
                        {
                            cookie.Expires = ObjectExtension.FromUnixTime(unixTime.Value);
                        }
                        cookie.Name = arr[5];
                        cookie.Value = arr[6];

                        container.Add(cookie);
                    }
                    catch
                    {
                    }
                }

                return container;
            }
        }

        public static CustomContainer LoadCustomContainerFromString(string input)
        {
            using (var reader = new StringReader(input))
            {
                string line = null;
                var container = new CustomContainer();

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#")) { continue; }

                    try
                    {
                        var arr = line.Split('\t');
                        var cookie = new Cookie();
                        cookie.Domain = arr[0];
                        cookie.HttpOnly = arr[1].To<bool>(false);
                        cookie.Path = arr[2];
                        cookie.Secure = arr[3].To<bool>(false);
                        var unixTime = arr[4].To<double?>(null);
                        if (unixTime.HasValue)
                        {
                            try
                            {
                                cookie.Expires = ObjectExtension.FromUnixTime(unixTime.Value);
                            }
                            catch (Exception)
                            {
                                cookie.Expires = DateTime.MaxValue;
                            }
                        }
                        cookie.Name = arr[5];
                        cookie.Value = arr[6];

                        container.Add(cookie);
                    }
                    catch (Exception)
                    {
                    }
                }

                return container;
            }
        }


        public static CookieContainer Load(string filename)
        {
            using (var reader = new StreamReader(filename, Encoding.UTF8))
            {
                string line = null;
                var container = new CookieContainer();

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#")) { continue; }

                    try
                    {
                        var arr = line.Split('\t');
                        var cookie = new Cookie();
                        cookie.Domain = arr[0];
                        cookie.HttpOnly = arr[1].To<bool>(false);
                        cookie.Path = arr[2];
                        cookie.Secure = arr[3].To<bool>(false);
                        var unixTime = arr[4].To<double?>(null);
                        if (unixTime.HasValue)
                        {
                            cookie.Expires = ObjectExtension.FromUnixTime(unixTime.Value);
                        }
                        cookie.Name = arr[5];
                        cookie.Value = arr[6];

                        container.Add(cookie);
                    }
                    catch
                    {
                    }
                }

                return container;
            }
        }
    }
}
