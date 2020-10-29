using System;
using System.Threading;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components.Binding;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Bindings.Core.Components.Binding
{
    public abstract class DelayBindingComponent : IComponent<IBinding>, IHasPriority, IAttachableComponent, IDetachableComponent, IThreadDispatcherHandler
    {
        #region Fields

        private IBinding? _binding;
        private bool _isUpdating;
        private Timer? _timer;

        private static readonly TimerCallback CallbackDelegate = Callback;

        #endregion

        #region Constructors

        protected DelayBindingComponent(ushort delay)
        {
            Delay = delay;
        }

        #endregion

        #region Properties

        public ushort Delay { get; }

        public static int Priority { get; set; } = BindingComponentPriority.Delay;

        public static ThreadExecutionMode ExecutionMode { get; set; } = ThreadExecutionMode.Main;

        int IHasPriority.Priority => Priority;

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _binding = (IBinding) owner;
            _timer = new Timer(CallbackDelegate, this, Timeout.Infinite, Timeout.Infinite);
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _timer?.Dispose();
            _binding = null;
        }

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

        #endregion

        #region Methods

        protected abstract void Update(IBinding binding);

        protected object? OnValueChanging(object? value)
        {
            if (value.IsDoNothing() || _isUpdating)
                return value;

            _timer?.Change(Delay, Timeout.Infinite);
            return BindingMetadata.DoNothing;
        }

        public static IComponent<IBinding> GetTarget(ushort delay) => new Target(delay);

        public static IComponent<IBinding> GetSource(ushort delay) => new Source(delay);

        private static void Callback(object? state) => MugenService.ThreadDispatcher.Execute(ExecutionMode, (IThreadDispatcherHandler) state!, null);

        #endregion

        #region Nested types

        internal sealed class Target : DelayBindingComponent, ITargetValueInterceptorBindingComponent
        {
            #region Constructors

            public Target(ushort delay) : base(delay)
            {
            }

            #endregion

            #region Implementation of interfaces

            public object? InterceptTargetValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata) => OnValueChanging(value);

            #endregion

            #region Methods

            protected override void Update(IBinding binding) => binding.UpdateTarget();

            #endregion
        }

        internal sealed class Source : DelayBindingComponent, ISourceValueInterceptorBindingComponent
        {
            #region Constructors

            public Source(ushort delay) : base(delay)
            {
            }

            #endregion

            #region Implementation of interfaces

            public object? InterceptSourceValue(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata) => OnValueChanging(value);

            #endregion

            #region Methods

            protected override void Update(IBinding binding) => binding.UpdateSource();

            #endregion
        }

        #endregion
    }
}