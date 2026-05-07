using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.Security;
using Microsoft.AspNetCore.Http;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.AspNet.EncryptionContext
{
    public class EncryptionResponse : DataServiceResponse
    {
        private HttpResponse httpResponse;
        private string securityKey;
        private string provider;
        private Lazy<List<System.IO.Stream>> outputStream;
        private Lazy<StreamWriter> output;

        public EncryptionResponse(HttpResponse httpResponse, DataServiceContext context)
            : base(context)
        {
            this.httpResponse = httpResponse;
            this.securityKey = (context as EncryptionHttpContext).SecurityKey;
            this.provider = EncryptionHttpContext.SymmetricAlgorithm;

            this.outputStream = new Lazy<List<System.IO.Stream>>(() =>
            {
                var list = new List<System.IO.Stream>();
                System.IO.Stream cs = Cryptography.CreateSymmetricCryptoStream(provider, securityKey, httpResponse.Body, CryptoStreamMode.Write);
                list.Add(cs);

                if ((context as EncryptionHttpContext).GZip)
                {
                    this.httpResponse.Headers.Append("Content-Encoding", this.provider.ToLower() + "+gzip");
                    list.Add(new System.IO.Compression.GZipStream(cs, System.IO.Compression.CompressionMode.Compress, true));
                }
                return list;
            });

            this.output = new Lazy<StreamWriter>(() =>
            {
                return new StreamWriter(outputStream.Value.Last(), Encoding.UTF8);
            });
        }

        public override object RawResponseObject
        {
            get { return httpResponse; }
        }

        public override System.IO.Stream OutputStream
        {
            get { return this.outputStream.Value.Last(); }
        }

        public override System.IO.TextWriter OutputWriter
        {
            get { return this.output.Value; }
        }

        public override void End()
        {
            base.End();
            if (output.IsValueCreated)
            {
                OutputWriter.Flush();
                OutputWriter.Close();
                OutputWriter.Dispose();
            }
            if (outputStream.IsValueCreated)
            {
                foreach (var item in outputStream.Value.Reverse<System.IO.Stream>())
                {
                    item.Flush();
                    item.Close();
                    item.Dispose();
                }
            }

            httpResponse.Body.Flush();
            httpResponse.Body.Close();
        }

        public override bool IsClientConnected
        {
            get { return true; }
        }

        public override string ContentType
        {
            get
            {
                return httpResponse.ContentType;
            }
            set
            {
                httpResponse.ContentType = value;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            this.End();
        }
    }

}
