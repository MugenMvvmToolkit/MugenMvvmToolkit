using System;
using System.Threading;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core.Components
{
    public abstract class DelayBindingHandler : IComponent<IBinding>, IHasPriority, IAttachableComponent, IDetachableComponent, IThreadDispatcherHandler, IDisposable
    {
        private IBinding? _binding;
        private bool _isUpdating;
        private Timer? _timer;

        protected DelayBindingHandler(ushort delay)
        {
            Delay = delay;
        }

        public static int Priority { get; set; } = BindingComponentPriority.Delay;

        public static ThreadExecutionMode ExecutionMode { get; set; } = ThreadExecutionMode.Main;//todo fix

        public ushort Delay { get; }

        int IHasPriority.Priority => Priority;

        public static IComponent<IBinding> GetTarget(ushort delay) => new Target(delay);

        public static IComponent<IBinding> GetSource(ushort delay) => new Source(delay);

        protected abstract void Update(IBinding binding);

        protected object? OnValueChanging(object? value)
        {
            if (value.IsDoNothing() || _isUpdating)
                return value;

            _timer?.Change(Delay, Timeout.Infinite);
            return BindingMetadata.DoNothing;
        }

        void IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _binding = (IBinding) owner;
            _timer = WeakTimer.Get(this, handler => MugenService.ThreadDispatcher.Execute(ExecutionMode, handler, null));
        }

        void IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _timer?.Dispose();
            _binding = null;
        }

        void IDisposable.Dispose() => _timer?.Dispose();

        void IThreadDispatcherHandler.Execute(object? state)
        {
            try
            {
                var binding = _binding;
                if (binding == null)
                    return;

                _isUpdating = true;
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                Update(binding);
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                _isUpdating = false;
            }
        }

        internal sealed class Target : DelayBindingHandler, ITargetValueInterceptorComponent
        {
            public Target(ushort delay) : base(delay)
            {
            }

            public object? InterceptTargetValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata) => OnValueChanging(value);

            protected override void Update(IBinding binding) => binding.UpdateTarget();
        }

        internal sealed class Source : DelayBindingHandler, ISourceValueInterceptorComponent
        {
            public Source(ushort delay) : base(delay)
            {
            }

            public object? InterceptSourceValue(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata) => OnValueChanging(value);

            protected override void Update(IBinding binding) => binding.UpdateSource();
        }
    }
}