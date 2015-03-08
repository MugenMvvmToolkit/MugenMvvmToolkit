#region Copyright

// ****************************************************************************
// <copyright file="CloseableViewModel.cs">
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

using System.Threading.Tasks;
using System.Windows.Input;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.ViewModels
{
    /// <summary>
    ///     Represents the base class that allows to close a view model.
    /// </summary>
    [BaseViewModel(Priority = 8)]
    public abstract class CloseableViewModel : ViewModelBase, ICloseableViewModel
    {
        #region Fields

        /// <summary>
        ///     Gets the parameter that can be used to immediately close the closeable view model without any check.
        ///     closeableViewModel.CloseAsync(CloseableMediator.ImmediateCloseParameter);
        /// </summary>
        public static readonly object ImmediateCloseParameter;
        private ICommand _closeCommand;

        #endregion

        #region Constructors

        static CloseableViewModel()
        {
            ImmediateCloseParameter = new object();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CloseableViewModel" /> class.
        /// </summary>
        protected CloseableViewModel()
        {
            _closeCommand = RelayCommandBase.FromAsyncHandler<object>(ExecuteClose, CanClose, false, this);
        }

        #endregion

        #region Implementation of ICloseableViewModel

        /// <summary>
        ///     Gets or sets a command that attempts to remove this workspace from the user interface.
        /// </summary>
        public ICommand CloseCommand
        {
            get { return _closeCommand; }
            set
            {
                if (Equals(_closeCommand, value))
                    return;
                _closeCommand = value;
                OnPropertyChanged("CloseCommand");
            }
        }

        /// <summary>
        ///     Tries to close view-model.
        /// </summary>
        /// <param name="parameter">The specified parameter, if any.</param>
        /// <returns>An instance of task with result.</returns>
        public virtual Task<bool> CloseAsync(object parameter)
        {
            if (parameter == ImmediateCloseParameter)
                return CloseInternal(parameter);
            return OnClosing(parameter)
                .TryExecuteSynchronously(task =>
                {
                    if (!task.Result || !RaiseClosing(parameter))
                        return false;
                    CloseInternal(parameter);
                    return true;
                });
        }

        /// <summary>
        ///     Called when <see cref="ICloseableViewModel" /> removing from the workspace.
        /// </summary>
        public virtual event EventHandler<ICloseableViewModel, ViewModelClosingEventArgs> Closing;

        /// <summary>
        ///     Called when <see cref="ICloseableViewModel" /> removed from the workspace.
        /// </summary>
        public virtual event EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> Closed;

        #endregion

        #region Methods

        /// <summary>
        ///     Determines whether the specified command <c>CloseCommand</c> can be execute.
        /// </summary>
        /// <param name="param">The specified command parameter.</param>
        /// <returns>
        ///     If <c>true</c> - can execute, otherwise <c>false</c>.
        /// </returns>
        protected virtual bool CanClose(object param)
        {
            return true;
        }

        /// <summary>
        ///     Occurs when view model is closing.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> - close, otherwise <c>false</c>.
        /// </returns>
        protected virtual Task<bool> OnClosing(object parameter)
        {
            return Empty.TrueTask;
        }

        /// <summary>
        ///     Occurs when <c>CloseCommand</c> executed.
        /// </summary>
        protected virtual void OnClosed(object parameter)
        {
        }

        /// <summary>
        ///     Invokes the Closing event.
        /// </summary>
        protected virtual bool RaiseClosing(object parameter)
        {
            var handler = Closing;
            if (handler == null)
                return true;
            var args = new ViewModelClosingEventArgs(this, parameter);
            handler(this, args);
            return !args.Cancel;
        }

        /// <summary>
        ///     Invokes the Closed event.
        /// </summary>
        protected virtual void RaiseClosed(object parameter)
        {
            var handler = Closed;
            if (handler != null)
                handler(this, new ViewModelClosedEventArgs(this, parameter));
        }

        private Task<bool> CloseInternal(object parameter)
        {
            OnClosed(parameter);
            RaiseClosed(parameter);
            return Empty.TrueTask;
        }

        private Task ExecuteClose(object o)
        {
            return this.TryCloseAsync(o, null).WithTaskExceptionHandler(this);
        }

        #endregion

        #region Overrides of ViewModelBase

        /// <summary>
        ///     Occurs after current view model disposed, use for clear resource and event listeners(Internal only).
        /// </summary>
        internal override void OnDisposeInternal(bool disposing)
        {
            if (disposing)
            {
                Closing = null;
                Closed = null;
            }
            base.OnDisposeInternal(disposing);
        }

        #endregion
    }
}