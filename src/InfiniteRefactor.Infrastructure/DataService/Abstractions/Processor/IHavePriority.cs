namespace InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor
{
    public interface IHavePriority
    {
        int Priority { get; set; }
    }

    //internal class PriorityComparer : IComparer<IHavePriority>
    //{
    //    internal static readonly PriorityComparer Instance = new PriorityComparer();
    //    private PriorityComparer() { }

    //    public int Compare(IHavePriority x, IHavePriority y)
    //    {
    //        return y.Priority - x.Priority;
    //    }
    //}

}
