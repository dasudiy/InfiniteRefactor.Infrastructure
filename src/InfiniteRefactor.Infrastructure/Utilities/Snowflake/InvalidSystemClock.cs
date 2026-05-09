// Copyright 2010-2012 Twitter, Inc.
// Licensed under the Apache License, Version 2.0
// Ported from Twitter Snowflake: https://github.com/twitter-archive/snowflake

using System;

namespace InfiniteRefactor.Infrastructure.Utilities.Snowflake
{
    public class InvalidSystemClock : Exception
    {
        public InvalidSystemClock(string message) : base(message) { }
    }
}