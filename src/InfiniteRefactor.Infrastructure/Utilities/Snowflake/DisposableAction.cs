// Copyright 2010-2012 Twitter, Inc.
// Licensed under the Apache License, Version 2.0
// Ported from Twitter Snowflake: https://github.com/twitter-archive/snowflake

using System;

namespace InfiniteRefactor.Infrastructure.Utilities.Snowflake
{
    public class DisposableAction : IDisposable
    {
        readonly Action _action;

        public DisposableAction(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            _action = action;
        }

        public void Dispose()
        {
            _action();
        }
    }
}