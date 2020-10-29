using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Core
{
    public sealed class InvalidBinding : Binding, IBindingBuilder
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

        IBinding IBindingBuilder.Build(object target, object? source, IReadOnlyMetadataContext? metadata) => this;

        #endregion

        #region Methods

        protected override bool UpdateSourceInternal(out object? newValue) => throw Exception;

        protected override bool UpdateTargetInternal(out object? newValue) => throw Exception;

        #endregion
    }
}