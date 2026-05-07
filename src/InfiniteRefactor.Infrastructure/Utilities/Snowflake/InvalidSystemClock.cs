using System;

namespace InfiniteRefactor.Infrastructure.Utilities.Snowflake
{
    public class InvalidSystemClock : Exception
    {
        public InvalidSystemClock(string message) : base(message) { }
    }
}