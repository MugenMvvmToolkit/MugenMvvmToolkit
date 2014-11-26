#region Copyright
// ****************************************************************************
// <copyright file="AsyncRelayCommand.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     A command whose sole purpose is to relay its functionality to other objects by invoking delegates. The default
    ///     return value for the CanExecute method is 'true'.
    /// </summary>
    public class AsyncRelayCommand<TArg> : RelayCommandBase
    {
        #region Fields

        private readonly Predicate<TArg> _canExecute;
        private readonly Func<TArg, Task> _execute;
        private bool _isRunning;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsyncRelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        public AsyncRelayCommand([NotNull] Func<TArg, Task> execute)
            : base(true)
        {
            Should.NotBeNull(execute, "execute");
            _execute = execute;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsyncRelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        /// <param name="canExecute">The specified command condition.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method, when a change
        ///     occurs.
        /// </param>
        public AsyncRelayCommand([NotNull] Func<TArg, Task> execute, Predicate<TArg> canExecute,
            [NotEmptyParams] params object[] notifiers)
            : base(true, notifiers)
        {
            Should.NotBeNull(execute, "execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region Propeties

        public bool IsRunning
        {
            get { return _isRunning; }
            private set
            {
                if (value.Equals(_isRunning)) return;
                _isRunning = value;
                OnPropertyChanged("IsRunning");
            }
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
            return !IsRunning && (_canExecute == null || _canExecute((TArg)parameter));
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
            IsRunning = true;
            _execute((TArg)parameter).TryExecuteSynchronously(task => IsRunning = false);
        }

        #endregion
    }

    /// <summary>
    ///     A command whose sole purpose is to relay its functionality to other objects by invoking delegates. The default
    ///     return value for the CanExecute method is 'true'.
    /// </summary>
    public class AsyncRelayCommand : AsyncRelayCommand<object>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsyncRelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        public AsyncRelayCommand(Func<object, Task> execute)
            : base(execute)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AsyncRelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        /// <param name="canExecute">The specified command condition.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method, when a change
        ///     occurs.
        /// </param>
        public AsyncRelayCommand(Func<object, Task> execute, Predicate<object> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers)
        {
        }

        #endregion
    }
}