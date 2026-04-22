using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Metadata;
using AirMaster.Infrastructure.Extensions;

using System;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Common
{
    public class UrlSegParameterReader<T> : IParameterValueReader
    {
        public int Part { get; set; }

        public UrlSegParameterReader(int part)
        {
            Part = part;
        }

        public object ReadValue(ParamInfo paramInfo, DataServiceRequest request)
        {
            try
            {
                var seg = request.Url.LocalPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (seg.Length > Part)
                {
                    return seg[Part].To<T>(default(T));
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
            return await Task.FromResult(ReadValue(paramInfo, request));
        }
    }
}
