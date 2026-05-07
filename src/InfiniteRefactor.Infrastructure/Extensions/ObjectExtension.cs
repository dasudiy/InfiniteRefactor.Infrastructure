using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.Serializer;

namespace InfiniteRefactor.Infrastructure.Extensions
{
    public static class ObjectExtension
    {
        private static Dictionary<Type, Func<object, object>> dict = new Dictionary<Type, Func<object, object>>();


        static ObjectExtension()
        {
            dict.Add(typeof(sbyte), WrapValueConvert(Convert.ToSByte));
            dict.Add(typeof(byte), WrapValueConvert(Convert.ToByte));
            dict.Add(typeof(short), WrapValueConvert(Convert.ToInt16));
            dict.Add(typeof(int), WrapValueConvert(Convert.ToInt32));
            dict.Add(typeof(long), WrapValueConvert(Convert.ToInt64));
            dict.Add(typeof(ushort), WrapValueConvert(Convert.ToUInt16));
            dict.Add(typeof(uint), WrapValueConvert(Convert.ToUInt32));
            dict.Add(typeof(ulong), WrapValueConvert(Convert.ToUInt64));
            dict.Add(typeof(double), WrapValueConvert(Convert.ToDouble));
            dict.Add(typeof(float), WrapValueConvert(Convert.ToSingle));
            dict.Add(typeof(decimal), WrapValueConvert(Convert.ToDecimal));
            dict.Add(typeof(bool), o =>
            {
                if (o == null) { return false; }

                if (o is string)
                {
                    if (Regex.IsMatch((string)o, @"^\d+$"))
                    {
                        return Convert.ToBoolean(Convert.ToInt32(o));
                    }
                }
                return WrapValueConvert(Convert.ToBoolean)(o);
            });
            dict.Add(typeof(Guid), f => new Guid(f.ToString()));
            dict.Add(typeof(DateTime), f => Convert.ToDateTime(f));

            dict.Add(typeof(sbyte?), (o) => { return !o.IsSByte() ? null : WrapValueConvert(Convert.ToSByte)(o); });
            dict.Add(typeof(byte?), (o) => { return !o.IsByte() ? null : WrapValueConvert(Convert.ToByte)(o); });
            dict.Add(typeof(short?), (o) => { return !o.IsShort() ? null : WrapValueConvert(Convert.ToInt16)(o); });
            dict.Add(typeof(int?), (o) => { return !o.IsInt() ? null : WrapValueConvert(Convert.ToInt32)(o); });
            dict.Add(typeof(long?), (o) => { return !o.IsLong() ? null : WrapValueConvert(Convert.ToInt64)(o); });
            dict.Add(typeof(ushort?), (o) => { return !o.IsUShort() ? null : WrapValueConvert(Convert.ToUInt16)(o); });
            dict.Add(typeof(uint?), (o) => { return !o.IsUInt() ? null : WrapValueConvert(Convert.ToUInt32)(o); });
            dict.Add(typeof(ulong?), (o) => { return !o.IsULong() ? null : WrapValueConvert(Convert.ToUInt64)(o); });
            dict.Add(typeof(double?), (o) => { return !o.IsDouble() ? null : WrapValueConvert(Convert.ToDouble)(o); });
            dict.Add(typeof(float?), (o) => { return !o.IsFloat() ? null : WrapValueConvert(Convert.ToSingle)(o); });
            dict.Add(typeof(decimal?), (o) => { return !o.IsDecimal() ? null : WrapValueConvert(Convert.ToDecimal)(o); });
            dict.Add(typeof(bool?), o =>
            {
                if (o == null) { return false; }

                if (o is string)
                {
                    if (Regex.IsMatch((string)o, @"^\d+$"))
                    {
                        return Convert.ToBoolean(Convert.ToInt32(o));
                    }
                }
                return WrapValueConvert(Convert.ToBoolean)(o);
            });
            //dict.Add(typeof(bool?), (o) => { return !o.IsBool() ? null : WrapValueConvert(Convert.ToBoolean)(o); });
            dict.Add(typeof(Guid?), (o) => { if (!o.IsGuid()) { return null; } return new Guid(o.ToString()); });
            dict.Add(typeof(DateTime?), (o) => { if (!o.IsDateTime()) { return null; } return Convert.ToDateTime(o); });
            dict.Add(typeof(string), Convert.ToString);
            dict.Add(typeof(DateOnly), o => DateOnly.FromDateTime(Convert.ToDateTime(o)));
        }

