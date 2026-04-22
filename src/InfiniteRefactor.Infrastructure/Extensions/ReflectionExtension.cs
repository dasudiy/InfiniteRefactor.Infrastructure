using AirMaster.Infrastructure.Extensions;
using AirMaster.Infrastructure.Reflection;
using AirMaster.Infrastructure.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AirMaster.Infrastructure.Extensions
{
    public static class ReflectionExtension
    {
        public static T GetCustomAttribute<T>(this Type type, bool inherit = true) where T : Attribute
        {
            return (T)type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }
        public static T GetCustomAttribute<T>(this FieldInfo field, bool inherit = true) where T : Attribute
        {
            return (T)field.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }
        public static T GetCustomAttribute<T>(this MethodInfo method, bool inherit = true) where T : Attribute
        {
            return (T)method.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }

        public static T GetCustomAttribute<T>(this PropertyInfo property, bool inherit = true) where T : Attribute
        {
            return (T)property.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }

        public static T GetCustomAttribute<T>(this ParameterInfo parameter, bool inherit = true) where T : Attribute
        {
            return (T)parameter.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }

        public static T[] GetCustomAttributes<T>(this Type type, bool inherit = true) where T : Attribute
        {
            return (T[])type.GetCustomAttributes(typeof(T), inherit);
        }

        public static T[] GetCustomAttributes<T>(this MethodInfo method, bool inherit = true) where T : Attribute
        {
            return (T[])method.GetCustomAttributes(typeof(T), inherit);
        }

        public static bool IsInherit(this Type type, Type baseType)
        {
            return type.IsSubclassOf(baseType)
                || type.GetInterfaces().Contains(baseType)
                || (baseType.IsGenericType && type.GetInterfaces().Where(c => c.IsGenericType).Select(c => c.GetGenericTypeDefinition()).Contains(baseType));
        }

        #region Copy
        static ConcurrentDictionary<Type, List<PropertyInfo>> CopyTCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        static List<Type> CopyNotNullPrimitiveTypes = new List<Type> { typeof(Int16), typeof(Int64), typeof(byte), typeof(char), typeof(bool), typeof(DateTime), typeof(int), typeof(double), typeof(float), typeof(decimal) };
        static List<PropertyInfo> GetCopyProperty(Type type, bool ignoreAttribute = true)
        {
            List<PropertyInfo> cacthPropertys = null;
            if (!CopyTCache.TryGetValue(type, out cacthPropertys))
            {
                cacthPropertys = new List<PropertyInfo>();

                var ps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty).Where(c => c.CanRead && c.CanWrite).ToArray();
                if (ignoreAttribute)
                {
                    cacthPropertys.AddRange(ps);
                }
                else
                {
                    var tHasCopyAtt = false;
                    if (type.GetCustomAttributes(typeof(CopyAttribute), true).Length > 0) { tHasCopyAtt = true; }

                    foreach (var p in ps)
                    {
                        if (tHasCopyAtt)
                        {
                            if (p.GetCustomAttributes(typeof(CopyIgnoreAttribute), true).Length == 0) { cacthPropertys.Add(p); }
                        }
                        else
                        {
                            if (p.GetCustomAttributes(typeof(CopyAttribute), true).Length > 0) { cacthPropertys.Add(p); }
                        }
                    }
                }
                CopyTCache.TryAdd(type, cacthPropertys);
            }
            return cacthPropertys;
        }
        public static T Copy<T, U>(this Type type, U source, bool ignoreAttribute = true, params string[] excludeProperty)
        {
            List<PropertyInfo> cacthPropertys = GetCopyProperty(type, ignoreAttribute);
            object desInstance = null;
            if (!type.IsClass) { desInstance = Activator.CreateInstance(type); }
            foreach (var p in cacthPropertys)
            {
                if (excludeProperty != null && excludeProperty.Contains(p.Name)) { continue; }
                var sourceValue = EmitHelper.GetValue(p.Name, source);
                if (sourceValue == null && CopyNotNullPrimitiveTypes.Contains(p.PropertyType))
                {
                    continue;
                }
                EmitHelper.SetValue(p.Name, desInstance, sourceValue);
            }
            return (T)desInstance;
        }
        public static void Copy<T, U>(this T des, U source, bool ignoreAttribute = true, params string[] excludeProperty)
        {
            var type = des.GetType();
            List<PropertyInfo> cacthPropertys = GetCopyProperty(type, ignoreAttribute);
            foreach (var p in cacthPropertys)
            {
                if (excludeProperty != null && excludeProperty.Contains(p.Name)) { continue; }
                var sourceValue = EmitHelper.GetValue(p.Name, source);
                var desValue = EmitHelper.GetValue(p.Name, des);
                if(sourceValue.SafeEquals(desValue)) { continue; }
                if (sourceValue == null && CopyNotNullPrimitiveTypes.Contains(p.PropertyType))
                {
                    continue;
                }
                EmitHelper.SetValue(p.Name, des, sourceValue);
            }
        }

        public static object CopyConvert(this object des, Type descType, bool ignoreAttribute = true, params string[] excludeProperty)
        {
            if (des == null) { return null; }
            var u = Activator.CreateInstance(descType);
            u.Copy(des, ignoreAttribute, excludeProperty);
            return u;
        }
        public static U CopyConvert<U>(this object des, bool ignoreAttribute = true, params string[] excludeProperty) where U : class, new()
        {
            if (des == null) { return null; }
            var u = new U { };
            u.Copy(des, ignoreAttribute, excludeProperty);
            return u;
        }
        public static List<U> CopyConvertList<U>(this IList list, bool ignoreAttribute = true, params string[] excludeProperty) where U : class, new()
        {
            var listU = new List<U>();
            foreach (var c in list)
            {
                listU.Add(c.CopyConvert<U>(ignoreAttribute, excludeProperty));
            }
            return listU;
        }
        #endregion

        static List<Type> PrimitiveTypes = new List<Type> {
            typeof(byte), typeof(byte?),
            typeof(sbyte), typeof(sbyte?),
            typeof(short), typeof(short?),
            typeof(ushort), typeof(ushort?),
            typeof(int), typeof(int?),
            typeof(uint), typeof(uint?),
            typeof(long), typeof(long?),
            typeof(ulong), typeof(ulong?),
            typeof(decimal), typeof(decimal?),
            typeof(char), typeof(char?),
            typeof(bool), typeof(bool?),
            typeof(float), typeof(float?),
            typeof(double), typeof(double?),
            typeof(bool), typeof(bool?),
            typeof(DateTime), typeof(DateTime?),
            typeof(DateTimeOffset), typeof(DateTimeOffset?),
            typeof(Guid), typeof(Guid?),typeof(string),
            typeof(byte[])
        };

        public static bool IsPrimitiveType(this Type type)
        {
            return PrimitiveTypes.Contains(type);
        }

    }
}
