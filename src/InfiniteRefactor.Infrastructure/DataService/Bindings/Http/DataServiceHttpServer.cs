/*
 * 由SharpDevelop创建。
 * 用户： Asuka
 * 日期: 2013/6/23
 * 时间: 10:17
 *
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Bindings.AspNet;
using AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace AirMaster.Infrastructure.DataService.Bindings.Http
{
    public class DataServiceHttpServer : DataServiceServerBase
    {
        private IWebHost webHost;
        private X509Certificate2 rootCert;

        public DataServiceHttpServer(string prefix, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null)
            : base(prefix, certificate, validateCertificate, host)
        {
            prefix = prefix.Replace("+", "0.0.0.0").Replace("*", "0.0.0.0");
            var uri = new Uri(prefix);
            if (uri.Scheme != "http" && uri.Scheme != "https") { throw new ArgumentException("only support http and https"); }
            if (uri.Scheme == "https" && certificate == null) { throw new ArgumentException("certificate is required when using https"); }

            webHost = new WebHostBuilder()
                .UseKestrel(o =>
                {
                    if (!IPAddress.TryParse(uri.Host, out var ipAddress))
                    {
                        ipAddress = Dns.GetHostAddresses(uri.DnsSafeHost).First();
                    }
                    var port = uri.Port;

                    if (uri.Scheme == "https" && uri.Scheme == "wss")
                    {
                        o.Listen(ipAddress, port, lo => lo.UseHttps(certificate, httpsOp =>
                        {
                            if (certificate != null)
                            {
                                httpsOp.ServerCertificate = certificate;
                            }

                            if (validateCertificate)
                            {
                                rootCert = CertificateValidator.ValidateAndGetRootCA(certificate, null, null);
                                httpsOp.ClientCertificateMode = Microsoft.AspNetCore.Server.Kestrel.Https.ClientCertificateMode.RequireCertificate;
                                httpsOp.ClientCertificateValidation = (cert, chain, err) => CertificateValidator.Validate(certificate, rootCert, chain);
                            }
                        }));
                    }
                    else
                    {
                        o.Listen(ipAddress, port);
                    }

                    o.Limits.MaxRequestBodySize = int.MaxValue;
                    o.AllowSynchronousIO = true;
                })
                .ConfigureServices(services =>
                {
                    services.Configure<FormOptions>(options =>
                    {
                        options.ValueLengthLimit = int.MaxValue;
                        options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
                        options.MultipartHeadersLengthLimit = int.MaxValue;
                    });
                })
                .Configure(app => app.Run(async httpContext =>
                {
                    try
                    {
                        var context = new AspNetDataServiceContext(httpContext);
                        await DataServiceHost.ProcessContextAsync(context);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                })).Build();               
        }

        public override void Start()
        {
            _ = webHost.RunAsync();
        }

        public override void Stop()
        {
            _ = webHost.StopAsync();
        }
    }

    ///// <summary>
    ///// Description of DataServiceServer.
    ///// </summary>
    //public class DataServiceHttpServer : DataServiceServerBase
    //{
    //    private HttpListener listener = new HttpListener();
    //    private volatile bool exiting = false;
    //    private Task workingTask;
    //    private CancellationTokenSource cancel = new CancellationTokenSource();

    //    public bool AllowWebSocket { get; set; }
    //    public bool AllowCompress { get; set; }

    //    public override IReadOnlyList<DataServiceClientBase> ActiveClients
    //    {
    //        get
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    public DataServiceHttpServer(string prefix, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null)
    //        : base(prefix, certificate, validateCertificate, host)
    //    {
    //        listener.Prefixes.Add(prefix);
    //    }

    //    public override void Start()
    //    {
    //        listener.Start();
    //        //if (!SystemInfo.IsMonoRuntime())
    //        //{
    //        //    try
    //        //    {
    //        //        HttpApi.SetRequestQueueLength(listener, ushort.MaxValue);
    //        //    }
    //        //    catch
    //        //    {
    //        //    }
    //        //}

    //        workingTask = Task.Run(new Action(Work), cancel.Token);
    //        //workingTask.Start();
    //    }

    //    public override void Stop()
    //    {
    //        lock (this)
    //        {
    //            exiting = true;
    //            cancel.Cancel();

    //            if (listener != null && listener.IsListening)
    //            {
    //                listener.Stop();
    //            }

    //            if (workingTask != null) { workingTask.Wait(); }
    //        }
    //    }

    //    private async void Work()
    //    {
    //        while (!exiting && listener.IsListening)
    //        {
    //            try
    //            {
    //                var httpContext = await listener.GetContextAsync().ConfigureAwait(false);
    //                try
    //                {
    //                    var context = new DataServiceHttpContext(httpContext, AllowCompress);
    //                    _ = Task.Run(async () => await this.DataServiceHost.ProcessContextAsync(context));
    //                }
    //                catch (Exception ex)
    //                {
    //                    System.Diagnostics.Debug.WriteLine(ex.Message);
    //                }
    //            }
    //            catch
    //            {
    //            }
    //        }
    //    }

    //    public override void Dispose()
    //    {
    //        Stop();
    //        listener.Close();
    //    }
    //}
}
