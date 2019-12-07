using System;
using System.Windows.Input;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
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

namespace MugenMvvm.Binding.Core.Components.Binding
{
    public sealed class EventTargetValueInterceptorBindingComponent : ITargetValueSetterBindingComponent, IAttachableComponent,
        IDetachableComponent, IEventListener, IHasEventArgsBindingComponent, IHasPriority
    {
        #region Fields

        private readonly IBindingManager? _bindingManager;
        private EventHandler? _canExecuteHandler;
        private IReadOnlyMetadataContext? _currentMetadata;
        private object? _currentValue;
        private IMemberAccessorInfo? _enabledMember;
        private bool _isDisposed;
        private IWeakReference? _targetRef;
        private ActionToken _unsubscriber;

        #endregion

        #region Constructors

        public EventTargetValueInterceptorBindingComponent(BindingParameterValue commandParameter, bool toggleEnabledState, IBindingManager? bindingManager = null)
        {
            _bindingManager = bindingManager;
            CommandParameter = commandParameter;
            ToggleEnabledState = toggleEnabledState;
        }

        #endregion

        #region Properties

        public static int Priority { get; set; } = BindingComponentPriority.EventHandler;

        int IHasPriority.Priority => Priority;

        public BindingParameterValue CommandParameter { get; private set; }

        public bool ToggleEnabledState { get; }

        bool IWeakItem.IsAlive => !_isDisposed;

        bool IEventListener.IsWeak => false;

        public object? EventArgs { get; private set; }

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            var targetMember = ((IBinding)owner).Target.GetLastMember();
            if (!(targetMember.Member is IEventInfo eventInfo))
                return false;

            _unsubscriber = eventInfo.TrySubscribe(targetMember.Target, this, metadata);
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
            _isDisposed = true;
        }

        bool IEventListener.TryHandle(object sender, object? message)
        {
            if (_isDisposed)
                return false;

            try
            {
                OnBeginEvent(sender, message);
                EventArgs = message;
                switch (_currentValue)
                {
                    case ICommand command:
                        command.Execute(CommandParameter.GetValue<object?>(_currentMetadata));
                        return true;
                    case IExpressionValue expression:
                        expression.Invoke(_currentMetadata);
                        return true;
                }

                return true;
            }
            catch (Exception e)
            {
                OnEventError(e, sender, message);
                return true;
            }
            finally
            {
                EventArgs = null;
                OnEndEvent(sender, message);
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
                if (ToggleEnabledState && InitializeCanExecute(targetMember.Target, command))
                {
                    OnCanExecuteChanged();
                    command.CanExecuteChanged += _canExecuteHandler;
                }

                _currentValue = value;
            }
            else if (value is IExpressionValue)
                _currentValue = value;

            return true;
        }

        #endregion

        #region Methods

        private bool InitializeCanExecute(object? target, ICommand command)
        {
            if (target == null)
                return false;
            if (command is IMediatorCommand m && !m.HasCanExecute)
                return false;

            _enabledMember = GetMemberProvider()
                .GetMember(target.GetType(), BindableMembers.Object.Enabled, MemberType.Accessor, MemberFlags.Public | MemberFlags.Extension | MemberFlags.Dynamic, _currentMetadata) as IMemberAccessorInfo;
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

        private void OnBeginEvent(object sender, object? message)
        {
            var components = _bindingManager.DefaultIfNull().Components.Get<IBindingEventHandlerComponent>(_currentMetadata);
            for (var i = 0; i < components.Length; i++)
                components[i].OnBeginEvent(sender, message, _currentMetadata);
        }

        private void OnEndEvent(object sender, object? message)
        {
            var components = _bindingManager.DefaultIfNull().Components.Get<IBindingEventHandlerComponent>(_currentMetadata);
            for (var i = 0; i < components.Length; i++)
                components[i].OnEndEvent(sender, message, _currentMetadata);
        }

        private void OnEventError(Exception exception, object sender, object? message)
        {
            var components = _bindingManager.DefaultIfNull().Components.Get<IBindingEventHandlerComponent>(_currentMetadata);
            for (var i = 0; i < components.Length; i++)
                components[i].OnEventError(exception, sender, message, _currentMetadata);
        }

        private IMemberProvider GetMemberProvider()
        {
            if (_bindingManager == null)
                return MugenBindingService.MemberProvider;
            return _bindingManager.GetComponentOptional<IMemberProvider>(_currentMetadata).DefaultIfNull();
        }

        #endregion
    }
}