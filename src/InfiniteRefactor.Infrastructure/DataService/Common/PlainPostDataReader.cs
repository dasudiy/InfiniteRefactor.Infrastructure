using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Metadata;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Common
{
    public class PlainPostDataReader : IParameterValueReader
    {
        public virtual object ReadValue(ParamInfo paramInfo, DataServiceRequest request)
        {
            try
            {
                var req = request.RawRequestObject as HttpRequest;
                if (req != null)
                {
                    //if (req.ContentType != "text/plain") { return null; }
                    using (var sr = new StreamReader(req.Body))
                    {
                        return sr.ReadToEnd();
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<object> ReadValueAsync(ParamInfo paramInfo, DataServiceRequest request)
        {
            try
            {
                var req = request.RawRequestObject as HttpRequest;
                if (req != null)
                {
                    //if (req.ContentType != "text/plain") { return null; }
                    using (var sr = new StreamReader(req.Body))
                    {
                        return await sr.ReadToEndAsync();
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
