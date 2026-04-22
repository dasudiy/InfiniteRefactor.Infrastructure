using AirMaster.Infrastructure.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace AirMaster.Infrastructure.Reflection
{
    public static class FastActivator
    {
        private static ConcurrentDictionary<Type, ObjectActivator> creatorCache = new ConcurrentDictionary<Type, ObjectActivator>();
        private static readonly ConcurrentDictionary<(Type, string), Func<object, object>> getterCache = new();
        private static readonly ConcurrentDictionary<(Type, string), Action<object, object>> setterCache = new();


        private delegate object ObjectActivator(params object[] args);

        private static ObjectActivator GetActivator(ConstructorInfo ctor)
        {
            Type type = ctor.DeclaringType;
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            ParameterExpression param =
                Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp =
                new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                Expression paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            LambdaExpression lambda =
                Expression.Lambda(typeof(ObjectActivator), newExp, param);

            //compile it
            ObjectActivator compiled = (ObjectActivator)lambda.Compile();
            return compiled;
        }

        /// <summary>
        /// Create a new instance of type T
        /// </summary>
        ///
        /// <typeparam name="T">
        /// The type of object to create
        /// </typeparam>
        ///
        /// <returns>
        /// A new instance of type T
        /// </returns>

        public static T CreateInstance<T>() where T : class
        {
            return (T)CreateInstance(typeof(T));
        }


        public static object CreateInstance(Type type)
        {
            ObjectActivator c;

            if (!creatorCache.TryGetValue(type, out c))
            {
                var ctor = type.GetConstructor(Array.Empty<Type>());

                c = GetActivator(ctor);
                creatorCache[type] = c;
            }
            return c();
        }

        /// <summary>
        /// 获取属性值（支持缓存）
        /// </summary>
        public static object GetValue(object instance, string propertyName)
        {
            var type = instance.GetType();
            var getter = getterCache.GetOrAdd((type, propertyName), key => CreateGetter(type, propertyName));
            return getter(instance);
        }

        /// <summary>
        /// 设置属性值（支持缓存）
        /// </summary>
        public static void SetValue(object instance, string propertyName, object value)
        {
            var type = instance.GetType();
            var setter = setterCache.GetOrAdd((type, propertyName), key => CreateSetter(type, propertyName));
            setter(instance, value);
        }

        private static Func<object, object> CreateGetter(Type type, string propertyName)
        {
            var param = Expression.Parameter(typeof(object), "instance");
            var castInstance = Expression.Convert(param, type);
            var property = Expression.PropertyOrField(castInstance, propertyName);
            var castResult = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(castResult, param);
            return lambda.Compile();
        }

        private static Action<object, object> CreateSetter(Type type, string propertyName)
        {
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var valueParam = Expression.Parameter(typeof(object), "value");
            var castInstance = Expression.Convert(instanceParam, type);

            var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || !property.CanWrite)
                throw new ArgumentException($"Property '{propertyName}' not found or not writable on type '{type.FullName}'.");

            var castValue = Expression.Convert(valueParam, property.PropertyType);
            var propertyAccess = Expression.Property(castInstance, property);
            var assign = Expression.Assign(propertyAccess, castValue);
            var lambda = Expression.Lambda<Action<object, object>>(assign, instanceParam, valueParam);
            return lambda.Compile();
        }

        private static readonly ConcurrentDictionary<MethodInfo, Func<object, object[], object>> _methodCache = new();
        /// <summary>
        /// 使用表达式树高性能调用对象方法
        /// </summary>
        public static object InvokeMethod(Type type, string method, object target, params object[] parameters)
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
            var instanceParam = Expression.Parameter(typeof(object), "target");
            var argsParam = Expression.Parameter(typeof(object[]), "args");

            var paramExprs = new Expression[method.GetParameters().Length];
            var ps = method.GetParameters();
            for (int i = 0; i < ps.Length; i++)
            {
                var arg = Expression.ArrayIndex(argsParam, Expression.Constant(i));
                paramExprs[i] = Expression.Convert(arg, ps[i].ParameterType);
            }

            Expression instanceExpr = method.IsStatic ? null : Expression.Convert(instanceParam, method.DeclaringType);
            var callExpr = Expression.Call(instanceExpr, method, paramExprs);

            Expression body;
            if (method.ReturnType == typeof(void))
            {
                var nullExpr = Expression.Constant(null);
                body = Expression.Block(callExpr, nullExpr);
            }
            else
            {
                body = Expression.Convert(callExpr, typeof(object));
            }

            var lambda = Expression.Lambda<Func<object, object[], object>>(body, instanceParam, argsParam);
            return lambda.Compile();
        }
    }
}
