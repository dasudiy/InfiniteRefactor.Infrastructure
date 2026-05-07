using System;

namespace InfiniteRefactor.Infrastructure.Utilities
{
    public static class SimpleArgument
    {
        public static SimpleArgument<T> Create<T>(T data) => new(data);
        public static SimpleArgument<T1, T2> Create<T1, T2>(T1 data1, T2 data2) => new(data1, data2);
    }

    public class SimpleArgument<T>(T data) : EventArgs
    {
        public T Data { get; set; } = data;
    }

    public class SimpleArgument<T1, T2>(T1 data1, T2 data2) : EventArgs
    {
        public T1 Data1 { get; set; } = data1;
        public T2 Data2 { get; set; } = data2;
    }
}