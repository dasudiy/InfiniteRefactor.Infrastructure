using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Abstractions
{
    public abstract class DataServiceResponse : IDisposable
    {
        public DataServiceResponse(DataServiceContext context)
        {
            OutputParams = new Dictionary<string, object>();
            this.Context = context;
        }

        public object RawResult { get; internal set; }
        public object Result { get; set; }
        public Dictionary<string, object> OutputParams { get; set; }
        public Exception Exception { get; set; }
        public abstract object RawResponseObject { get; }
        public DataServiceContext Context { get; set; }
        public abstract Stream OutputStream { get; }
        public bool IsEnded { get; protected set; }
        //public Encoding Encoding { get; set; }

        public abstract TextWriter OutputWriter { get; }

        public virtual void WriteResult()
        {
            if (!IsEnded && OutputStream.CanWrite)
            {
                try
                {
                    if (Exception != null)
                    {
                        Context.Serializer.Serialize(Exception, OutputStream);
                    }
                    else
                    {
                        Context.Serializer.Serialize(Result, OutputStream);
                    }
                }
                catch (Exception ex)
                {
                    Context.Serializer.Serialize(ex, OutputStream);
                }
                OutputStream.Flush();
                //End();
            }
        }

        public virtual async Task WriteResultAsync()
        {
            if (!IsEnded && OutputStream.CanWrite)
            {
                try
                {
                    if (Exception != null)
                    {
                        await Context.Serializer.SerializeAsync(Exception, OutputStream);
                    }
                    else
                    {
                        await Context.Serializer.SerializeAsync(Result, OutputStream);
                    }
                }
                catch (Exception ex)
                {
                    await Context.Serializer.SerializeAsync(ex, OutputStream);
                }
                await OutputStream.FlushAsync();
                //End();
            }
        }

        public virtual void End()
        {
            IsEnded = true;
        }

        public abstract bool IsClientConnected { get; }

        public abstract string ContentType { get; set; }

        public virtual void Dispose()
        {
        }
    }
}
