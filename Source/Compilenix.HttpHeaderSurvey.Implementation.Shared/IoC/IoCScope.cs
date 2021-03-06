﻿using System;
using System.Diagnostics;
using Compilenix.HttpHeaderSurvey.Integration.Shared.IoC;
using SimpleInjector;

namespace Compilenix.HttpHeaderSurvey.Implementation.Shared.IoC
{
    [DebuggerStepThrough]
    public class IoCScope : IIoCScope
    {
        private readonly Scope _scope;

        public IoCScope(Scope scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public IIoCScope BeginLifetimeScope()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public T Resolve<T>()
            where T : class
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return _scope.GetInstance<T>();
        }
    }
}