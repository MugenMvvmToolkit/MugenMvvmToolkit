#region Copyright

// ****************************************************************************
// <copyright file="BindingSourceAccessor.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Accessors
{
    public sealed class BindingSourceAccessor : BindingSourceAccessorBase, ISingleBindingSourceAccessor
    {
        #region Nested types

        private sealed class EventClosure : IEventListener
        {
            #region Fields

            private object _source;
            private readonly bool _toggleEnabledState;
            private readonly Func<IDataContext, object> _parameterDelegate;
            private IDisposable _subscriber;
            private BindingActionValue _currentValue;
            private object _valueReference;
            private EventHandler _canExecuteHandler;
            public IDataContext LastContext;
            private IBindingPath _path;

            #endregion

            #region Constructors

            public EventClosure(IObserver source, bool toggleEnabledState, Func<IDataContext, object> parameterDelegate)
            {
                _source = source;
                _toggleEnabledState = toggleEnabledState;
                _parameterDelegate = parameterDelegate;
            }

            #endregion

            #region Methods

            public void SetValue(BindingActionValue currentValue, object newValue, IBindingPathMembers pathMembers)
            {
                _path = pathMembers.Path;
                //it's normal here.
                lock (this)
                {
                    if (_currentValue != null && Equals(currentValue.Member, _currentValue.Member) &&
                        Equals(_currentValue.MemberSource.Target, currentValue.MemberSource.Target))
                        SetValue(newValue, pathMembers);
                    else
                    {
                        UnsubscribeEventHandler();
                        currentValue.TrySetValue(new object[] { this }, out _subscriber);
                        if (_subscriber != null)
                        {
                            if (_path.IsDebuggable)
                                DebugInfo($"Binding subscribed to event: '{pathMembers.LastMember.Path}'", new object[] { pathMembers });
                            _currentValue = currentValue;
                            SetValue(newValue, pathMembers);
                        }
                    }
                }
            }

            public void Unsubscribe(bool dispose, bool oneTime)
            {
                if (_subscriber == null)
                    return;
                if (dispose && oneTime && GetReferenceValue() is ICommand)
                {
                    var penultimateValue = GetPenultimateValue();
                    if (!penultimateValue.IsNullOrUnsetValue())
                    {
                        LastContext = LastContext == null ? new DataContext() : new DataContext(LastContext);
                        LastContext.Remove(BindingConstants.Binding);
                        _source = ToolkitExtensions.GetWeakReference(penultimateValue);
                        return;
                    }
                }

                LastContext = null;
                //it's normal here.
                lock (this)
                {
                    UnsubscribeEventHandler();
                    UnsubscribeCommand();
                    _valueReference = null;
                }
                if (dispose)
                    _canExecuteHandler = null;
            }

            private void DebugInfo(string message, object[] args = null)
            {
                BindingServiceProvider.DebugBinding(this, _path.DebugTag, message, args);
            }

            private void SetValue(object newValue, IBindingPathMembers pathMembers)
            {
                if (newValue == null)
                {
                    UnsubscribeCommand();
                    _valueReference = null;
                    return;
                }

                var command = newValue as ICommand;
                if (command == null)
                {
                    if (!(newValue is BindingActionValue))
                        throw BindingExceptionManager.InvalidEventSourceValue(_currentValue.Member, newValue);
                    _valueReference = newValue;
                    if (_path.IsDebuggable)
                        DebugInfo($"Binding will use event: '{pathMembers.LastMember.Path}' to update member: '{((BindingActionValue)newValue).Member.Path}'", new[] { newValue });
                }
                else
                {
                    var reference = _valueReference as WeakReference;
                    if (reference != null && ReferenceEquals(reference.Target, command))
                        return;
                    if (_path.IsDebuggable)
                        DebugInfo($"Binding will use event: '{pathMembers.LastMember.Path}' to update command: '{newValue}'", new[] { newValue });
                    UnsubscribeCommand();
                    _valueReference = ToolkitExtensions.GetWeakReferenceOrDefault(command, null, true);
                    if (_toggleEnabledState && InitializeCanExecuteDelegate(command))
                    {
                        CommandOnCanExecuteChanged(command);
                        command.CanExecuteChanged += _canExecuteHandler;
                    }
                }
            }

            private void UnsubscribeCommand()
            {
                var command = GetReferenceValue() as ICommand;
                if (command == null)
                    return;
                var handler = _canExecuteHandler;
                if (handler != null)
                {
                    command.CanExecuteChanged -= handler;
                    SetIsEnabled(true);
                }
            }

            private void UnsubscribeEventHandler()
            {
                IDisposable unsubscriber = _subscriber;
                if (unsubscriber == null)
                    return;
                Interlocked.CompareExchange(ref _subscriber, null, unsubscriber);
                unsubscriber.Dispose();
            }

            private object GetReferenceValue()
            {
                var valueReference = _valueReference;
                if (valueReference == null)
                    return null;
                var actionValue = valueReference as BindingActionValue;
                if (actionValue != null)
                    return actionValue;
                return (valueReference as WeakReference)?.Target;
            }

            private bool InitializeCanExecuteDelegate(ICommand command)
            {
                if (_canExecuteHandler == null)
                {
                    var relayCommand = command as IRelayCommand;
                    if (relayCommand == null || relayCommand.HasCanExecuteImpl)
                        Interlocked.CompareExchange(ref _canExecuteHandler, ReflectionExtensions
                            .CreateWeakEventHandler<EventClosure, EventArgs>(this, (closure, o, arg3) => closure.CommandOnCanExecuteChanged(o)).Handle, null);
                }
                return _canExecuteHandler != null;
            }

            private void SetIsEnabled(bool value)
            {
                object penultimateValue = GetPenultimateValue();
                if (penultimateValue.IsNullOrUnsetValue())
                    return;
                IBindingMemberInfo member = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(penultimateValue.GetType(), AttachedMemberConstants.Enabled, false, false);
                if (member == null)
                    Tracer.Warn("The member {0} cannot be obtained on type {1}", AttachedMemberConstants.Enabled, penultimateValue.GetType());
                else
                {
                    member.SetSingleValue(penultimateValue, Empty.BooleanToObject(value));
                    if (_path.IsDebuggable)
                        DebugInfo($"Binding changed enabled state to '{value}' for source: '{penultimateValue}'");
                }
            }

            private object GetCommandParameterFromSource(IDataContext context)
            {
                if (_parameterDelegate != null)
                    return _parameterDelegate(context);

                object target = GetPenultimateValue();
                if (target.IsNullOrUnsetValue())
                    return null;

                return BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(target.GetType(), AttachedMemberConstants.CommandParameter, false, false)
                    ?.GetValue(target, new object[] { context });
            }

            private void CommandOnCanExecuteChanged(object sender)
            {
                var command = sender as ICommand ?? GetReferenceValue() as ICommand;
                if (command != null)
                    SetIsEnabled(command.CanExecute(GetCommandParameter(LastContext)));
            }

            private object GetCommandParameter(IDataContext context)
            {
                var param = GetCommandParameterFromSource(context);
                var path = param as string;
                if (string.IsNullOrEmpty(path) ||
                    (!path.StartsWith("$args.", StringComparison.Ordinal) &&
                     !path.StartsWith("$arg.", StringComparison.Ordinal)))
                    return param;
                var args = context.GetData(BindingConstants.CurrentEventArgs);
                if (args == null)
                    return null;
                return BindingExtensions.GetValueFromPath(args, path, 1);
            }

            private object GetPenultimateValue()
            {
                var observer = _source as IObserver;
                if (observer == null)
                    return ((WeakReference)_source).Target;
                return observer.GetPathMembers(false).PenultimateValue;
            }

            #endregion

            #region Implementation of IEventListener

            public bool IsAlive
            {
                get
                {
                    var observer = _source as IObserver;
                    if (observer == null)
                        return ((WeakReference)_source).Target != null;
                    return observer.IsAlive;
                }
            }

            public bool IsWeak => true;

            public bool TryHandle(object sender, object message)
            {
                var target = GetReferenceValue();
                if (target == null)
                {
                    UnsubscribeEventHandler();
                    return false;
                }
                LastContext.AddOrUpdate(BindingConstants.CurrentEventArgs, message);

                WeakReference reference = null;
                var command = target as ICommand;
                if (command != null)
                {
                    if (sender != null)
                    {
                        reference = ToolkitServiceProvider.WeakReferenceFactory(sender);
                        ToolkitExtensions.AddCurrentBindingEventSender(reference);
                    }
                    if (_path.IsDebuggable)
                    {
                        var parameter = GetCommandParameter(LastContext);
                        DebugInfo($"Binding invokes command '{command}' with parameter: '{parameter}', event args: '{message}'", new[] { command, parameter, message });
                        command.Execute(parameter);
                    }
                    else
                        command.Execute(GetCommandParameter(LastContext));
                    if (reference != null)
                        ToolkitExtensions.RemoveCurrentBindingEventSender(reference);
                    return true;
                }
                var actionValue = target as BindingActionValue;
                if (actionValue == null || actionValue.MemberSource.Target == null)
                {
                    UnsubscribeEventHandler();
                    return false;
                }
                                
                if (sender != null)
                {
                    reference = ToolkitServiceProvider.WeakReferenceFactory(sender);
                    ToolkitExtensions.AddCurrentBindingEventSender(reference);
                }
                if (_path.IsDebuggable)
                {
                    var args = new[] { GetCommandParameter(LastContext), message, LastContext };
                    DebugInfo($"Binding invokes member '{actionValue.Member.Path}' with parameter: '{args[0]}', event args: '{message}'", args);
                    actionValue.TrySetValue(args, out target);
                }
                else
                    actionValue.TrySetValue(new[] { GetCommandParameter(LastContext), message, LastContext }, out target);
                if (reference != null)
                    ToolkitExtensions.RemoveCurrentBindingEventSender(reference);
                return true;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly bool _toggleEnabledState;
        private readonly bool _isOneTime;
        private readonly IObserver _bindingSource;
        private EventClosure _closure;
        private bool _disableEqualityChecking;

        #endregion

        #region Constructors

        public BindingSourceAccessor([NotNull]IObserver bindingSource, [NotNull] IDataContext context, bool isTarget)
            : base(context, isTarget)
        {
            Should.NotBeNull(bindingSource, nameof(bindingSource));
            _bindingSource = bindingSource;
            if (isTarget)
            {
                if (!context.TryGetData(BindingBuilderConstants.ToggleEnabledState, out _toggleEnabledState))
                    _toggleEnabledState = true;
                List<IBindingBehavior> data;
                if (context.TryGetData(BindingBuilderConstants.Behaviors, out data))
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (data[i] is OneTimeBindingMode)
                        {
                            _isOneTime = true;
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Overrides of BindingSourceAccessorBase

        public IObserver Source => _bindingSource;

        public override bool CanRead => true;

        public override bool CanWrite => true;

        protected override bool IsDebuggable => _bindingSource.Path.IsDebuggable;

        protected override string DebugTag => _bindingSource.Path.DebugTag;

        public override bool DisableEqualityChecking
        {
            get { return _disableEqualityChecking; }
            set { _disableEqualityChecking = value; }
        }

        public override IList<IObserver> Sources
        {
            get
            {
                if (_bindingSource == null)
                    return Empty.Array<IObserver>();
                return new[] { _bindingSource };
            }
        }

        public override void Dispose()
        {
            _closure?.Unsubscribe(true, _isOneTime);
            _bindingSource.Dispose();
            ValueChanging = null;
            ValueChanged = null;
            _closure = null;
            base.Dispose();
        }

        public override event EventHandler<IBindingSourceAccessor, ValueAccessorChangingEventArgs> ValueChanging;

        public override event EventHandler<IBindingSourceAccessor, ValueAccessorChangedEventArgs> ValueChanged;

        protected override object GetRawValueInternal(IBindingMemberInfo targetMember, IDataContext context,
            bool throwOnError)
        {
            IBindingPathMembers members = _bindingSource.GetPathMembers(throwOnError);
            var value = members.GetLastMemberValue();
            if (members.Path.IsDebuggable)
                DebugInfo($"Binding got a raw value: '{value}', for path: '{members.Path}'", new[] { value, members });
            return value;
        }

        protected override bool SetValueInternal(IBindingSourceAccessor targetAccessor, IDataContext context,
            bool throwOnError)
        {
            IBindingPathMembers members = _bindingSource.GetPathMembers(throwOnError);
            object penultimateValue = members.PenultimateValue;
            if (penultimateValue.IsUnsetValue() || (penultimateValue == null && !members.AllMembersAvailable))
            {
                if (members.Path.IsDebuggable)
                    DebugInfo($"Binding cannot set value for path {members.Path.Path}", new object[] { members });
                return false;
            }

            IBindingMemberInfo lastMember = members.LastMember;
            object oldValue;
            object newValue = targetAccessor.GetValue(lastMember, context, throwOnError);
            if (lastMember.CanRead && !BindingMemberType.BindingContext.EqualsWithoutNullCheck(lastMember.MemberType))
            {
                if (_disableEqualityChecking && !BindingMemberType.Event.EqualsWithoutNullCheck(lastMember.MemberType))
                    oldValue = BindingConstants.UnsetValue;
                else
                    oldValue = lastMember.GetValue(penultimateValue, null);
                if (ReferenceEquals(oldValue, newValue) || newValue.IsUnsetValueOrDoNothing())
                {
                    if (members.Path.IsDebuggable)
                        DebugInfo($"Binding ignores setter because old value: '{oldValue}' equals to new value '{newValue}'", new[] { members, oldValue, newValue });
                    return false;
                }
            }
            else
            {
                oldValue = BindingConstants.UnsetValue;
                if (newValue.IsUnsetValueOrDoNothing())
                {
                    if (members.Path.IsDebuggable)
                        DebugInfo($"Binding ignores setter for value '{newValue}'", new[] { members, newValue });
                    return false;
                }
            }

            ValueAccessorChangingEventArgs args = null;
            if (ValueChanging != null)
            {
                args = RaiseValueChanging(context, penultimateValue, lastMember, oldValue, newValue);
                if (args != null)
                {
                    if (args.Cancel)
                        return false;
                    if (!ReferenceEquals(newValue, args.NewValue))
                    {
                        newValue = args.NewValue;
                        if (newValue.IsUnsetValueOrDoNothing())
                        {
                            if (members.Path.IsDebuggable)
                                DebugInfo($"Binding ignores setter for value '{newValue}'", new[] { members, newValue });
                            return false;
                        }
                    }
                }
            }
            newValue = BindingServiceProvider.ValueConverter(lastMember, lastMember.Type, newValue);
            if (Equals(oldValue, newValue))
            {
                if (members.Path.IsDebuggable)
                    DebugInfo($"Binding ignores setter because old value: '{oldValue}' equals to new value '{newValue}'", new[] { members, oldValue, newValue });
                return false;
            }
            if (BindingMemberType.Event.EqualsWithoutNullCheck(lastMember.MemberType))
            {
                TryRegisterEvent((BindingActionValue)oldValue, newValue, context, members);
                RaiseValueChanged(context, penultimateValue, lastMember, oldValue, newValue, args);
            }
            else
            {
                _closure?.Unsubscribe(false, _isOneTime);
                lastMember.SetSingleValue(penultimateValue, newValue);
                if (members.Path.IsDebuggable)
                    DebugInfo($"Binding set value: '{newValue}' for source: '{penultimateValue}' with path: '{lastMember.Path}'", new[] { newValue, penultimateValue, lastMember });
                if (ValueChanged != null)
                    RaiseValueChanged(context, penultimateValue, lastMember, oldValue, newValue, args);
            }
            return true;
        }

        #endregion

        #region Methods

        private ValueAccessorChangingEventArgs RaiseValueChanging(IDataContext context, object penultimateValue,
            IBindingMemberInfo lastMember, object oldValue, object newValue)
        {
            var valueChanging = ValueChanging;
            if (valueChanging == null)
                return null;
            var args = new ValueAccessorChangingEventArgs(context, penultimateValue, lastMember, oldValue, newValue);
            valueChanging(this, args);
            return args;
        }

        private void RaiseValueChanged(IDataContext context, object penultimateValue, IBindingMemberInfo lastMember,
            object oldValue, object newValue, ValueAccessorChangedEventArgs args)
        {
            var valueChanged = ValueChanged;
            if (valueChanged == null)
                return;
            if (args == null)
                args = new ValueAccessorChangedEventArgs(context, penultimateValue, lastMember, oldValue, newValue);
            valueChanged(this, args);
        }

        private void TryRegisterEvent(BindingActionValue bindingActionValue, object newValue, IDataContext context, IBindingPathMembers pathMembers)
        {
            if (newValue == null && _closure == null)
                return;
            if (_closure == null)
                Interlocked.CompareExchange(ref _closure, new EventClosure(_bindingSource, _toggleEnabledState && IsTarget, Parameters?.CommandParameterDelegate), null);
            _closure.LastContext = context;
            _closure.SetValue(bindingActionValue, newValue, pathMembers);
        }

        #endregion
    }
}
