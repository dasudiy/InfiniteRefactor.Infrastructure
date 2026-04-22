using System.Collections.Concurrent;
using System.Reflection;

namespace AirMaster.Infrastructure.Reflection
{
    public static class MethodHelper
    {
        static ConcurrentDictionary<string, MethodInfo> cache = new ConcurrentDictionary<string, MethodInfo>();

        public static object InvokeMethod(string method, object target, params object[] paramters)
        {
            var type = target.GetType();
            var key = $"{type.FullName}{method}";
            MethodInfo dmh = null;
            if (!cache.TryGetValue(key, out dmh))
            {
                dmh = type.GetMethod(method);// EmitHelper.GetDynamicMethod();
                cache.TryAdd(key, dmh);
            }
            return dmh.Invoke(target, paramters);
        }

        public static object GetValue(string property, object target)
        {
            var type = target.GetType();
            var key = string.Format("get_{0}_{1}", type.FullName, property);
            MethodInfo dmh = null;
            if (!cache.TryGetValue(key, out dmh))
            {
                var pro = type.GetProperty(property);
                if (pro == null) { return null; }
                dmh = pro.GetGetMethod();// EmitHelper.GetDynamicMethod(pro.GetGetMethod());
                cache.TryAdd(key, dmh);
            }
            return dmh.Invoke(target, null);
        }
        public static void SetValue(string property, object target, params object[] paramters)
        {
            var type = target.GetType();
            var key = string.Format("set_{0}_{1}", type.FullName, property);
            MethodInfo dmh = null;
            if (!cache.TryGetValue(key, out dmh))
            {
                var pro = type.GetProperty(property);
                if (pro == null) { return; }
                dmh = pro.GetSetMethod();// EmitHelper.GetDynamicMethod(pro.GetSetMethod());
                cache.TryAdd(key, dmh);
            }
            dmh.Invoke(target, paramters);
        }
    }
}
