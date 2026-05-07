namespace InfiniteRefactor.Infrastructure.DataService
{
    public class ProcessResult
    {
        public static readonly ProcessResult Default = new ProcessResult();

        public bool CancelProcess { get; set; }
        public bool Last { get; set; }
        public string SourceName { get; set; }
        public string Message { get; set; }

        public ProcessResult()
        {
            CancelProcess = false;
            Last = false;
        }
    }
}
