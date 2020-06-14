using System;
using System.Windows.Input;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components.Binding
{
    public class EventHandlerBindingComponent : ITargetValueSetterBindingComponent, IAttachableComponent, IDetachableComponent, IEventListener, IHasEventArgsBindingComponent, IHasPriority
    {
        #region Fields

        private EventHandler? _canExecuteHandler;
        private IReadOnlyMetadataContext? _currentMetadata;
        private object? _currentValue;
        private IAccessorMemberInfo? _enabledMember;
        private IWeakReference? _targetRef;
        private ActionToken _unsubscriber;

        #endregion

        #region Constructors

        private EventHandlerBindingComponent(BindingParameterValue commandParameter, bool toggleEnabledState)
        {
            CommandParameter = commandParameter;
            ToggleEnabledState = toggleEnabledState;
        }

        #endregion

        #region Properties

        public static int Priority { get; set; } = BindingComponentPriority.EventHandler;

        int IHasPriority.Priority => Priority;

        public BindingParameterValue CommandParameter { get; private set; }

        public bool ToggleEnabledState { get; }

        public object? EventArgs { get; private set; }

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            var targetMember = ((IBinding)owner).Target.GetLastMember(metadata);
            if (!(targetMember.Member is IObservableMemberInfo eventInfo) || eventInfo.MemberType != MemberType.Event)
                return false;

            _unsubscriber = eventInfo.TryObserve(targetMember.Target, this, metadata);
            if (_unsubscriber.IsEmpty)
                return false;
            return true;
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            ((IBinding)owner).UpdateTarget();
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _unsubscriber.Dispose();
            ClearValue();
            _canExecuteHandler = null;
            CommandParameter.Dispose();
            CommandParameter = default;
        }

        bool IEventListener.TryHandle<T>(object? sender, in T message, IReadOnlyMetadataContext? metadata)
        {
            var components = MugenBindingService.BindingManager.GetComponents<IBindingEventHandlerComponent>(_currentMetadata);
            try
            {
                EventArgs = message;
                components.OnBeginEvent(sender, message, _currentMetadata);
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
                components.OnEventError(e, sender, message, _currentMetadata);
                return true;
            }
            finally
            {
                EventArgs = null;
                components.OnEndEvent(sender, message, _currentMetadata);
            }
        }

        public bool TrySetTargetValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata)
        {
            if (ReferenceEquals(value, _currentValue))
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

        #endregion

        #region Methods

        public static EventHandlerBindingComponent Get(BindingParameterValue commandParameter, bool toggleEnabledState, bool isOneTime)
        {
            if (isOneTime)
                return new EventHandlerBindingComponent(commandParameter, toggleEnabledState);
            return new OneWay(commandParameter, toggleEnabledState);
        }

        private bool InitializeCanExecute(object? target, ICommand command)
        {
            if (target == null)
                return false;
            if (command is ICompositeCommand m && !m.HasCanExecute)
                return false;

            _enabledMember = MugenBindingService.MemberManager
                .GetMember(target.GetType(), MemberType.Accessor, MemberFlags.All & ~(MemberFlags.NonPublic | MemberFlags.Static), BindableMembers.Object.Enabled.Name, _currentMetadata) as IAccessorMemberInfo;
            if (_enabledMember == null || !_enabledMember.CanWrite)
                return false;

            _targetRef = target.ToWeakReference();
            if (_canExecuteHandler == null)
            {
                _canExecuteHandler = MugenExtensions
                    .CreateWeakEventHandler<EventHandlerBindingComponent, EventArgs>(this, (closure, _, __) => closure.OnCanExecuteChanged())
                    .Handle;
            }

            return true;
        }

        private void OnCanExecuteChanged()
        {
            if (!(_currentValue is ICommand cmd))
                return;

            var target = _targetRef?.Target;
            if (target != null)
                SetEnabled(cmd.CanExecute(CommandParameter.GetValue<object?>(_currentMetadata)), target);
        }

        private void SetEnabled(bool value, object? target = null)
        {
            var enabledMember = _enabledMember;
            if (enabledMember == null)
                return;

            if (target == null)
                target = _targetRef?.Target;
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

        #endregion

        #region Nested types

        internal sealed class OneWay : EventHandlerBindingComponent, IBindingSourceObserverListener
        {
            #region Constructors

            public OneWay(BindingParameterValue commandParameter, bool toggleEnabledState)
                : base(commandParameter, toggleEnabledState)
            {
            }

            #endregion

            #region Implementation of interfaces

            public void OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
            {
                binding.UpdateTarget();
            }

            public void OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
            {
                binding.UpdateTarget();
            }

            public void OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
            {
            }

            #endregion
        }

        #endregion
    }
}