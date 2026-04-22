using System;

namespace AirMaster.Infrastructure.Utilities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CopyIgnoreAttribute : Attribute
    {
    }
}
