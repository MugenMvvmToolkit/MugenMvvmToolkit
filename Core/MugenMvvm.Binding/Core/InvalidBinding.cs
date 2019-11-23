using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    public sealed class InvalidBinding : Binding, IBindingExpression
    {
        #region Fields

        private readonly Exception _exception;

        #endregion

        #region Constructors

        public InvalidBinding(Exception exception) : base(EmptyPathObserver.Empty, sourceRaw: null)
        {
            SetFlag(InvalidFlag);
            _exception = exception;
        }

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
            throw _exception;
        }

        protected override bool UpdateTargetInternal(out object? newValue)
        {
            throw _exception;
        }

        #endregion
    }
}