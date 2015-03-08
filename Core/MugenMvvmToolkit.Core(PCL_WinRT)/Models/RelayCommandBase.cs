#region Copyright

// ****************************************************************************
// <copyright file="RelayCommandBase.cs">
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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     An extension to ICommand to provide an ability to raise changed events.
    /// </summary>
    public abstract class RelayCommandBase : NotifyPropertyChangedBase, IRelayCommand, IHandler<IBroadcastMessage>
    {
        #region Fields

        private static readonly Action<RelayCommandBase, EventArgs> RaiseCanExecuteChangedDelegate;
        private static readonly Action<RelayCommandBase, object, PropertyChangedEventArgs> OnPropertyChangedDelegate;
        private static readonly List<KeyValuePair<WeakReference, Action<RelayCommandBase, object>>> DisposedFlag;

        private PropertyChangedEventHandler _weakHandler;
        private List<KeyValuePair<WeakReference, Action<RelayCommandBase, object>>> _notifiers;
        private EventHandler _canExecuteChangedInternal;

        #endregion

        #region Constructors

        static RelayCommandBase()
        {
            DisposedFlag = new List<KeyValuePair<WeakReference, Action<RelayCommandBase, object>>>();
            RaiseCanExecuteChangedDelegate = RaiseCanExecuteChangedStatic;
            OnPropertyChangedDelegate = OnPropertyChangedStatic;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommandBase" /> class.
        /// </summary>
        protected RelayCommandBase(bool hasCanExecuteImpl)
        {
            if (hasCanExecuteImpl)
            {
                _notifiers = new List<KeyValuePair<WeakReference, Action<RelayCommandBase, object>>>(2);
                _weakHandler = ReflectionExtensions.MakeWeakPropertyChangedHandler(this, OnPropertyChangedDelegate);
            }
            ExecutionMode = ApplicationSettings.CommandExecutionMode;
            CanExecuteMode = ApplicationSettings.CommandCanExecuteMode;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommandBase" /> class.
        /// </summary>
        protected RelayCommandBase(bool hasCanExecuteImpl, params object[] notifiers)
            : this(hasCanExecuteImpl)
        {
            if (hasCanExecuteImpl || notifiers != null)
            {
                for (int index = 0; index < notifiers.Length; index++)
                    AddNotifier(notifiers[index]);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommandBase" /> class.
        /// </summary>
        protected RelayCommandBase(params object[] notifiers)
            : this(true, notifiers)
        {

        }

        #endregion

        #region Properties

        /// <summary>
        ///     Indicates that task-based command allows multiple execution default is false.
        /// </summary>
        public static bool AllowMultipleExecutionDefault { get; set; }

        /// <summary>
        ///     Gets or sets the value, if <c>true</c> execute asynchronously; otherwise <c>false</c> - synchronously.
        ///     Default value is false.
        /// </summary>
        public bool ExecuteAsynchronously { get; set; }

        #endregion

        #region Implementation of IRelayCommand

        /// <summary>
        ///     Gets the value that indicates that command has can execute handler.
        /// </summary>
        public bool HasCanExecuteImpl
        {
            get { return _weakHandler != null; }
        }

        /// <summary>
        ///     Specifies the execution mode for <c>Execute</c> method.
        /// </summary>
        public CommandExecutionMode ExecutionMode { get; set; }

        /// <summary>
        ///     Specifies the execution mode for <c>RaiseCanExecuteChanged</c> method in <c>IRelayCommand</c>.
        /// </summary>
        public ExecutionMode CanExecuteMode { get; set; }

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        ///     true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public bool CanExecute(object parameter)
        {
            return !HasCanExecuteImpl || CanExecuteInternal(parameter);
        }

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            switch (ExecutionMode)
            {
                case CommandExecutionMode.CanExecuteBeforeExecute:
                    if (!CanExecute(parameter))
                    {
                        Tracer.Warn(ExceptionManager.CommandCannotBeExecutedString);
                        RaiseCanExecuteChanged();
                        return;
                    }
                    break;
                case CommandExecutionMode.CanExecuteBeforeExecuteWithException:
                    if (!CanExecute(parameter))
                        throw ExceptionManager.CommandCannotBeExecuted();
                    break;
            }
            if (ExecuteAsynchronously)
                ThreadManager.Invoke(Models.ExecutionMode.Asynchronous, this, parameter,
                    (@base, o) => @base.ExecuteInternal(o));
            else
                ExecuteInternal(parameter);
        }

        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (!HasCanExecuteImpl)
                    return;
                var action = ApplicationSettings.AddCanExecuteChangedEvent;
                if (action != null)
                    action(this, value);
                _canExecuteChangedInternal += value;
            }
            remove
            {
                if (!HasCanExecuteImpl)
                    return;
                var action = ApplicationSettings.RemoveCanExecuteChangedEvent;
                if (action != null)
                    action(this, value);
                _canExecuteChangedInternal -= value;
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var notifiers = Interlocked.Exchange(ref _notifiers, DisposedFlag);
            if (notifiers == DisposedFlag)
                return;
            if (notifiers != null)
                ClearInternal(notifiers);
            _weakHandler = null;
            _canExecuteChangedInternal = null;
            OnDispose();
            ServiceProvider.AttachedValueProvider.Clear(this);
        }

        /// <summary>
        ///     This method can be used to raise the CanExecuteChanged handler.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if (IsNotificationsSuspended)
            {
                IsNotificationsDirty = true;
                return;
            }
            if (_canExecuteChangedInternal != null)
                ThreadManager.Invoke(CanExecuteMode, this, EventArgs.Empty, RaiseCanExecuteChangedDelegate);
        }

        /// <summary>
        ///     Gets the current command notifiers.
        /// </summary>
        public IList<object> GetNotifiers()
        {
            if (!HasCanExecuteImpl)
                return Empty.Array<object>();
            var objects = new List<object>();
            lock (_notifiers)
            {
                for (int i = 0; i < _notifiers.Count; i++)
                {
                    var target = _notifiers[i].Key.Target;
                    if (target == null)
                    {
                        _notifiers.RemoveAt(i);
                        i--;
                    }
                    else
                        objects.Add(target);
                }
            }
            return objects;
        }

        /// <summary>
        ///     Adds the specified notifier to manage the <c>CanExecuteChanged</c> event.
        /// </summary>
        /// <param name="item">The specified notifier item.</param>
        public bool AddNotifier(object item)
        {
            Should.NotBeNull(item, "item");
            if (!HasCanExecuteImpl)
                return false;
            lock (_notifiers)
            {
                if (Contains(item, false))
                    return true;
                Action<RelayCommandBase, object> notifier = CreateNotifier(item);
                if (notifier == null)
                    return false;
                _notifiers.Add(new KeyValuePair<WeakReference, Action<RelayCommandBase, object>>(ToolkitExtensions.GetWeakReference(item), notifier));
                return true;
            }
        }

        /// <summary>
        ///     Removes the specified notifier.
        /// </summary>
        /// <param name="item">The specified notifier item.</param>
        public bool RemoveNotifier(object item)
        {
            Should.NotBeNull(item, "item");
            if (!HasCanExecuteImpl)
                return false;
            lock (_notifiers)
                return Contains(item, true);
        }

        /// <summary>
        ///     Removes all notifiers.
        /// </summary>
        public void ClearNotifiers()
        {
            if (HasCanExecuteImpl)
                ClearInternal(_notifiers);
        }

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        void IHandler<IBroadcastMessage>.Handle(object sender, IBroadcastMessage message)
        {
            RaiseCanExecuteChanged();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Factory method to create a new instance of <see cref="IRelayCommand" /> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">
        ///     Delegate to execute when Execute is called on the command. This can be null to just hook up
        ///     a CanExecute delegate.
        /// </param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <param name="allowMultipleExecution">Indicates that command allows multiple execution.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method when changes occur.
        /// </param>
        public static IRelayCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod, bool allowMultipleExecution, [NotEmptyParams] params object[] notifiers)
        {
            return new RelayCommand(executeMethod, canExecuteMethod, allowMultipleExecution, notifiers);
        }

        /// <summary>
        ///     Factory method to create a new instance of <see cref="IRelayCommand" /> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">
        ///     Delegate to execute when Execute is called on the command. This can be null to just hook up
        ///     a CanExecute delegate.
        /// </param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method when changes occur.
        /// </param>
        public static IRelayCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod, [NotEmptyParams] params object[] notifiers)
        {
            return new RelayCommand(executeMethod, canExecuteMethod, AllowMultipleExecutionDefault, notifiers);
        }

        /// <summary>
        ///     Factory method to create a new instance of <see cref="IRelayCommand" /> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">
        ///     Delegate to execute when Execute is called on the command. This can be null to just hook up
        ///     a CanExecute delegate.
        /// </param>
        public static IRelayCommand FromAsyncHandler(Func<Task> executeMethod)
        {
            return new RelayCommand(executeMethod, null, true, null);
        }


        /// <summary>
        ///     Factory method to create a new instance of <see cref="IRelayCommand" /> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">
        ///     Delegate to execute when Execute is called on the command. This can be null to just hook up
        ///     a CanExecute delegate.
        /// </param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <param name="allowMultipleExecution">Indicates that command allows multiple execution.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method when changes occur.
        /// </param>
        public static IRelayCommand FromAsyncHandler<TArg>(Func<TArg, Task> executeMethod, Func<TArg, bool> canExecuteMethod, bool allowMultipleExecution, [NotEmptyParams] params object[] notifiers)
        {
            return new RelayCommand<TArg>(executeMethod, canExecuteMethod, allowMultipleExecution, notifiers);
        }

        /// <summary>
        ///     Factory method to create a new instance of <see cref="IRelayCommand" /> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">
        ///     Delegate to execute when Execute is called on the command. This can be null to just hook up
        ///     a CanExecute delegate.
        /// </param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command. This can be null.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method when changes occur.
        /// </param>
        public static IRelayCommand FromAsyncHandler<TArg>(Func<TArg, Task> executeMethod, Func<TArg, bool> canExecuteMethod, [NotEmptyParams] params object[] notifiers)
        {
            return new RelayCommand<TArg>(executeMethod, canExecuteMethod, AllowMultipleExecutionDefault, notifiers);
        }

        /// <summary>
        ///     Factory method to create a new instance of <see cref="IRelayCommand" /> from an awaitable handler method.
        /// </summary>
        /// <param name="executeMethod">
        ///     Delegate to execute when Execute is called on the command. This can be null to just hook up
        ///     a CanExecute delegate.
        /// </param>
        public static IRelayCommand FromAsyncHandler<TArg>(Func<TArg, Task> executeMethod)
        {
            return new RelayCommand<TArg>(executeMethod, null, true, null);
        }

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        ///     true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        protected abstract bool CanExecuteInternal(object parameter);

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        protected abstract void ExecuteInternal(object parameter);

        /// <summary>
        ///     Creates the notifier.
        /// </summary>
        /// <param name="item">The specified item to create notifier.</param>
        /// <returns>An action to unsubscribe.</returns>
        [CanBeNull]
        protected virtual Action<RelayCommandBase, object> CreateNotifier(object item)
        {
            var observable = item as IObservable;
            if (observable != null && observable.Subscribe(this) != null)
                return (@base, o) => ((IObservable)o).Unsubscribe(@base);

            var propertyChanged = item as INotifyPropertyChanged;
            if (propertyChanged == null)
                return null;
            propertyChanged.PropertyChanged += _weakHandler;
            return (@base, o) => ((INotifyPropertyChanged)o).PropertyChanged -= @base._weakHandler;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        private bool Contains(object item, bool remove)
        {
            for (int i = 0; i < _notifiers.Count; i++)
            {
                object target = _notifiers[i].Key.Target;
                if (target == null)
                {
                    _notifiers.RemoveAt(i);
                    i--;
                }
                else if (ReferenceEquals(target, item))
                {
                    if (remove)
                    {
                        _notifiers[i].Value(this, target);
                        _notifiers.RemoveAt(i);
                    }
                    return true;
                }
            }
            return false;
        }

        private void ClearInternal(List<KeyValuePair<WeakReference, Action<RelayCommandBase, object>>> notifiers)
        {
            lock (notifiers)
            {
                for (int index = 0; index < notifiers.Count; index++)
                {
                    var notifier = notifiers[index];
                    object target = notifier.Key.Target;
                    if (target != null)
                        notifier.Value(this, target);
                }
                notifiers.Clear();
            }
        }

        private static void RaiseCanExecuteChangedStatic(RelayCommandBase @this, EventArgs args)
        {
            EventHandler handler = @this._canExecuteChangedInternal;
            if (handler != null)
                handler(@this, args);
        }

        private static void OnPropertyChangedStatic(RelayCommandBase relayCommandBase, object o, PropertyChangedEventArgs arg3)
        {
            relayCommandBase.RaiseCanExecuteChanged();
        }

        #endregion

        #region Overrides of NotifyPropertyChangedBase

        /// <summary>
        ///     Occurs on end suspend notifications.
        /// </summary>
        protected override void OnEndSuspendNotifications(bool isDirty)
        {
            if (isDirty)
                RaiseCanExecuteChanged();
        }

        #endregion
    }
}