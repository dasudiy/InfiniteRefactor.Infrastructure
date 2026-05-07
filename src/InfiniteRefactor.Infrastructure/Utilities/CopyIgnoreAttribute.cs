using System;

namespace InfiniteRefactor.Infrastructure.Utilities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CopyIgnoreAttribute : Attribute
    {
    }
}
