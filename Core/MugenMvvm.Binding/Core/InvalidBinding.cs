using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    public sealed class InvalidBinding : Binding, IBindingExpression
    {
        #region Constructors

        public InvalidBinding(Exception exception) : base(EmptyPathObserver.Empty, null)
        {
            SetFlag(InvalidFlag);
            Exception = exception;
        }

        #endregion

        #region Properties

        public Exception Exception { get; }

        #endregion

        #region Implementation of interfaces

        IBinding IBindingExpression.Build(object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return this;
        }

        #endregion

        #region Methods

        protected override bool UpdateSourceInternal(out object? newValue)
        {
            throw Exception;
        }

        protected override bool UpdateTargetInternal(out object? newValue)
        {
            throw Exception;
        }

        #endregion
    }
}