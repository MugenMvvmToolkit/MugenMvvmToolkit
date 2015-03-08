#region Copyright

// ****************************************************************************
// <copyright file="RelayCommand.cs">
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
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     A command whose sole purpose is to relay its functionality to other objects by invoking delegates. The default
    ///     return value for the CanExecute method is 'true'.
    /// </summary>
    public class RelayCommand<TArg> : RelayCommandBase
    {
        #region Fields

        private Func<TArg, bool> _canExecute;
        private Delegate _execute;
        private readonly byte _state;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        public RelayCommand([NotNull] Action<TArg> execute)
            : this(execute, null, Empty.Array<object>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        /// <param name="canExecute">The specified command condition.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method when changes occur.
        /// </param>
        public RelayCommand([NotNull] Action<TArg> execute, Func<TArg, bool> canExecute,
            [NotEmptyParams] params object[] notifiers)
            : base(canExecute != null, notifiers)
        {
            Should.NotBeNull(execute, "execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        /// <param name="canExecute">The specified command condition.</param>
        /// <param name="allowMultipleExecution">Indicates that command allows multiple execution.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method when changes occur.
        /// </param>
        protected internal RelayCommand([NotNull] Func<TArg, Task> execute, [CanBeNull] Func<TArg, bool> canExecute, bool allowMultipleExecution,
            [NotEmptyParams] params object[] notifiers)
            : base(canExecute != null, notifiers)
        {
            Should.NotBeNull(execute, "execute");
            _execute = execute;
            _canExecute = canExecute;
            _state |= RelayCommand.TaskDelegateFlag;
            if (allowMultipleExecution)
                _state |= RelayCommand.AllowMultipleExecutionFlag;
        }

        #endregion

        #region Overrides of RelayCommandBase

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
        protected override bool CanExecuteInternal(object parameter)
        {
            Func<TArg, bool> canExecute = _canExecute;
            return canExecute != null && canExecute((TArg)parameter);
        }

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
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
                    t.TryExecuteSynchronously(task => _execute = execute);
            }
            catch
            {
                _execute = execute;
                throw;
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected override void OnDispose()
        {
            _canExecute = null;
            _execute = null;
            base.OnDispose();
        }

        #endregion
    }

    /// <summary>
    ///     A command whose sole purpose is to relay its functionality to other objects by invoking delegates. The default
    ///     return value for the CanExecute method is 'true'.
    /// </summary>
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        public RelayCommand([NotNull] Action<object> execute)
            : this(execute, null, Empty.Array<object>())
        {
        }


        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        public RelayCommand([NotNull] Action execute)
            : this(execute, null, Empty.Array<object>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        /// <param name="canExecute">The specified command condition.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method when changes occur.
        /// </param>
        public RelayCommand([NotNull] Action<object> execute, [CanBeNull] Func<object, bool> canExecute,
            [NotEmptyParams] params object[] notifiers)
            : base(canExecute != null, notifiers)
        {
            Should.NotBeNull(execute, "execute");
            _execute = execute;
            _canExecute = canExecute;
            _state |= ObjectDelegateFlag;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        /// <param name="canExecute">The specified command condition.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method when changes occur.
        /// </param>
        public RelayCommand([NotNull] Action execute, [CanBeNull] Func<bool> canExecute,
            [NotEmptyParams] params object[] notifiers)
            : base(canExecute != null, notifiers)
        {
            Should.NotBeNull(execute, "execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        /// <param name="canExecute">The specified command condition.</param>
        /// <param name="allowMultipleExecution">Indicates that command allows multiple execution.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method when changes occur.
        /// </param>
        protected internal RelayCommand([NotNull] Func<Task> execute, [CanBeNull] Func<bool> canExecute, bool allowMultipleExecution, [NotEmptyParams] params object[] notifiers)
            : base(canExecute != null, notifiers)
        {
            Should.NotBeNull(execute, "execute");
            _execute = execute;
            _canExecute = canExecute;
            _state |= TaskDelegateFlag;
            if (allowMultipleExecution)
                _state |= AllowMultipleExecutionFlag;
        }

        #endregion

        #region Overrides of RelayCommandBase

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
        protected override bool CanExecuteInternal(object parameter)
        {
            var canExecute = _canExecute;
            if (canExecute == null)
                return false;
            if (_state.HasFlagEx(ObjectDelegateFlag))
                return ((Func<object, bool>)canExecute).Invoke(parameter);
            return ((Func<bool>)canExecute).Invoke();
        }

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
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
                        t.TryExecuteSynchronously(task => _execute = execute);
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

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected override void OnDispose()
        {
            _execute = null;
            _canExecute = null;
            base.OnDispose();
        }

        #endregion
    }
}