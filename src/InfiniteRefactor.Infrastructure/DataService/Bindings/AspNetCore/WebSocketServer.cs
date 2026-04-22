using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using AirMaster.Infrastructure.DataService.Common.Processor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AirMaster.Infrastructure.DataService.Bindings.AspNetCore
{
    public class WebSocketServer : DataServiceServerBase
    {
        private readonly WebApplication app;
        private X509Certificate2 rootCert;

        public WebSocketServer(string prefix, X509Certificate2 certificate = null, bool validateCertificate = true,
            DataServiceHost host = null) : base(prefix, certificate, validateCertificate, host)
        {
            prefix = prefix.Replace("+", "0.0.0.0").Replace("*", "0.0.0.0");
            var uri = new Uri(prefix);

            var builder = WebApplication.CreateBuilder();
            builder.Services.Configure<KestrelServerOptions>(o =>
            {
                if (!IPAddress.TryParse(uri.Host, out var ipAddress))
                {
                    ipAddress = Dns.GetHostAddresses(uri.DnsSafeHost).First();
                }

                var port = uri.Port;

                if (uri.Scheme is "https" or "wss")
                {
                    if (certificate == null)
                    {
                        throw new ArgumentException("certificate is required when using https/wss");
                    }

                    o.Listen(ipAddress, port, lo => lo.UseHttps(certificate, httpsOp =>
                    {
                        httpsOp.ServerCertificate = certificate;

                        if (validateCertificate)
                        {
                            rootCert = CertificateValidator.ValidateAndGetRootCA(certificate, null, null);
                            httpsOp.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                            httpsOp.ClientCertificateValidation = (cert, chain, err) =>
                                CertificateValidator.Validate(certificate, rootCert, chain);
                        }
                    }));
                }
                else
                {
                    o.Listen(ipAddress, port);
                }

                o.Limits.MaxRequestBodySize = int.MaxValue;
                o.AllowSynchronousIO = true;
            });
            builder.Services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue; // if we don't set, the default value is: 128 MB
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            app = builder.Build();
            //app.UseRouting();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (uri.Scheme == "ws" || uri.Scheme == "wss")
            {
                app.UseWebSockets();
            }

            app.UseDataService(new DataServiceOption
            {
                RequestPath = uri.LocalPath.TrimEnd(Uri.SchemeDelimiter.ToCharArray()),
                InitFn = ds =>
                {
                    ds.PostProcessors.Add(new ResultWrapperAttribute()
                        { Priority = -1, DirectOutputParameterToResult = false });
                    //AirMaster.Infrastructure.DataService.DataServiceDefaultConfiguration.DefaultParameterReader = new AirMaster.Infrastructure.DataService.Common.PostDataReader();        
                    //ds.FindDataservicesFromAssembly(Assembly.GetExecutingAssembly());
                    ds.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                },
                EnableMetadataService = true,
                UseRestApi = false,
            }, host);
        }

        public override void Start()
        {
            app.Start();
        }

        public override void Stop()
        {
            _ = app.StopAsync();
        }
    }
}