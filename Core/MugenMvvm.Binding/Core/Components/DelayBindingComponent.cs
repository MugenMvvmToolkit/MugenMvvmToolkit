using System;
using System.Threading;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Binding.Core.Components
{
    public abstract class DelayBindingComponent : IComponent<IBinding>, IHasPriority, IAttachableComponent, IDetachableComponent, IThreadDispatcherHandler<object?>
    {
        #region Fields

        private readonly int _delay;

        private IBinding? _binding;
        private bool _isUpdating;
        private Timer? _timer;

        private static readonly TimerCallback CallbackDelegate = Callback;

        #endregion

        #region Constructors

        protected DelayBindingComponent(int delay)
        {
            _delay = delay;
        }

        #endregion

        #region Properties

        public static int Priority { get; set; } = BindingComponentPriority.Delay;

        public static ThreadExecutionMode ExecutionMode { get; set; } = ThreadExecutionMode.Main;

        int IHasPriority.Priority => Priority;

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _binding = (IBinding) owner;
            _timer = new Timer(CallbackDelegate, this.ToWeakReference(), Timeout.Infinite, Timeout.Infinite);
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _timer.Dispose();
            _binding = null;
        }

        void IThreadDispatcherHandler<object?>.Execute(object? state)
        {
            try
            {
                var binding = _binding;
                if (binding == null)
                    return;

                _isUpdating = true;
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
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

        #endregion

        #region Methods

        private static void Callback(object state)
        {
            var component = (DelayBindingComponent) ((IWeakReference) state).Target;
            if (component != null)
                MugenService.ThreadDispatcher.Execute(ExecutionMode, component);
        }

        public static IComponent<IBinding> GetTarget(int delay)
        {
            return new TargetDelay(delay);
        }

        public static IComponent<IBinding> GetSource(int delay)
        {
            return new SourceDelay(delay);
        }

        protected abstract void Update(IBinding binding);

        protected object? OnValueChanging(object? value)
        {
            if (value.IsDoNothing() || _isUpdating)
                return value;

            _timer.Change(_delay, Timeout.Infinite);
            return BindingMetadata.DoNothing;
        }

        #endregion

        #region Nested types

        private sealed class TargetDelay : DelayBindingComponent, ITargetValueInterceptorBindingComponent
        {
            #region Constructors

            public TargetDelay(int delay) : base(delay)
            {
            }

            #endregion

            #region Implementation of interfaces

            public object? InterceptTargetValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata)
            {
                return OnValueChanging(value);
            }

            #endregion

            #region Methods

            protected override void Update(IBinding binding)
            {
                binding.UpdateTarget();
            }

            #endregion
        }

        private sealed class SourceDelay : DelayBindingComponent, ISourceValueInterceptorBindingComponent
        {
            #region Constructors

            public SourceDelay(int delay) : base(delay)
            {
            }

            #endregion

            #region Implementation of interfaces

            public object? InterceptSourceValue(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata)
            {
                return OnValueChanging(value);
            }

            #endregion

            #region Methods

            protected override void Update(IBinding binding)
            {
                binding.UpdateSource();
            }

            #endregion
        }

        #endregion
    }
}