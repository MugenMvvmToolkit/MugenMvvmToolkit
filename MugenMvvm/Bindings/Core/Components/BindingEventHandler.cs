using System;
using System.Windows.Input;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core.Components
{
    public class BindingEventHandler : ITargetValueSetterComponent, IAttachableComponent, IDetachableComponent, IEventListener, IHasEventArgsComponent, IHasPriority
    {
        private EventHandler? _canExecuteHandler;
        private IReadOnlyMetadataContext? _currentMetadata;
        private object? _currentValue;
        private IAccessorMemberInfo? _enabledMember;
        private IWeakReference? _targetRef;
        private ActionToken _unsubscriber;

        private BindingEventHandler(BindingParameterValue commandParameter, bool toggleEnabledState)
        {
            CommandParameter = commandParameter;
            ToggleEnabledState = toggleEnabledState;
        }

        public static int Priority { get; set; } = BindingComponentPriority.EventHandler;

        public BindingParameterValue CommandParameter { get; private set; }

        public bool ToggleEnabledState { get; }

        public object? EventArgs { get; private set; }

        protected virtual bool IsOneTime => true;

        int IHasPriority.Priority => Priority;

        public static BindingEventHandler Get(BindingParameterValue commandParameter, bool toggleEnabledState, bool isOneTime)
        {
            if (isOneTime)
                return new BindingEventHandler(commandParameter, toggleEnabledState);
            return new OneWay(commandParameter, toggleEnabledState);
        }

        public bool TrySetTargetValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata)
        {
            if (value == _currentValue)
                return true;

            ClearValue();
            _currentMetadata = metadata;

            if (value is ICommand command)
            {
                _currentValue = value;
                if (ToggleEnabledState && InitializeCanExecute(targetMember.Target, command))
                {
                    OnCanExecuteChanged();
                    command.CanExecuteChanged += _canExecuteHandler;
                }
            }
            else if (value is IValueExpression)
                _currentValue = value;

            return true;
        }

        internal void OnCanExecuteChanged()
        {
            if (_currentValue is not ICommand cmd)
                return;

            var target = _targetRef?.Target;
            if (target != null)
                SetEnabled(cmd.CanExecute(CommandParameter.GetValue<object?>(_currentMetadata)), target);
        }

        private bool InitializeCanExecute(object? target, ICommand command)
        {
            if (target == null)
                return false;
            if (command is ICompositeCommand m && !m.HasCanExecute(_currentMetadata))
                return false;

            _enabledMember = BindableMembers.For<object>().Enabled().TryGetMember(target.GetType(), MemberFlags.InstancePublicAll, _currentMetadata);
            if (_enabledMember == null || !_enabledMember.CanWrite)
                return false;

            _targetRef = target.ToWeakReference();
            _canExecuteHandler ??= this.ToWeakReference().EventHandlerWeakCanExecuteHandler;
            return true;
        }

        private void SetEnabled(bool value, object? target = null)
        {
            var enabledMember = _enabledMember;
            if (enabledMember == null)
                return;

            target ??= _targetRef?.Target;
            if (target == null)
                return;

            enabledMember.SetValue(target, BoxingExtensions.Box(value), _currentMetadata);
        }

        private void ClearValue()
        {
            if (_canExecuteHandler != null && _currentValue is ICommand c)
            {
                c.CanExecuteChanged -= _canExecuteHandler;
                SetEnabled(true);
            }

            _targetRef = null;
            _enabledMember = null;
            _currentMetadata = null;
            _currentValue = null;
        }

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            var targetMember = ((IBinding) owner).Target.GetLastMember(metadata);
            if (targetMember.Member is not IObservableMemberInfo eventInfo || eventInfo.MemberType != MemberType.Event)
                return false;

            _unsubscriber = eventInfo.TryObserve(targetMember.Target, this, metadata);
            if (_unsubscriber.IsEmpty)
                return false;
            return true;
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            var binding = (IBinding) owner;
            binding.UpdateTarget();
            if (!BindingMugenExtensions.IsAllMembersAvailable(binding.Source) && IsOneTime)
                binding.Components.TryAdd(OneTimeBindingMode.NonDisposeInstance);
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _unsubscriber.Dispose();
            ClearValue();
            _canExecuteHandler = null;
            CommandParameter.Dispose();
            CommandParameter = default;
        }

        bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            var bindingManager = MugenService.BindingManager;
            var components = bindingManager.GetComponents<IBindingEventHandlerComponent>(_currentMetadata);
            try
            {
                EventArgs = message;
                components.OnBeginEvent(bindingManager, sender, message, _currentMetadata);
                switch (_currentValue)
                {
                    case ICommand command:
                        command.Execute(CommandParameter.GetValue<object?>(_currentMetadata));
                        return true;
                    case IValueExpression expression:
                        expression.Invoke(_currentMetadata);
                        return true;
                }

                return true;
            }
            catch (Exception e)
            {
                components.OnEventError(bindingManager, e, sender, message, _currentMetadata);
                return true;
            }
            finally
            {
                EventArgs = null;
                components.OnEndEvent(bindingManager, sender, message, _currentMetadata);
            }
        }

        internal sealed class OneWay : BindingEventHandler, IBindingSourceObserverListener
        {
            public OneWay(BindingParameterValue commandParameter, bool toggleEnabledState)
                : base(commandParameter, toggleEnabledState)
            {
            }

            protected override bool IsOneTime => false;

            public void OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => binding.UpdateTarget();

            public void OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => binding.UpdateTarget();

            public void OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
            {
            }
        }
    }
}