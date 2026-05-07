using System;
using System.Collections.Generic;
using System.Net;
using InfiniteRefactor.Infrastructure.DataService.Metadata;
using InfiniteRefactor.Infrastructure.Extensions;

namespace InfiniteRefactor.Infrastructure.DataService.Abstractions
{
    public abstract class DataServiceRequest : IDisposable
    {
        public DataServiceRequest(DataServiceContext context)
        {
            this.Context = context;
        }

        public DataServiceContext Context { get; set; }
        public abstract Uri Url { get; }
        //public abstract string RawRequestUrl { get; }
        public abstract object RawRequestObject { get; }
        //public abstract string PathInfo { get; }
        //public abstract Dictionary<string, object> Parameters { get; }

        public abstract IPEndPoint Remote { get; }

        public abstract object this[string key] { get; }

        public Dictionary<string, object> ParameterCache { get; private set; } = new Dictionary<string, object>();

        public virtual object ReadParameter(string name, Type targetType)
        {
            object rawValue = null;
            if (!ParameterCache.TryGetValue(name, out rawValue))
            {
                rawValue = ParameterCache[name] = this[name];
            }
            if (rawValue != null)
            {
                object value;
                if (rawValue.TryConvertTo(targetType, out value))
                {
                    return ParameterCache[name] = value;
                }
                else
                {
                    return ParameterCache[name] = Context.Serializer.Deserialize(rawValue.ToString(), targetType);
                }
            }
            return rawValue;
        }

        public T ReadParameter<T>(string name, T defaultValue)
        {
            var result = ReadParameter(name, typeof(T));
            if (result == null) { return defaultValue; }
            else { return (T)result; }
        }

        internal object ReadParameter(ParamInfo parameter)
        {
            var value = ReadParameter(parameter.Name, parameter.Type);
            if (value == null && !(parameter.DefaultValue is DBNull))
            {
                return parameter.DefaultValue;
            }
            return value;
        }

        internal bool Satisfy(Dictionary<string, ParamInfo> dictionary)
        {
            return true;
        }

        public virtual void Dispose()
        {
        }
    }
}