        private static Func<object, object> WrapValueConvert<T>(Func<object, T> input) where T : struct
        {
            return i =>
            {
                if (i == null || i is DBNull) { return null; }
                return input(i);
            };
        }

        public static bool CanConvertTo(this object obj, Type targetType)
        {
            return dict.ContainsKey(targetType);
        }

        public static T To<T>(this object obj)
        {
            return (T)To(obj, typeof(T));
        }

        public static T To<T>(this object obj, T defaultValue)
        {
            try
            {
                object value;
                if (TryConvertTo(obj, typeof(T), out value))
                {
                    return (T)value;
                }
                else
                {
                    return defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool TryConvertTo<T>(this object obj, out T value)
        {
            object v;
            var ret = TryConvertTo(obj, typeof(T), out v);
            if (ret)
            {
                value = (T)v;
            }
            else
            {
                value = default(T);
            }
            return ret;
        }

        public static bool TryConvertTo(this object obj, Type targetType, out object value)
        {
            value = null;
            //不要注释了，针对nullable类型，比如:decimal?, int?等，当值未null时
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (obj == null)
                {
                    value = null;
                    return true;
                }
                else
                {
                    return TryConvertTo(obj, targetType.GetGenericArguments().First(), out value);
                }
            }


            if (obj != null)
            {
                try
                {
                    if (obj.GetType() == targetType || targetType.IsAssignableFrom(obj.GetType()))
                    {
                        value = obj;
                    }
                    else if (obj.GetType() == typeof(Newtonsoft.Json.Linq.JObject) || obj.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                    {
                        //Newtonsoft.Json.Linq.JObject jobj = (Newtonsoft.Json.Linq.JObject)obj;
                        value = obj.GetType().GetMethod("ToObject", new Type[] { }).MakeGenericMethod(targetType).Invoke(obj, null);
                    }
                    else if (obj is JsonDocument)
                    {
                        JsonDocument o = (JsonDocument)obj;
                        value = o.Deserialize(targetType, new JsonSerializerOptions
                        {
                            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                        });
                    }
                    else if (obj is JsonElement)
                    {
                        JsonElement o = (JsonElement)obj;
                        if (o.ValueKind == JsonValueKind.Object)
                        {
                            value = o.Deserialize(targetType, SystemJsonSerializer.Instance.Options);
                        }
                        else
                        {
                            value = SystemJsonSerializer.Instance.Deserialize(o.GetRawText(), targetType);
                        }
                    }
                    else if (dict.ContainsKey(targetType))
                    {
                        value = dict[targetType](obj);
                    }
                    else if (targetType.IsEnum)
                    {
                        value = Enum.Parse(targetType, obj.ToString(), true);
                    }
                    else if (targetType.IsArray && obj is string)
                    {
                        if (obj.ToString().StartsWith("[") && obj.ToString().EndsWith("]")) { return false; }
                        var elementType = targetType.GetElementType();
                        var items = obj.ToString().Split(',');
                        var target = Array.CreateInstance(elementType, items.Length);
                        for (var i = 0; i < items.Length; i++)
                        {
                            target.SetValue(To(items[i], elementType), i);
                        }

                        value = target;
                    }
                    else
                    {
                        value = Convert.ChangeType(obj, targetType);
                    }
                    return true;
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                if (!targetType.IsValueType)
                {
                    value = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public static object To(this object obj, Type targetType)
        {
            object value;
            if (TryConvertTo(obj, targetType, out value))
            {
                return value;
            }
            else
            {
                throw new NotImplementedException($"无法将{obj?.ToString()}({obj?.GetType().Name})转换为{targetType.Name}");
            }
        }


        private static Dictionary<Type, PropertyInfo[]> cache = new Dictionary<Type, PropertyInfo[]>();
        public static IEnumerable<PropertyInfo> GetPropertiesWithCache(this Type type)
        {
            if (!cache.ContainsKey(type))
            {
                lock (cache)
                {
                    if (!cache.ContainsKey(type))
                    {
                        cache[type] = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.GetProperty);
                    }
                }
            }
            return cache[type];
        }

        public static PropertyInfo GetPropertyWithCache(this Type type, string name)
        {
            if (!cache.ContainsKey(type))
            {
                lock (cache)
                {
                    if (!cache.ContainsKey(type))
                    {
                        cache[type] = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.GetProperty);
                    }
                }
            }

            return cache[type].FirstOrDefault(p => p.Name == name);
        }


        public static T ApplyObject<T>(this T target, object source, bool ignoreCase = false)
        {
            object wrapper = target; // add this for value types like struct
            foreach (var prop in source.GetType().GetPropertiesWithCache().Where(p => p.CanRead))
            {
                var targetProp = wrapper.GetType().GetProperty(prop.Name, ignoreCase ? (BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public) : (BindingFlags.Instance | BindingFlags.Public));
                if (targetProp != null && targetProp.CanWrite)
                {
                    try
                    {
                        targetProp.SetValue(wrapper, prop.GetValue(source, null).To(targetProp.PropertyType), null);
                    }
                    catch
                    {
                    }
                }
            }
            return (T)wrapper;
        }

        public static T ApplyObjectFields<T>(this T target, object source, bool ignoreCase = false)
        {
            object wrapper = target; // add this for value types like struct
            foreach (var member in source.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public).Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property))
            {
                try
                {
                    if (member.MemberType == MemberTypes.Property)
                    {
                        var prop = member as PropertyInfo;
                        var targetField = wrapper.GetType().GetField(prop.Name, ignoreCase ? (BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public) : (BindingFlags.Instance | BindingFlags.Public));
                        targetField?.SetValue(wrapper, prop.GetValue(source).To(targetField.FieldType));


                        var targetProp = wrapper.GetType().GetProperty(prop.Name, ignoreCase ? (BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public) : (BindingFlags.Instance | BindingFlags.Public));
                        targetProp?.SetValue(wrapper, prop.GetValue(source).To(targetProp.PropertyType), null);
                    }
                    else if (member.MemberType == MemberTypes.Field)
                    {
                        var field = member as FieldInfo;
                        var targetField = wrapper.GetType().GetField(field.Name, ignoreCase ? (BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public) : (BindingFlags.Instance | BindingFlags.Public));
                        targetField?.SetValue(wrapper, field.GetValue(source).To(targetField.FieldType));

                        var targetProp = wrapper.GetType().GetProperty(field.Name, ignoreCase ? (BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public) : (BindingFlags.Instance | BindingFlags.Public));
                        targetProp?.SetValue(wrapper, field.GetValue(source).To(targetProp.PropertyType), null);
                    }
                }
                catch
                {
                }
            }
            return (T)wrapper;
        }

        public static T ApplyObjectWithSelectedProperty<T>(this T target, object source, Expression<Func<T, object>> selector)
        {
            if (selector.Body.NodeType == ExpressionType.New)
            {
                var expression = selector.Body as NewExpression;

                var fields = expression.Arguments.Where(a => a.NodeType == ExpressionType.MemberAccess)
                    .Select(a => (a as MemberExpression).Member.Name);

                ApplyObjectWithSelectedProperty(target, source, fields);
                return target;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public static T ApplyObjectWithSelectedProperty<T>(this T target, object source, IEnumerable<string> selectedFields)
        {
            object wrapper = target; // add this for value types like struct
            foreach (var prop in source.GetType().GetPropertiesWithCache().Where(p => p.CanRead && selectedFields.Contains(p.Name)))
            {
                var targetProp = wrapper.GetType().GetPropertyWithCache(prop.Name);
                if (targetProp != null && targetProp.CanWrite)
                {
                    try
                    {
                        targetProp.SetValue(wrapper, prop.GetValue(source, null).To(targetProp.PropertyType), null);
                    }
                    catch
                    {
                    }
                }
            }
            return (T)wrapper;
        }

        public static T ApplyObjectSkipSelectedProperty<T>(this T target, object source, Expression<Func<T, object>> selector)
        {
            if (selector.Body.NodeType == ExpressionType.New)
            {
                var expression = selector.Body as NewExpression;

                var fields = expression.Arguments.Where(a => a.NodeType == ExpressionType.MemberAccess)
                    .Select(a => (a as MemberExpression).Member.Name);


                ApplyObjectSkipSelectedProperty<T>(target, source, fields);
                return target;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public static T ApplyObjectSkipSelectedProperty<T>(this T target, object source, IEnumerable<string> selectedFields)
        {
            object wrapper = target; // add this for value types like struct
            foreach (var prop in source.GetType().GetPropertiesWithCache().Where(p => p.CanRead && !selectedFields.Contains(p.Name)))
            {
                var targetProp = wrapper.GetType().GetPropertyWithCache(prop.Name);
                if (targetProp != null && targetProp.CanWrite)
                {
                    try
                    {
                        targetProp.SetValue(wrapper, prop.GetValue(source, null).To(targetProp.PropertyType), null);
                    }
                    catch
                    {
                    }
                }
            }
            return (T)wrapper;
        }

        public static T ApplyDictionary<T>(this T target, IDictionary<string, object> source)
        {
            object wrapper = target; // add this for value type like struct
            foreach (var prop in source)
            {
                var targetProp = wrapper.GetType().GetPropertyWithCache(prop.Key);
                if (targetProp != null && targetProp.CanWrite)
                {
                    try
                    {
                        targetProp.SetValue(wrapper, prop.Value.To(targetProp.PropertyType), null);
                    }
                    catch
                    {
                    }
                }
            }
            return (T)wrapper;
        }

        public static T ApplyCollection<T>(this T target, NameValueCollection source)
        {
            object wrapper = target; // add this for value types like struct
            foreach (var key in source.AllKeys)
            {
                var targetProp = wrapper.GetType().GetPropertyWithCache(key);
                if (targetProp != null && targetProp.CanWrite)
                {
                    try
                    {
                        targetProp.SetValue(wrapper, source[key].To(targetProp.PropertyType), null);
                    }
                    catch
                    {
                    }
                }
            }
            return (T)wrapper;
        }

        public static bool AllPropertyEqual<T>(this T source, T target) where T : class
        {
            bool equal = true;
            foreach (var prop in typeof(T).GetPropertiesWithCache().Where(p => p.CanRead))
            {
                equal &= prop.GetValue(source, null).Equals(prop.GetValue(target, null));
                if (!equal) { break; }
            }
            return equal;
        }

        /// <summary>
        /// Extension method that turns a dictionary of string and object to an ExpandoObject
        /// </summary>
        public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
        {
            var expando = new ExpandoObject();
            var expandoDic = (IDictionary<string, object>)expando;

            // go through the items in the dictionary and copy over the key value pairs)
            foreach (var kvp in dictionary)
            {
                // if the value can also be turned into an ExpandoObject, then do it!
                if (kvp.Value is IDictionary<string, object>)
                {
                    var expandoValue = ((IDictionary<string, object>)kvp.Value).ToExpando();
                    expandoDic.Add(kvp.Key, expandoValue);
                }
                else if (kvp.Value is ICollection)
                {
                    // iterate through the collection and convert any strin-object dictionaries
                    // along the way into expando objects
                    var itemList = new List<object>();
                    foreach (var item in (ICollection)kvp.Value)
                    {
                        if (item is IDictionary<string, object>)
                        {
                            var expandoItem = ((IDictionary<string, object>)item).ToExpando();
                            itemList.Add(expandoItem);
                        }
                        else
                        {
                            itemList.Add(item);
                        }
                    }

                    expandoDic.Add(kvp.Key, itemList);
                }
                else
                {
                    expandoDic.Add(kvp);
                }
            }

            return expando;
        }

        /// <summary>
        /// above .net6,use this,please add pro in project.
        /// <EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Obsolete]
        public static T Clone<T>(this T obj)
        {
            if (obj == null) { return default(T); }
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, obj);
                memoryStream.Position = 0;
                T newObject = (T)formatter.Deserialize(memoryStream);
                return newObject;
            }
        }

        public static double GetUnixTime(this DateTime dateTime)
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (dateTime - epochStart).TotalSeconds;
        }

        public static DateTime FromUnixTime(double dateTime)
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epochStart.AddSeconds(dateTime);
        }

        public static double GetUnixTime(this DateTimeOffset dateTime)
        {
            var epochStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, dateTime.Offset);
            return (dateTime - epochStart).TotalSeconds;
        }

        public static DateTimeOffset FromUnixTime(double dateTime, TimeSpan offset)
        {
            var epochStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, offset);
            return epochStart.AddSeconds(dateTime);
        }

        public static bool In<T>(this T item, params T[] arr)
        {
            return arr.Contains(item);
        }

        public static async Task<int> ReadAsyncAtLeast(this Stream stream, byte[] buffer, int offset, int count, int minCount, CancellationToken cancellationToken)
        {
            int read = 0;
            do
            {
                var currentRread = await stream.ReadAsync(buffer, offset + read, count - read, cancellationToken).ConfigureAwait(false);
                if (currentRread <= 0) { throw new EndOfStreamException(); }
                read += currentRread;
            } while (read < minCount);
            return read;
        }

        public static async Task<byte[]> ReadBytesAsync(this Stream stream, int count, CancellationToken cancellationToken)
        {
            if (count == 0)
            {
                return new byte[] { };
            }
            byte[] array = new byte[count];
            int num = 0;
            do
            {
                int num2 = await stream.ReadAsync(array, num, count).ConfigureAwait(false);
                if (num2 == 0)
                {
                    break;
                }
                num += num2;
                count -= num2;
            }
            while (count > 0);
            return array;
        }

        private static Dictionary<string, MethodInfo> MethodInfoCache = new Dictionary<string, MethodInfo>();
        public static MethodInfo GetMethodWithCache(this Type type, string name)
        {
            var key = $"{type.FullName}{name}";
            if (!MethodInfoCache.ContainsKey(key))
            {
                lock (MethodInfoCache)
                {
                    if (!MethodInfoCache.ContainsKey(key))
                    {
                        MethodInfoCache.Add(key, type.GetMethod(name));
                    }
                }
            }
            return MethodInfoCache[key];
        }
        public static MethodInfo GetMethod(this Type type, string name, Type[] types, bool includingBase)
        {
            var methodInfo = type.GetMethod(name, types);
            if (methodInfo == null && includingBase)
            {
                if (type.BaseType != null)
                {
                    methodInfo = type.BaseType.GetMethod(name, types, true);
                }

                if (methodInfo == null)
                {
                    foreach (var item in type.GetInterfaces())
                    {
                        methodInfo = item.GetMethod(name, types, true);
                        if (methodInfo != null) { break; }
                    }
                }
            }
            return methodInfo;
        }

        public static MethodInfo[] GetMethods(this Type type, BindingFlags flags, bool includingBase)
        {
            var methodInfos = type.GetMethods(flags).ToList();
            if (includingBase)
            {
                if (type.BaseType != null)
                {
                    methodInfos.AddRange(type.BaseType.GetMethods(flags, true));
                }

                foreach (var item in type.GetInterfaces())
                {
                    methodInfos.AddRange(item.GetMethods(flags, true));
                }
            }
            return methodInfos.ToArray();
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> col, int size)
        {
            List<IEnumerable<T>> result = new List<IEnumerable<T>>();
            for (int i = 0; i < Math.Ceiling(col.Count().To<double>() / size.To<double>()); i++)
            {
                yield return col.Skip(i * size).Take(size);
            }
        }

        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> instance, TKey key)
        {
            TValue v = default(TValue);
            var success = instance.TryGetValue(key, out v);
            return v;
        }
    }
}
