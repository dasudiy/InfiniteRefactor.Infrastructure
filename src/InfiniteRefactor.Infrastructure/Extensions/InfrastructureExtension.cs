using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using AirMaster.Infrastructure.Serializer;

namespace AirMaster.Infrastructure.Extensions
{
    public static class InfrastructureExtension
    {
        static readonly Regex datetimeReg = new Regex("\"\\\\/Date\\((-?[0-9]+)(?:[a-zA-Z]|(?:\\+|-)[0-9]{4})?\\)\\\\/\\\"", RegexOptions.Compiled);
        public static string ToJson<T>(this T entity)
        {
            if (entity is null) { return null; }
            return SerializerFactory.Serialize("json", entity);
        }
        public static object FromJson(this string json, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(json)) { return default; }
            return SerializerFactory.Deserialize("json", json, targetType);
        }
        public static T FromJson<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json)) { return default; }
            return SerializerFactory.Deserialize<T>("json", json);
        }
        public static string ToXml<T>(this T entity)
        {
            return SerializerFactory.Serialize("xml", entity);
        }
        public static object FromXml(this string xml, Type targetType)
        {
            return SerializerFactory.Deserialize("xml", xml, targetType);
        }
        public static T FromXml<T>(this string xml)
        {
            return SerializerFactory.Deserialize<T>("xml", xml);
        }
        public static void Reset(this byte[] bytes, byte v)
        {
            for (var i = 0; i < bytes.Length; i++) { bytes[i] = 0; }
        }

        #region SumOrDefault of IQueryable
        public static decimal SumOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector)
        {
            return source.Any() ? source.Sum(selector) : default;
        }
        public static decimal SumOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector)
        {
            return source.Any() ? (source.Sum(selector) ?? default) : default;
        }
        public static int SumOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector)
        {
            return source.Any() ? source.Sum(selector) : default;
        }
        public static int SumOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector)
        {
            return source.Any() ? (source.Sum(selector) ?? default) : default;
        }
        public static long SumOrDefault<TSource, U>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector)
        {
            return source.Any() ? source.Sum(selector) : default;
        }
        public static long SumOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector)
        {
            return source.Any() ? (source.Sum(selector) ?? default) : default;
        }
        public static float SumOrDefault<TSource, U>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector)
        {
            return source.Any() ? source.Sum(selector) : default;
        }
        public static float SumOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector)
        {
            return source.Any() ? (source.Sum(selector) ?? default) : default;
        }
        public static double SumOrDefault<TSource, U>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector)
        {
            return source.Any() ? source.Sum(selector) : default;
        }
        public static double SumOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector)
        {
            return source.Any() ? (source.Sum(selector) ?? default) : default;
        }
        #endregion

        #region SumOrDefault of IEnumerable
        public static decimal SumOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
        {
            return source.Any() ? source.Sum(selector) : default;
        }
        public static decimal SumOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
        {
            return source.Any() ? (source.Sum(selector) ?? default) : default;
        }
        public static int SumOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            return source.Any() ? source.Sum(selector) : default;
        }
        public static int SumOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            return source.Any() ? (source.Sum(selector) ?? default) : default;
        }
        public static long SumOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
        {
            return source.Any() ? source.Sum(selector) : default;
        }
        public static long SumOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
        {
            return source.Any() ? (source.Sum(selector) ?? default) : default;
        }
        public static float SumOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            return source.Any() ? source.Sum(selector) : default;
        }
        public static float SumOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
        {
            return source.Any() ? (source.Sum(selector) ?? default) : default;
        }
        public static double SumOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            return source.Any() ? source.Sum(selector) : default;
        }
        public static double SumOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
        {
            return source.Any() ? (source.Sum(selector) ?? 0) : default;
        }
        #endregion

        public static T MinOrDefault<T>(this IEnumerable<T> list)
        {
            return list.Any() ? list.Min() : default;
        }
        public static U MinOrDefault<T, U>(this IEnumerable<T> list, Func<T, U> func)
        {
            return list.Any() ? list.Min(func) : default;
        }
        public static T MaxOrDefault<T>(this IEnumerable<T> list)
        {
            return list.Any() ? list.Max() : default;
        }
        public static U MaxOrDefault<T, U>(this IEnumerable<T> list, Func<T, U> func)
        {
            return list.Any() ? list.Max(func) : default;
        }
        public static string Substr(this string s, int length)
        {
            return s.Substr(0, length);
        }
        public static string Substr(this string s, int start, int length)
        {
            if (string.IsNullOrEmpty(s)) { return string.Empty; }
            return s.Length - start < length ? s.Substring(start) : s.Substring(start, length);
        }
        public static string Neaten(this string s)
        {
            if (string.IsNullOrEmpty(s)) { return s; }
            return s.Trim();
        }
        public static string Neaten(this string s, params char[] trimChars)
        {
            if (string.IsNullOrEmpty(s)) { return s; }
            return s.Trim(trimChars);
        }
        public static string NeatenStart(this string s, params char[] trimChars)
        {
            if (string.IsNullOrEmpty(s)) { return s; }
            return s.TrimStart(trimChars);
        }
        public static string NeatenEnd(this string s, params char[] trimChars)
        {
            if (string.IsNullOrEmpty(s)) { return s; }
            return s.TrimEnd(trimChars);
        }
        public static string RemoveBlank(this string s)
        {
            if (string.IsNullOrEmpty(s)) { return s; }
            return s.Replace(" ", string.Empty);
        }
        //public static bool IsMatch(this string s, string pattern)
        //{
        //    return new Regex(pattern).IsMatch(s);
        //}
        public static Match Match(this string s, string pattern)
        {
            return new Regex(pattern).Match(s);
        }
        public static MatchCollection Matchs(this string s, string pattern)
        {
            return new Regex(pattern).Matches(s);
        }
        //public static T Clone<T>(this T obj)
        //{
        //    if (obj == null) { return default(T); }
        //    BinaryFormatter formatter = new BinaryFormatter();
        //    using (MemoryStream memoryStream = new MemoryStream())
        //    {
        //        formatter.Serialize(memoryStream, obj);
        //        memoryStream.Position = 0;
        //        T newObject = (T)formatter.Deserialize(memoryStream);
        //        return newObject;
        //    }
        //}
        public static int GetQuarter(this DateTime datetime)
        {
            int quarter = 0;
            if (datetime.Month <= 3) { quarter = 1; }
            else if (datetime.Month <= 6) { quarter = 2; }
            else if (datetime.Month <= 9) { quarter = 3; }
            else { quarter = 4; }
            return quarter;
        }
        public static DateTime Tomorrow(this DateTime datetime)
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
        }
        public static DateTime Yesterday(this DateTime datetime)
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-1);
        }
        public static bool In(this DateTime datetime, DateTime start, DateTime end)
        {
            return datetime >= start && datetime <= end;
        }
        public static string FormatDateTime(this string str)
        {
            return datetimeReg.Replace(str, "new Date($1 + (new Date().getTimezoneOffset() * 60000))");
        }
        public static string GetDescription(this DataRow row)
        {
            StringBuilder des = new StringBuilder();
            foreach (DataColumn col in row.Table.Columns)
            {
                des.Append(col.ColumnName + ":" + row[col.ColumnName].ToString() + "|");
            }
            return des.ToString();
        }
        public static Guid GetGuid(this NameValueCollection collection, string name)
        {
            return new Guid(collection[name]);
        }
        public static decimal GetDecimal(this NameValueCollection collection, string name)
        {
            return decimal.Parse(collection[name]);
        }
        public static int GetInt(this NameValueCollection collection, string name)
        {
            return int.Parse(collection[name]);
        }
        public static bool GetBool(this NameValueCollection collection, string name)
        {
            return bool.Parse(collection[name]);
        }
        public static DateTime GetDateTime(this NameValueCollection collection, string name)
        {
            return DateTime.Parse(collection[name]);
        }
        public static void AddRange<T>(this SortedSet<T> source, IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                source.Add(item);
            }
        }
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
        {
            if (source.ContainsKey(key)) { return source[key]; }
            return default(TValue);
        }
        public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value)
        {
            if (source.ContainsKey(key))
            {
                source[key] = value;
            }
            else
            {
                source.Add(key, value);
            }
        }

        public static void Clear<T>(this ConcurrentQueue<T> quene)
        {
            while (quene.Count > 0)
            {
                var t = default(T);
                quene.TryDequeue(out t);
            }
        }
        public static bool IsAnonymousType(this Type type) { return type.Name.StartsWith("<>f__AnonymousType"); }
        public static byte[] GetDecimalBytes(this decimal dec)
        {
            var bits = decimal.GetBits(dec);
            var bytes = new List<byte>();
            foreach (Int32 i in bits)
            {
                bytes.AddRange(BitConverter.GetBytes(i));
            }
            return bytes.ToArray();
        }
        public static void For(this int times, Action<int> callback)
        {
            for (var i = 0; i < times; i++)
            {
                callback(i);
            }
        }
        public static void For(this double times, Action<int> callback)
        {
            for (var i = 0; i < times; i++)
            {
                callback(i);
            }
        }
        public static decimal ToDecimal(this byte[] bytes)
        {
            if (bytes.Count() != 16) { throw new Exception("A decimal must be created from exactly 16 bytes"); }
            var bits = new int[4];
            for (int i = 0; i <= 15; i += 4)
            {
                bits[i / 4] = BitConverter.ToInt32(bytes, i);
            }
            return new decimal(bits);
        }
        //public static byte[] RuntimeSerializeBinary(this object obj)
        //{
        //    return BinarySerializer.Instance.Serialize(obj);
        //}
        //public static T RuntimeDeserializeBinary<T>(this byte[] buffer)
        //{
        //    return BinarySerializer.Instance.Deserialize<T>(buffer);
        //}
        public static string ToTrace(this Exception e)
        {
            if (e == null) { return string.Empty; }
            var logBuilder = new StringBuilder();
            Exception ex = e;
            while (ex != null)
            {
                logBuilder.AppendLine(ex.ToString());
                ex = ex.InnerException;
            }
            return logBuilder.ToString();
        }
        public static string GetInnerMessage(this Exception e)
        {
            if (e == null) { return String.Empty; }
            var message = e.Message;
            for (Exception ex = e; ex != null; ex = ex.InnerException)
            {
                message = ex.ToString();
            }
            return message;
        }
        public static string Join(this IEnumerable<string> s, string separator)
        {
            return string.Join(separator, s);
        }
        public static StringBuilder AppendLineFormat(this StringBuilder s, string format, params object[] args)
        {
            s.AppendFormat(format, args);
            s.AppendLine();
            return s;
        }
        public static byte[] EncodeString(this string text, string encoding)
        {
            return EncodeString(text, Encoding.GetEncoding(encoding));
        }
        public static byte[] EncodeString(this string text, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return encoding.GetBytes(text);
        }
        public static string DecodeString(this byte[] array, string encoding)
        {
            return DecodeString(array, Encoding.GetEncoding(encoding));
        }
        public static string DecodeString(this byte[] array, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return encoding.GetString(array);
        }
        public static byte[] ToBitBytes(this object value)
        {
            byte[] array = null;
            var typeCode = value == null ? TypeCode.Empty : Type.GetTypeCode(value.GetType());
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    array = BitConverter.GetBytes((bool)value);
                    break;
                case TypeCode.Byte:
                    array = new byte[] { (byte)value };
                    break;
                case TypeCode.Char:
                    array = BitConverter.GetBytes((char)value);
                    break;
                case TypeCode.DBNull:
                    break;
                case TypeCode.DateTime:
                    array = BitConverter.GetBytes(((DateTime)value).Ticks);
                    break;
                case TypeCode.Decimal:
                    array = ((decimal)value).GetDecimalBytes();
                    break;
                case TypeCode.Double:
                    array = BitConverter.GetBytes((double)value);
                    break;
                case TypeCode.Empty:
                    break;
                case TypeCode.Int16:
                    array = BitConverter.GetBytes((short)value);
                    break;
                case TypeCode.Int32:
                    array = BitConverter.GetBytes((int)value);
                    break;
                case TypeCode.Int64:
                    array = BitConverter.GetBytes((long)value);
                    break;
                case TypeCode.SByte:
                    array = new byte[] { (byte)value };
                    break;
                case TypeCode.Single:
                    array = BitConverter.GetBytes((float)value);
                    break;
                case TypeCode.UInt16:
                    array = BitConverter.GetBytes((ushort)value);
                    break;
                case TypeCode.UInt32:
                    array = BitConverter.GetBytes((uint)value);
                    break;
                case TypeCode.UInt64:
                    array = BitConverter.GetBytes((ulong)value);
                    break;
                default:
                    break;
            }
            return array;
        }
        public static T FromBitBytes<T>(this byte[] array)
        {
            return (T)FromBitBytes(array, typeof(T));
        }
        public static object FromBitBytes(this byte[] array, Type objectType)
        {
            object value = null;
            switch (Type.GetTypeCode(objectType))
            {
                case TypeCode.Boolean:
                    value = BitConverter.ToBoolean(array, 0);
                    break;
                case TypeCode.Byte:
                    value = array[0];
                    break;
                case TypeCode.Char:
                    value = BitConverter.ToChar(array, 0);
                    break;
                case TypeCode.DBNull:
                    break;
                case TypeCode.DateTime:
                    value = new DateTime(BitConverter.ToInt64(array, 0));
                    break;
                case TypeCode.Decimal:
                    value = array.ToDecimal();
                    break;
                case TypeCode.Double:
                    value = BitConverter.ToDouble(array, 0);
                    break;
                case TypeCode.Empty:
                    break;
                case TypeCode.Int16:
                    value = BitConverter.ToInt16(array, 0);
                    break;
                case TypeCode.Int32:
                    value = BitConverter.ToInt32(array, 0);
                    break;
                case TypeCode.Int64:
                    value = BitConverter.ToInt64(array, 0);
                    break;
                case TypeCode.SByte:
                    value = (sbyte)array[0];
                    break;
                case TypeCode.Single:
                    value = BitConverter.ToSingle(array, 0);
                    break;
                case TypeCode.UInt16:
                    value = BitConverter.ToUInt16(array, 0);
                    break;
                case TypeCode.UInt32:
                    value = BitConverter.ToUInt32(array, 0);
                    break;
                case TypeCode.UInt64:
                    value = BitConverter.ToUInt64(array, 0);
                    break;
                default:
                    break;
            }
            return value;
        }
        public static byte[] ToArray(this Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public static string ToBase64String(this byte[] array)
        {
            return Convert.ToBase64String(array);
        }
        public static byte[] FromBase64String(this string text)
        {
            return Convert.FromBase64String(text);
        }
        public static void PageRun<T>(this IEnumerable<T> list, int everyTake, Action<T> action)
        {
            int num = 0;
            while (true)
            {
                IEnumerable<T> enumerable = list.Skip(num).Take(everyTake);
                if (enumerable.Count() != 0)
                {
                    enumerable.ForEach(action);
                    num += everyTake;
                    continue;
                }

                break;
            }
        }
        public static void PageRunList<T>(this IEnumerable<T> list, int everyTake, Action<IEnumerable<T>> everyTakeAction)
        {
            int num = 0;
            while (true)
            {
                IEnumerable<T> enumerable = list.Skip(num).Take(everyTake);
                if (enumerable.Count() != 0)
                {
                    everyTakeAction(enumerable);
                    num += everyTake;
                    continue;
                }

                break;
            }
        }
        public static void Page(this int totalCount, int everyTake, Action<int, int> action)
        {
            for (int i = 0; i < totalCount; i += everyTake)
            {
                action(i, everyTake);
            }
        }
        public static IEnumerable<T[]> SplitList<T>(this IEnumerable<T> list, int everyTake)
        {
            int pagesize = (int)Math.Ceiling((double)list.Count() / (double)everyTake);
            for (int pageIndex = 0; pageIndex < pagesize; pageIndex++)
            {
                yield return list.Skip(pageIndex * pagesize).Take(everyTake).ToArray();
            }
        }

        private static readonly ConcurrentDictionary<Type, Func<object, object, bool>> _equalsCache = new();
        public static bool SafeEquals(this object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            Type type = a.GetType();
            if (type != b.GetType()) return false;

            var equalsFunc = _equalsCache.GetOrAdd(type, t =>
            {
                // 构建表达式：(x, y) => EqualityComparer<T>.Default.Equals((T)x, (T)y)
                var xParam = Expression.Parameter(typeof(object));
                var yParam = Expression.Parameter(typeof(object));

                var comparerType = typeof(EqualityComparer<>).MakeGenericType(t);
                var defaultProp = comparerType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static);
                var equalsMethod = comparerType.GetMethod("Equals", new[] { t, t });

                var body = Expression.Call(
                    Expression.Property(null, defaultProp),
                    equalsMethod,
                    Expression.Convert(xParam, t),
                    Expression.Convert(yParam, t)
                );

                var lambda = Expression.Lambda<Func<object, object, bool>>(body, xParam, yParam);
                return lambda.Compile();
            });

            return equalsFunc(a, b);
        }

        /// <summary>
        /// 将字节数转换为KB，并格式化为带两位小数的字符串。
        /// </summary>
        /// <param name="bytes">需要转换的字节数。</param>
        /// <returns>格式化后的KB字符串。</returns>
        public static string BytesToKB(this long bytes)
        {
            double kb = bytes / 1024.0;
            return kb.ToString("F2") + " KB";
        }

        /// <summary>
        /// 将字节数转换为KB，并格式化为带两位小数的字符串。
        /// </summary>
        /// <param name="bytes">需要转换的字节数。</param>
        /// <returns>格式化后的KB字符串。</returns>
        public static string BytesToMB(this long bytes)
        {
            double kb = bytes / (1024.0 * 1024.0);
            return kb.ToString("F2") + " MB";
        }

        /// <summary>
        /// 米转换为毫米
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int? ToMM(this double? m)
        {
            return (int?)(m * 1000);
        }
        /// <summary>
        /// 米转换为毫米
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int ToMM(this double m)
        {
            return (int)(m * 1000);
        }
        /// <summary>
        /// 吨转换为公斤
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int? ToKG(this double? t)
        {
            return (int?)(t * 1000);
        }
        /// <summary>
        /// 吨转换为公斤
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int ToKG(this double t)
        {
            return (int)(t * 1000);
        }
    }
}
