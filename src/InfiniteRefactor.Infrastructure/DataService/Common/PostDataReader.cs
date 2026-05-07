using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Metadata;
using InfiniteRefactor.Infrastructure.Serializer;
using Microsoft.AspNetCore.Http;

namespace InfiniteRefactor.Infrastructure.DataService.Common
{
    /// <summary>
    /// 无需使用泛型，泛型多余了，反而造成ValueReader无法固定配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PostDataReader<T> : IParameterValueReader
    {
        public object ReadValue(ParamInfo paramInfo, DataServiceRequest request)
        {
            try
            {
                var req = request.RawRequestObject as HttpRequest;
                if (req != null)
                {
                    return request.Context.Serializer.Deserialize<T>(req.Body);
                }
                else
                {
                    return default(T);
                }
            }
            catch
            {
                return default(T);
            }
        }

        public async Task<object> ReadValueAsync(ParamInfo paramInfo, DataServiceRequest request)
        {
            try
            {
                var req = request.RawRequestObject as HttpRequest;
                if (req != null)
                {
                    return await request.Context.Serializer.DeserializeAsync(req.Body, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
            catch
            {
                return default(T);
            }
        }
    }

    public class PostDataReader : IParameterValueReader
    {
        //private ParamInfo paramInfo;
        //public PostDataReader(ParamInfo pInfo)
        //{
        //    paramInfo = pInfo;
        //}
        public object ReadValue(ParamInfo paramInfo, DataServiceRequest request)
        {
            try
            {
                var req = request.RawRequestObject as HttpRequest;
                if (req != null)
                {
                    return request.Context.Serializer.Deserialize(req.Body, paramInfo.Type);
                }
                else
                {
                    return default;
                }
            }
            catch
            {
                return default;
            }
        }

        public async Task<object> ReadValueAsync(ParamInfo paramInfo, DataServiceRequest request)
        {
            try
            {
                var req = request.RawRequestObject as HttpRequest;
                if (req != null)
                {
                    return await request.Context.Serializer.DeserializeAsync(req.Body, paramInfo.Type);
                }
                else
                {
                    return default;
                }
            }
            catch
            {
                return default;
            }
        }
    }
}
