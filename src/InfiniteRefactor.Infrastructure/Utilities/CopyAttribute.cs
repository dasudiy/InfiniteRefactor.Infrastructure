using System;

namespace InfiniteRefactor.Infrastructure.Utilities
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class CopyAttribute : Attribute
    {
    }
}
