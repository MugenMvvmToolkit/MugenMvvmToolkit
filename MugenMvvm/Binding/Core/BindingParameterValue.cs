using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingParameterValue : IDisposable
    {
        #region Fields

        public readonly ICompiledExpression? Expression;
        public readonly object? Parameter;

        #endregion

        #region Constructors

        public BindingParameterValue(object? parameter, ICompiledExpression? expression)
        {
            Parameter = parameter;
            Expression = expression;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Parameter == null && Expression == null;

        #endregion

        #region Methods

        [return: MaybeNull]
        public T GetValue<T>(IReadOnlyMetadataContext? metadata)
        {
            if (Expression != null)
                return (T) Expression.Invoke(Parameter, metadata)!;
            if (Parameter is IMemberPathObserver observer)
                return (T) observer.GetLastMember(metadata).GetValueOrThrow(metadata)!;
            return (T) Parameter!;
        }

        public void Dispose()
        {
            Expression?.Dispose();
            MugenBindingExtensions.DisposeBindingSource(Parameter);
        }

        #endregion
    }
}