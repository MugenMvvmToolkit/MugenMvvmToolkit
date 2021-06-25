﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Core
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingParameterValue : IDisposable
    {
        public readonly ICompiledExpression? Expression;
        public readonly object? Parameter;

        public BindingParameterValue(object? parameter, ICompiledExpression? expression)
        {
            Parameter = parameter;
            Expression = expression;
        }

        [MemberNotNullWhen(false, nameof(Parameter), nameof(Expression))]
        public bool IsEmpty => Parameter == null && Expression == null;

        public T? GetValue<T>(IReadOnlyMetadataContext? metadata)
        {
            if (Expression != null)
                return (T)Expression.Invoke(Parameter, metadata)!;
            if (Parameter is IMemberPathObserver observer)
                return (T)observer.GetLastMember(metadata).GetValueOrThrow(metadata)!;
            if (Parameter == null)
                return default;
            return (T?)Parameter;
        }

        public void Dispose() => BindingMugenExtensions.DisposeBindingSource(Parameter);
    }
}