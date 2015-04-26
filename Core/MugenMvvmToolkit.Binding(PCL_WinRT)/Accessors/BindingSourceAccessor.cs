#region Copyright

// ****************************************************************************
// <copyright file="BindingSourceAccessor.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Accessors
{
    /// <summary>
    ///     Represents the accessor for the single binding source.
    /// </summary>
    public sealed class BindingSourceAccessor : BindingSourceAccessorBase, ISingleBindingSourceAccessor
    {
        #region Nested types

        private sealed class EventClosure : IEventListener
        {
            #region Fields

            private readonly WeakReference _sourceReference;
            private IDisposable _subscriber;
            private BindingMemberValue _currentValue;
            private object _valueReference;
            private EventHandler _canExecuteHandler;
            public IDataContext LastContext;

            #endregion

            #region Constructors

            public EventClosure(WeakReference sourceReference)
            {
                _sourceReference = sourceReference;
            }

            #endregion

            #region Methods

            public void SetValue(BindingMemberValue currentValue, object newValue)
            {
                //it's normal here.
                lock (this)
                {
                    if (_currentValue != null && Equals(currentValue.Member, _currentValue.Member) &&
                        Equals(_currentValue.MemberSource.Target, currentValue.MemberSource.Target))
                        SetValue(newValue);
                    else
                    {
                        UnsubscribeEventHandler();
                        currentValue.TrySetValue(new object[] { this }, out _subscriber);
                        if (_subscriber != null)
                        {
                            _currentValue = currentValue;
                            SetValue(newValue);
                        }
                    }
                }
            }

            public void Unsubscribe(bool dispose)
            {
                if (_subscriber == null)
                    return;
                LastContext = null;
                //it's normal here.
                lock (this)
                {
                    if (UnsubscribeEventHandler())
                        UnsubscribeCommand();
                    ClearValueReference();
                }
                if (dispose)
                    _canExecuteHandler = null;
            }

            private void ClearValueReference()
            {
                _valueReference = null;
            }

            private void SetValue(object newValue)
            {
                var accessor = (BindingSourceAccessor)_sourceReference.Target;
                if (accessor == null || newValue == null)
                {
                    UnsubscribeCommand();
                    ClearValueReference();
                    return;
                }
                var command = newValue as ICommand;
                if (command == null)
                {
                    if (!(newValue is BindingMemberValue))
                        throw BindingExceptionManager.InvalidEventSourceValue(_currentValue.Member, newValue);
                    _valueReference = newValue;
                }
                else
                {
                    var reference = _valueReference as WeakReference;
                    if (reference == null || !ReferenceEquals(reference.Target, command))
                        _valueReference = ToolkitExtensions.GetWeakReferenceOrDefault(command, null, true);
                    if (accessor._toggleEnabledState && accessor.BindingTarget != null &&
                        InitializeCanExecuteDelegate(command))
                    {
                        accessor.CommandOnCanExecuteChanged(command, LastContext);
                        command.CanExecuteChanged += _canExecuteHandler;
                    }
                }
            }

            private void CommandOnCanExecuteChanged()
            {
                var target = (BindingSourceAccessor)_sourceReference.Target;
                if (target == null)
                    UnsubscribeEventHandler();
                else
                {
                    var command = GetReferenceValue() as ICommand;
                    if (command != null)
                        target.CommandOnCanExecuteChanged(command, LastContext);
                }
            }

            private void UnsubscribeCommand()
            {
                var command = GetReferenceValue() as ICommand;
                if (command == null)
                    return;
                if (_canExecuteHandler == null)
                    return;
                command.CanExecuteChanged -= _canExecuteHandler;

                var accessor = (BindingSourceAccessor)_sourceReference.Target;
                if (accessor == null)
                    return;

                var cmdSource = accessor.BindingTarget;
                if (cmdSource != null)
                    cmdSource.IsEnabled = true;
            }

            private bool UnsubscribeEventHandler()
            {
                IDisposable unsubscriber = _subscriber;
                if (unsubscriber == null)
                    return false;
                _subscriber = null;
                unsubscriber.Dispose();
                return true;
            }

            private object GetReferenceValue()
            {
                var valueReference = _valueReference;
                if (valueReference == null)
                    return null;
                var memberValue = valueReference as BindingMemberValue;
                if (memberValue != null)
                    return memberValue;
                var reference = valueReference as WeakReference;
                if (reference == null)
                    return null;
                return reference.Target;
            }

            private bool InitializeCanExecuteDelegate(ICommand command)
            {
                if (_canExecuteHandler == null)
                {
                    var relayCommand = command as IRelayCommand;
                    if (relayCommand == null || relayCommand.HasCanExecuteImpl)
                        Interlocked.CompareExchange(ref _canExecuteHandler, ReflectionExtensions
                            .CreateWeakEventHandler<EventClosure, EventArgs>(this, (closure, o, arg3) => closure.CommandOnCanExecuteChanged()).Handle, null);
                }
                return _canExecuteHandler != null;
            }

            #endregion

            #region Implementation of IEventListener

            public bool IsAlive
            {
                get { return _sourceReference.Target != null; }
            }

            public bool IsWeak
            {
                get { return true; }
            }

            public bool TryHandle(object sender, object message)
            {
                var target = (BindingSourceAccessor)_sourceReference.Target;
                if (target == null)
                {
                    UnsubscribeEventHandler();
                    return false;
                }

                var value = GetReferenceValue();
                LastContext.AddOrUpdate(BindingConstants.CurrentEventArgs, message);
                target.OnEventImpl(value, message, LastContext);
                return true;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly bool _toggleEnabledState;
        private IBindingSource _bindingSource;
        private EventClosure _closure;

        #endregion

        #region Constructors

        static BindingSourceAccessor()
        {
            ToggleEnabledStateDefault = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingSourceAccessor" /> class.
        /// </summary>
        public BindingSourceAccessor([NotNull]IBindingSource bindingSource, [NotNull] IDataContext context, bool isTarget)
            : base(context, isTarget)
        {
            Should.NotBeNull(bindingSource, "bindingSource");
            _bindingSource = bindingSource;
            if (!context.TryGetData(BindingBuilderConstants.ToggleEnabledState, out _toggleEnabledState))
                _toggleEnabledState = ToggleEnabledStateDefault;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the <see cref="IBindingTarget" />, if any.
        /// </summary>
        private IBindingTarget BindingTarget
        {
            get { return _bindingSource as IBindingTarget; }
        }

        /// <summary>
        ///     Gets or sets the property that is responsible for the automatic toggle enabled state for command.
        /// </summary>
        public static bool ToggleEnabledStateDefault { get; set; }

        #endregion

        #region Overrides of BindingSourceAccessorBase

        /// <summary>
        ///     Gets the underlying source.
        /// </summary>
        public IBindingSource Source
        {
            get { return _bindingSource; }
        }

        /// <summary>
        ///     Gets the underlying sources.
        /// </summary>
        public override IList<IBindingSource> Sources
        {
            get
            {
                if (_bindingSource == null)
                    return Empty.Array<IBindingSource>();
                return new[] { _bindingSource };
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (_closure != null)
                _closure.Unsubscribe(true);
            _bindingSource.Dispose();
            _bindingSource = null;
            ValueChanging = null;
            ValueChanged = null;
            _closure = null;
            base.Dispose();
        }

        /// <summary>
        ///     Occurs before the value changes.
        /// </summary>
        public override event EventHandler<IBindingSourceAccessor, ValueAccessorChangingEventArgs> ValueChanging;

        /// <summary>
        ///     Occurs when value changed.
        /// </summary>
        public override event EventHandler<IBindingSourceAccessor, ValueAccessorChangedEventArgs> ValueChanged;

        /// <summary>
        ///     Gets the raw value from source.
        /// </summary>
        protected override object GetRawValueInternal(IBindingMemberInfo targetMember, IDataContext context,
            bool throwOnError)
        {
            IBindingPathMembers members = _bindingSource.GetPathMembers(throwOnError);
            return members.LastMember.GetValue(members.PenultimateValue, null);
        }

        /// <summary>
        ///     Sets the source value.
        /// </summary>
        /// <param name="targetAccessor">The specified accessor to get value.</param>
        /// <param name="context">The specified operation context.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the value cannot be set.
        /// </param>
        protected override bool SetValueInternal(IBindingSourceAccessor targetAccessor, IDataContext context,
            bool throwOnError)
        {
            IBindingPathMembers members = _bindingSource.GetPathMembers(throwOnError);
            if (!members.AllMembersAvailable)
                return false;

            object penultimateValue = members.PenultimateValue;
            IBindingMemberInfo lastMember = members.LastMember;

            object oldValue;
            object newValue = targetAccessor.GetValue(lastMember, context, throwOnError);
            if (lastMember.CanRead)
            {
                oldValue = lastMember.GetValue(penultimateValue, null);
                if (ReferenceEquals(oldValue, newValue) || newValue.IsUnsetValueOrDoNothing())
                    return false;
            }
            else
            {
                oldValue = BindingConstants.UnsetValue;
                if (newValue.IsUnsetValueOrDoNothing())
                    return false;
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
                            return false;
                    }
                }
            }
            newValue = BindingServiceProvider.ValueConverter(lastMember, lastMember.Type, newValue);
            if (Equals(oldValue, newValue))
                return false;
            if (lastMember.MemberType == BindingMemberType.Event)
            {
                TryRegisterEvent((BindingMemberValue)oldValue, newValue, context);
                RaiseValueChanged(context, penultimateValue, lastMember, oldValue, newValue, args);
            }
            else
            {
                if (_closure != null)
                    _closure.Unsubscribe(false);
                lastMember.SetValue(penultimateValue, new[] { newValue });
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

        private void TryRegisterEvent(BindingMemberValue bindingMemberValue, object newValue, IDataContext context)
        {
            if (newValue == null && _closure == null)
                return;
            if (_closure == null)
                Interlocked.CompareExchange(ref _closure, new EventClosure(ServiceProvider.WeakReferenceFactory(this, true)), null);
            _closure.LastContext = context;
            _closure.SetValue(bindingMemberValue, newValue);
        }

        private void CommandOnCanExecuteChanged([NotNull] ICommand command, IDataContext context)
        {
            var cmdSource = BindingTarget;
            if (cmdSource != null)
                cmdSource.IsEnabled = command.CanExecute(GetCommandParameter(context));
        }

        private void OnEventImpl(object target, object eventArgs, IDataContext context)
        {
            if (target == null)
                return;

            var command = target as ICommand;
            if (command != null)
            {
                command.Execute(GetCommandParameter(context));
                return;
            }

            var memberValue = target as BindingMemberValue;
            if (memberValue != null)
                memberValue.TrySetValue(new[] { GetCommandParameter(context), eventArgs, context }, out target);
        }

        private object GetCommandParameter(IDataContext context)
        {
            var target = BindingTarget;
            if (target == null)
                return null;
            var param = target.GetCommandParameter(context);
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

        #endregion
    }
}