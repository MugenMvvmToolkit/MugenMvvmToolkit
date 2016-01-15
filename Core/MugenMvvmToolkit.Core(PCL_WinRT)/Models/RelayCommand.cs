#region Copyright

// ****************************************************************************
// <copyright file="RelayCommand.cs">
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
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;

namespace MugenMvvmToolkit.Models
{
    public class RelayCommand<TArg> : RelayCommandBase
    {
        #region Fields

        private Func<TArg, bool> _canExecute;
        private Delegate _execute;
        private readonly byte _state;

        #endregion

        #region Constructors

        public RelayCommand([NotNull] Action<TArg> execute)
            : this(execute, null, Empty.Array<object>())
        {
        }

        public RelayCommand([NotNull] Action<TArg> execute, Func<TArg, bool> canExecute,
            [NotEmptyParams] params object[] notifiers)
            : base(canExecute != null, notifiers)
        {
            Should.NotBeNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
        }

        protected RelayCommand([NotNull] Func<TArg, Task> execute, [CanBeNull] Func<TArg, bool> canExecute, bool allowMultipleExecution,
            [NotEmptyParams] params object[] notifiers)
            : base(canExecute != null, notifiers)
        {
            Should.NotBeNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
            _state |= RelayCommand.TaskDelegateFlag;
            if (allowMultipleExecution)
                _state |= RelayCommand.AllowMultipleExecutionFlag;
        }

        #endregion

        #region Overrides of RelayCommandBase

        protected override bool CanExecuteInternal(object parameter)
        {
            Func<TArg, bool> canExecute = _canExecute;
            return canExecute != null && _execute != null && canExecute((TArg)parameter);
        }

        protected override void ExecuteInternal(object parameter)
        {
            var execute = _execute;
            if (execute == null)
                return;
            if (_state == 0)
            {
                ((Action<TArg>)_execute).Invoke((TArg)parameter);
                return;
            }
            var allowMultiple = _state.HasFlagEx(RelayCommand.AllowMultipleExecutionFlag);
            if (!allowMultiple && Interlocked.Exchange(ref _execute, null) == null)
                return;

            try
            {
                var t = ((Func<TArg, Task>)execute).Invoke((TArg)parameter);
                if (!allowMultiple)
                {
                    RaiseCanExecuteChanged();
                    t.TryExecuteSynchronously(task =>
                    {
                        _execute = execute;
                        RaiseCanExecuteChanged();
                    });
                }
            }
            catch
            {
                _execute = execute;
                throw;
            }
        }

        protected override void OnDispose()
        {
            _canExecute = null;
            _execute = null;
            base.OnDispose();
        }

        #endregion
    }

    public class RelayCommand : RelayCommandBase
    {
        #region Fields

        private const byte ObjectDelegateFlag = 1 << 0;
        internal const byte TaskDelegateFlag = 1 << 1;
        internal const byte AllowMultipleExecutionFlag = 1 << 2;

        private readonly byte _state;
        private Delegate _execute;
        private Delegate _canExecute;

        #endregion

        #region Constructors

        public RelayCommand([NotNull] Action<object> execute)
            : this(execute, null, Empty.Array<object>())
        {
        }


        public RelayCommand([NotNull] Action execute)
            : this(execute, null, Empty.Array<object>())
        {
        }

        public RelayCommand([NotNull] Action<object> execute, [CanBeNull] Func<object, bool> canExecute,
            [NotEmptyParams] params object[] notifiers)
            : base(canExecute != null, notifiers)
        {
            Should.NotBeNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
            _state |= ObjectDelegateFlag;
        }

        public RelayCommand([NotNull] Action execute, [CanBeNull] Func<bool> canExecute,
            [NotEmptyParams] params object[] notifiers)
            : base(canExecute != null, notifiers)
        {
            Should.NotBeNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
        }

        protected RelayCommand([NotNull] Func<Task> execute, [CanBeNull] Func<bool> canExecute, bool allowMultipleExecution, [NotEmptyParams] params object[] notifiers)
            : base(canExecute != null, notifiers)
        {
            Should.NotBeNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
            _state |= TaskDelegateFlag;
            if (allowMultipleExecution)
                _state |= AllowMultipleExecutionFlag;
        }

        #endregion

        #region Overrides of RelayCommandBase

        protected override bool CanExecuteInternal(object parameter)
        {
            var canExecute = _canExecute;
            if (canExecute == null || _execute == null)
                return false;
            if (_state.HasFlagEx(ObjectDelegateFlag))
                return ((Func<object, bool>)canExecute).Invoke(parameter);
            return ((Func<bool>)canExecute).Invoke();
        }

        protected override void ExecuteInternal(object parameter)
        {
            var execute = _execute;
            if (execute == null)
                return;
            if (_state == 0)
                ((Action)execute).Invoke();
            else if (_state.HasFlagEx(TaskDelegateFlag))
            {
                var allowMultiple = _state.HasFlagEx(AllowMultipleExecutionFlag);
                if (!allowMultiple && Interlocked.Exchange(ref _execute, null) == null)
                    return;
                try
                {
                    var t = ((Func<Task>)execute).Invoke();
                    if (!allowMultiple)
                    {
                        RaiseCanExecuteChanged();
                        t.TryExecuteSynchronously(task =>
                        {
                            _execute = execute;
                            RaiseCanExecuteChanged();
                        });
                    }
                }
                catch
                {
                    _execute = execute;
                    throw;
                }
            }
            else
                ((Action<object>)execute).Invoke(parameter);
        }

        protected override void OnDispose()
        {
            _execute = null;
            _canExecute = null;
            base.OnDispose();
        }

        #endregion
    }
}
