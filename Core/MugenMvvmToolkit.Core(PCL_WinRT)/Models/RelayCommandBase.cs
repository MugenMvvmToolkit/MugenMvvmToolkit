#region Copyright

// ****************************************************************************
// <copyright file="RelayCommandBase.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
    public abstract class RelayCommandBase : NotifyPropertyChangedBase, IRelayCommand, IHandler<IBroadcastMessage>
    {
        #region Fields

        private static readonly Action<RelayCommandBase, EventArgs> RaiseCanExecuteChangedDelegate;
        private static readonly Action<RelayCommandBase, object, PropertyChangedEventArgs> OnPropertyChangedDelegate;
        private static readonly List<KeyValuePair<WeakReference, Action<RelayCommandBase, object>>> DisposedFlag;

        private PropertyChangedEventHandler _weakHandler;
        private List<KeyValuePair<WeakReference, Action<RelayCommandBase, object>>> _notifiers;
        private EventHandler _canExecuteChangedInternal;
        private HashSet<string> _ignoreProperties;

        #endregion

        #region Constructors

        static RelayCommandBase()
        {
            DisposedFlag = new List<KeyValuePair<WeakReference, Action<RelayCommandBase, object>>>();
            RaiseCanExecuteChangedDelegate = RaiseCanExecuteChangedStatic;
            OnPropertyChangedDelegate = OnPropertyChangedStatic;
        }

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

        protected RelayCommandBase(bool hasCanExecuteImpl, params object[] notifiers)
            : this(hasCanExecuteImpl)
        {
            if (hasCanExecuteImpl || notifiers != null)
            {
                for (int index = 0; index < notifiers.Length; index++)
                    AddNotifier(notifiers[index]);
            }
        }

        protected RelayCommandBase(params object[] notifiers)
            : this(true, notifiers)
        {

        }

        #endregion

        #region Properties

        [NotNull]
        public ICollection<string> IgnoreProperties
        {
            get
            {
                if (_ignoreProperties == null)
                    _ignoreProperties = new HashSet<string>(StringComparer.Ordinal);
                return _ignoreProperties;
            }
        }

        public bool ExecuteAsynchronously { get; set; }

        #endregion

        #region Implementation of IRelayCommand

        public bool HasCanExecuteImpl => _weakHandler != null;

        public CommandExecutionMode ExecutionMode { get; set; }

        public ExecutionMode CanExecuteMode { get; set; }

        public bool CanExecute(object parameter)
        {
            return !HasCanExecuteImpl || CanExecuteInternal(parameter);
        }

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
        }

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

        public bool AddNotifier(object item)
        {
            Should.NotBeNull(item, nameof(item));
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

        public bool RemoveNotifier(object item)
        {
            Should.NotBeNull(item, nameof(item));
            if (!HasCanExecuteImpl)
                return false;
            lock (_notifiers)
                return Contains(item, true);
        }

        public void ClearNotifiers()
        {
            if (HasCanExecuteImpl)
                ClearInternal(_notifiers);
        }

        void IHandler<IBroadcastMessage>.Handle(object sender, IBroadcastMessage message)
        {
            RaiseCanExecuteChanged();
        }

        #endregion

        #region Methods

        public static RelayCommandBase FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod, bool allowMultipleExecution, [NotEmptyParams] params object[] notifiers)
        {
            return new RelayCommand(executeMethod, canExecuteMethod, allowMultipleExecution, notifiers);
        }

        public static RelayCommandBase FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod, [NotEmptyParams] params object[] notifiers)
        {
            return new RelayCommand(executeMethod, canExecuteMethod, true, notifiers);
        }

        public static RelayCommandBase FromAsyncHandler(Func<Task> executeMethod, bool allowMultipleExecution = true)
        {
            return new RelayCommand(executeMethod, null, allowMultipleExecution, null);
        }


        public static RelayCommandBase FromAsyncHandler<TArg>(Func<TArg, Task> executeMethod, Func<TArg, bool> canExecuteMethod, bool allowMultipleExecution, [NotEmptyParams] params object[] notifiers)
        {
            return new RelayCommand<TArg>(executeMethod, canExecuteMethod, allowMultipleExecution, notifiers);
        }

        public static RelayCommandBase FromAsyncHandler<TArg>(Func<TArg, Task> executeMethod, Func<TArg, bool> canExecuteMethod, [NotEmptyParams] params object[] notifiers)
        {
            return new RelayCommand<TArg>(executeMethod, canExecuteMethod, true, notifiers);
        }

        public static RelayCommandBase FromAsyncHandler<TArg>(Func<TArg, Task> executeMethod, bool allowMultipleExecution = true)
        {
            return new RelayCommand<TArg>(executeMethod, null, allowMultipleExecution, null);
        }

        protected abstract bool CanExecuteInternal(object parameter);

        protected abstract void ExecuteInternal(object parameter);

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
            @this._canExecuteChangedInternal?.Invoke(@this, args);
        }

        private static void OnPropertyChangedStatic(RelayCommandBase relayCommandBase, object o, PropertyChangedEventArgs arg3)
        {
            if (arg3.PropertyName == null || relayCommandBase._ignoreProperties == null || !relayCommandBase._ignoreProperties.Contains(arg3.PropertyName))
                relayCommandBase.RaiseCanExecuteChanged();
        }

        #endregion

        #region Overrides of NotifyPropertyChangedBase

        protected override void OnEndSuspendNotifications(bool isDirty)
        {
            if (isDirty)
                RaiseCanExecuteChanged();
        }

        #endregion
    }
}
