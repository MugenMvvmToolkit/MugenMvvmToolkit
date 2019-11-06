using System;
using System.Collections.Generic;
using System.Windows.Input;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class EventTargetValueInterceptorBindingComponent : ITargetValueSetterBindingComponent, IDetachableComponent, IEventListener, IHasEventArgsBindingComponent
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;
        private EventHandler? _canExecuteHandler;
        private IReadOnlyMetadataContext? _currentMetadata;
        private object? _currentValue;
        private IMemberAccessorInfo? _enabledMember;
        private bool _isDisposed;
        private IWeakReference? _targetRef;
        private ActionToken _unsubscriber;

        private static readonly List<object> CurrentEventSources = new List<object>();

        #endregion

        #region Constructors

        public EventTargetValueInterceptorBindingComponent(object? commandParameter, bool toggleEnabledState, IMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
            CommandParameter = commandParameter;
            ToggleEnabledState = toggleEnabledState;
        }

        #endregion

        #region Properties

        public object? CommandParameter { get; }

        public bool ToggleEnabledState { get; }

        bool IEventListener.IsAlive => !_isDisposed;

        bool IEventListener.IsWeak => false;

        public object? EventArgs { get; private set; }

        #endregion

        #region Implementation of interfaces

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            _unsubscriber.Dispose();
            ClearValue();
            _canExecuteHandler = null;
            _isDisposed = true;
        }

        bool IEventListener.TryHandle(object sender, object? message)
        {
            if (_isDisposed)
                return false;

            try
            {
                if (sender != null)
                    CurrentEventSources.Add(sender);
                EventArgs = message;
                switch (_currentValue)
                {
                    case ICommand command:
                        command.Execute(GetCommandParameter());
                        break;
                    case IExpressionValue expression:
                        expression.Invoke(_currentMetadata);
                        break;
                }

                return true;
            }
            finally
            {
                EventArgs = null;
                if (sender != null)
                    CurrentEventSources.Remove(sender);
            }
        }

        public bool TrySetTargetValue(IMemberPathObserver targetObserver, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata)
        {
            if (ReferenceEquals(value, _currentValue))
                return true;

            if (_unsubscriber.IsEmpty)
            {
                if (!(targetMember.Member is IEventInfo eventInfo))
                    return false;

                _unsubscriber = eventInfo.TrySubscribe(targetMember.Target, this, metadata);
                if (_unsubscriber.IsEmpty)
                    return false;
            }

            ClearValue();
            _currentMetadata = metadata;

            if (value == null)
            {
                _unsubscriber.Dispose();
                _unsubscriber = default;
                return true;
            }

            if (value is ICommand command)
            {
                if (ToggleEnabledState && InitializeCanExecute(targetMember.Target, command))
                {
                    OnCanExecuteChanged();
                    command.CanExecuteChanged += _canExecuteHandler;
                }

                _currentValue = value;
                return true;
            }

            if (value is IExpressionValue expressionValue)
            {
                _currentValue = expressionValue;
                return true;
            }

            return false;
        }

        #endregion

        #region Methods

        public static object[] GetCurrentEventSources()
        {
            return CurrentEventSources.ToArray();
        }

        private object? GetCommandParameter()
        {
            if (CommandParameter is IExpressionValue expression)
                return expression.Invoke(_currentMetadata);
            return CommandParameter;
        }

        private bool InitializeCanExecute(object? target, ICommand command)
        {
            if (target == null)
                return false;
            if (command is IMediatorCommand m && !m.HasCanExecute)
                return false;

            _enabledMember = _memberProvider
                    .ServiceIfNull()
                    .GetMember(target.GetType(), BindableMembers.Object.Enabled, MemberType.Property, MemberFlags.InstancePublic, _currentMetadata) as
                IMemberAccessorInfo;
            if (_enabledMember == null)
                return false;

            _targetRef = target.ToWeakReference();
            if (_canExecuteHandler == null)
            {
                _canExecuteHandler = MugenExtensions
                    .CreateWeakEventHandler<EventTargetValueInterceptorBindingComponent, EventArgs>(this, (closure, _, __) => closure.OnCanExecuteChanged())
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
                SetEnabled(cmd.CanExecute(GetCommandParameter()), target);
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
    }
}