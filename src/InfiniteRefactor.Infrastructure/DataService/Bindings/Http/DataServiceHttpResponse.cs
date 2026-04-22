using AirMaster.Infrastructure.DataService.Abstractions;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Bindings.Http
{
    public class DataServiceHttpResponse : DataServiceResponse
    {
        private System.Net.HttpListenerResponse httpListenerResponse;
        private Lazy<TextWriter> outputWriter;

        public DataServiceHttpResponse(System.Net.HttpListenerResponse httpListenerResponse, bool gzip, DataServiceContext context)
            : base(context)
        {
            this.httpListenerResponse = httpListenerResponse;

            if (gzip)
            {
                this.httpListenerResponse.Headers[System.Net.HttpResponseHeader.ContentEncoding] = "gzip";
                _outputStream = new GZipStream(_memStream, CompressionMode.Compress, true);
            }
            else
            {
                _outputStream = _memStream;
            }
            outputWriter = new Lazy<TextWriter>(() => new StreamWriter(_outputStream));
        }

        public override object RawResponseObject
        {
            get { return httpListenerResponse; }
        }

        private MemoryStream _memStream = new MemoryStream();
        private System.IO.Stream _outputStream;
        public override System.IO.Stream OutputStream
        {
            get
            {
                return _outputStream;
            }
        }


        public override TextWriter OutputWriter
        {
            get { return outputWriter.Value; }
        }

        public override void WriteResult()
        {
            try
            {
                base.WriteResult();
            }
            catch
            {

            }
            finally
            {
                End();
            }
        }

        public override async Task WriteResultAsync()
        {
            try
            {
                await base.WriteResultAsync();
            }
            catch
            {
            }
            finally
            {
                End();
            }
        }

        public override void End()
        {
            base.End();

            OutputStream.Flush();
            httpListenerResponse.ContentLength64 = _memStream.Length;
            _memStream.Position = 0;
            _memStream.CopyTo(httpListenerResponse.OutputStream, 1024);
            httpListenerResponse.OutputStream.Flush();

            httpListenerResponse.Close();
            OutputStream.Close();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (outputWriter.IsValueCreated) { outputWriter.Value.Dispose(); }
            OutputStream.Dispose();
        }

        public override bool IsClientConnected
        {
            get
            {
                return true;
            }
        }

        public override string ContentType
        {
            get
            {
                return httpListenerResponse.ContentType;
            }
            set
            {
                httpListenerResponse.ContentType = value;
            }
        }
    }
}
