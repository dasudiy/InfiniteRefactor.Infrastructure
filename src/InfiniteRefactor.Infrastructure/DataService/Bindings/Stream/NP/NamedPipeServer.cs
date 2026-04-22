using AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using NLog;
using System;
using System.IO.Pipes;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Bindings.Stream.NP
{
    public class NamedPipeServer : StreamServer
    {
        private volatile bool _exiting = false;
        private string pipeName;
        private CancellationTokenSource source = new CancellationTokenSource();
        private Thread _workingThread;
        //private PipeSecurity ps;
        private Logger log = LogManager.GetCurrentClassLogger();

        public NamedPipeServer(string prefix, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null)
            : base(prefix, certificate, validateCertificate, host)
        {
            var uri = new Uri(prefix);
            this.pipeName = uri.PathAndQuery.TrimStart('/');

            //ps = new PipeSecurity();
            //ps.AddAccessRule(new PipeAccessRule("Users", PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance, AccessControlType.Allow));
            //ps.AddAccessRule(new PipeAccessRule("CREATOR OWNER", PipeAccessRights.FullControl, AccessControlType.Allow));
            //ps.AddAccessRule(new PipeAccessRule("SYSTEM", PipeAccessRights.FullControl, AccessControlType.Allow));
        }

        void ServerLoop()
        {
            //var ps = new PipeSecurity();
            //var sid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
            //var par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            //ps.AddAccessRule(par);

            while (!_exiting)
            {
                try
                {
                    var stream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024/*, ps*/);
                    //stream.SetAccessControl(ps);

                    var task = Task.Factory.FromAsync(stream.BeginWaitForConnection, stream.EndWaitForConnection, null);
                    //task.Start();
                    task.Wait(source.Token);

                    if (task.IsCompleted)
                    {
                        base.NewConnection(new NamedPipeController(stream));
                    }
                }
                catch
                {
                }
            }
        }

        public override void Start()
        {
            _exiting = false;
            _workingThread = new Thread(new ThreadStart(() => ServerLoop()));
            _workingThread.Start();
            log.Trace("DataService Server在{0}开始监听...", this.Prefix);
        }

        public override void Stop()
        {
            _exiting = true;
            source.Cancel();
            base.Dispose();
            if (_workingThread != null)
            {
                try
                {
                    _workingThread.Join();
                }
                catch
                {
                }
            }
        }
    }

}
