using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Core
{
    public sealed class InvalidBinding : Binding, IBindingBuilder
    {
        public InvalidBinding(Exception exception) : base(EmptyPathObserver.Empty, null)
        {
            SetFlag(InvalidFlag);
            Exception = exception;
        }

        public Exception Exception { get; }

        protected override bool UpdateSourceInternal(out object? newValue) => Throw(out newValue);

        protected override bool UpdateTargetInternal(out object? newValue) => Throw(out newValue);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool Throw(out object? _) => throw Exception;

        IBinding IBindingBuilder.Build(object target, object? source, IReadOnlyMetadataContext? metadata) => this;
    }
}