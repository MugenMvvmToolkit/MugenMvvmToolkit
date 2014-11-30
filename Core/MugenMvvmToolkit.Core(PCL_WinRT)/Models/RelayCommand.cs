#region Copyright
// ****************************************************************************
// <copyright file="RelayCommand.cs">
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

        private Predicate<TArg> _canExecute;
        private Action<TArg> _execute;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        public RelayCommand([NotNull] Action<TArg> execute)
            : base(false)
        {
            Should.NotBeNull(execute, "execute");
            _execute = execute;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        /// <param name="canExecute">The specified command condition.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method, when a change
        ///     occurs.
        /// </param>
        public RelayCommand([NotNull] Action<TArg> execute, Predicate<TArg> canExecute,
            [NotEmptyParams] params object[] notifiers)
            : base(canExecute != null, notifiers)
        {
            Should.NotBeNull(execute, "execute");
            _execute = execute;
            _canExecute = canExecute;
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
            return canExecute == null || canExecute((TArg)parameter);
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
            if (execute != null)
                execute((TArg)parameter);
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
    public class RelayCommand : RelayCommand<object>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        public RelayCommand([NotNull] Action<object> execute)
            : base(execute)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="execute">The specified command action for execute.</param>
        /// <param name="canExecute">The specified command condition.</param>
        /// <param name="notifiers">
        ///     The specified objects that invokes the <c>RaiseCanExecuteChanged</c> method, when a change
        ///     occurs.
        /// </param>
        public RelayCommand([NotNull] Action<object> execute, Predicate<object> canExecute, params object[] notifiers)
            : base(execute, canExecute, notifiers)
        {
        }

        #endregion
    }
}