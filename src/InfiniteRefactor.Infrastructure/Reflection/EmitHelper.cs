using AirMaster.Infrastructure.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AirMaster.Infrastructure.Reflection
{
    public static class EmitHelper
    {
        #region full emit Constructor and property getter/setter
        private static readonly ConcurrentDictionary<ConstructorInfo, Func<object[], object>> _ctorCache = new();
        public static T CreateInstance<T>(ConstructorInfo ctor, object[] args)
        {
            return (T)CreateInstance(ctor, args);
        }
        public static object CreateInstance(ConstructorInfo ctor, object[] args)
        {
            var func = _ctorCache.GetOrAdd(ctor, CreateCtorDelegate);
            return func(args);
        }

        private static Func<object[], object> CreateCtorDelegate(ConstructorInfo ctor)
        {
            var dm = new System.Reflection.Emit.DynamicMethod(
                "CtorInvoker", typeof(object), new[] { typeof(object[]) }, true);
            var il = dm.GetILGenerator();
            var parameters = ctor.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, i);
                il.Emit(System.Reflection.Emit.OpCodes.Ldelem_Ref);

                var paramType = parameters[i].ParameterType;
                if (paramType.IsValueType)
                    il.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, paramType);
                else
                    il.Emit(System.Reflection.Emit.OpCodes.Castclass, paramType);
            }
            il.Emit(System.Reflection.Emit.OpCodes.Newobj, ctor);
            if (ctor.DeclaringType.IsValueType)
                il.Emit(System.Reflection.Emit.OpCodes.Box, ctor.DeclaringType);
            il.Emit(System.Reflection.Emit.OpCodes.Ret);

            return (Func<object[], object>)dm.CreateDelegate(typeof(Func<object[], object>));
        }

        private static readonly ConcurrentDictionary<(Type, string), Action<object, object>> _setterCache = new();
        public static void SetValue(string propertyName, object instance, object value, Type type = null)
        {
            type ??= instance.GetType();
            var setter = _setterCache.GetOrAdd((type, propertyName), key => CreateSetter(type, propertyName));
            setter(instance, value);
        }

        private static Action<object, object> CreateSetter(Type type, string propertyName)
        {
            var prop = type.GetProperty(propertyName);
            var setMethod = prop.GetSetMethod(true);
            var dm = new System.Reflection.Emit.DynamicMethod(
                "set_" + propertyName, null, new[] { typeof(object), typeof(object) },type, true);
            var il = dm.GetILGenerator();

            il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            il.Emit(System.Reflection.Emit.OpCodes.Castclass, type);
            il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
            if (prop.PropertyType.IsValueType)
                il.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, prop.PropertyType);
            else
                il.Emit(System.Reflection.Emit.OpCodes.Castclass, prop.PropertyType);
            il.Emit(System.Reflection.Emit.OpCodes.Callvirt, setMethod);
            il.Emit(System.Reflection.Emit.OpCodes.Ret);

            return (Action<object, object>)dm.CreateDelegate(typeof(Action<object, object>));
        }

        private static readonly ConcurrentDictionary<(Type, string), Func<object, object>> _getterCache = new();

        /// <summary>
        /// 验证过这里不存在问题
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="instance"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetValue(string propertyName, object instance, Type type = null)
        {
            type ??= instance.GetType();
            var getter = _getterCache.GetOrAdd((type, propertyName), key => CreateGetter(type, propertyName));
            return getter(instance);
        }

        private static Func<object, object> CreateGetter(Type type, string propertyName)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null || !prop.CanRead)
                throw new ArgumentException($"Property '{propertyName}' not found or not readable on type '{type.FullName}'.");

            var getMethod = prop.GetGetMethod(true);
            var dm = new System.Reflection.Emit.DynamicMethod(
                "get_" + propertyName, typeof(object), new[] { typeof(object) },type, true);
            var il = dm.GetILGenerator();

            il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            il.Emit(System.Reflection.Emit.OpCodes.Castclass, type);
            il.Emit(System.Reflection.Emit.OpCodes.Callvirt, getMethod);

            if (prop.PropertyType.IsValueType)
                il.Emit(System.Reflection.Emit.OpCodes.Box, prop.PropertyType);

            il.Emit(System.Reflection.Emit.OpCodes.Ret);

            return (Func<object, object>)dm.CreateDelegate(typeof(Func<object, object>));
        }

        private static readonly ConcurrentDictionary<MethodInfo, Func<object, object[], object>> _methodCache = new();

        /// <summary>
        /// 使用Emit高性能调用对象方法
        /// </summary>
        public static object InvokeMethod(Type type,string method, object target, params object[] parameters)
        {
            return InvokeMethod(type.GetMethodWithCache(method), target, parameters);
        }
        public static object InvokeMethod(MethodInfo method, object target, params object[] parameters)
        {
            var invoker = _methodCache.GetOrAdd(method, CreateMethodInvoker);
            return invoker(target, parameters);
        }

        private static Func<object, object[], object> CreateMethodInvoker(MethodInfo method)
        {
            var dm = new DynamicMethod(
                "MethodInvoker",
                typeof(object),
                new[] { typeof(object), typeof(object[]) },
                method.DeclaringType,
                true);

            var il = dm.GetILGenerator();
            var ps = method.GetParameters();

            // 加载实例（如果是实例方法）
            if (!method.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, method.DeclaringType);
            }

            // 加载参数
            for (int i = 0; i < ps.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);

                var paramType = ps[i].ParameterType;
                if (paramType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, paramType);
                else
                    il.Emit(OpCodes.Castclass, paramType);
            }

            // 调用方法
            il.Emit(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method);

            // 处理返回值
            if (method.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else if (method.ReturnType.IsValueType)
                il.Emit(OpCodes.Box, method.ReturnType);

            il.Emit(OpCodes.Ret);

            return (Func<object, object[], object>)dm.CreateDelegate(typeof(Func<object, object[], object>));
        }
        #endregion
    }
}
