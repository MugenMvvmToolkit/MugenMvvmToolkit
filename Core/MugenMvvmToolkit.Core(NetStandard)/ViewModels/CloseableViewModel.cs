#region Copyright

// ****************************************************************************
// <copyright file="CloseableViewModel.cs">
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

using System.Threading.Tasks;
using System.Windows.Input;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.ViewModels
{
    [BaseViewModel(Priority = 8)]
    public abstract class CloseableViewModel : ViewModelBase, ICloseableViewModel
    {
        #region Fields

        public static readonly object ImmediateCloseParameter;
        private ICommand _closeCommand;

        #endregion

        #region Constructors

        static CloseableViewModel()
        {
            ImmediateCloseParameter = new object();
        }

        protected CloseableViewModel()
        {
            _closeCommand = RelayCommandBase.FromAsyncHandler<object>(ExecuteClose, CanClose, false, this);
        }

        #endregion

        #region Implementation of ICloseableViewModel

        public ICommand CloseCommand
        {
            get { return _closeCommand; }
            set
            {
                if (Equals(_closeCommand, value))
                    return;
                _closeCommand = value;
                OnPropertyChanged();
            }
        }

        public virtual Task<bool> CloseAsync(object parameter = null)
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

        public virtual event EventHandler<ICloseableViewModel, ViewModelClosingEventArgs> Closing;

        public virtual event EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> Closed;

        #endregion

        #region Methods

        protected virtual bool CanClose(object param)
        {
            return true;
        }

        protected virtual Task<bool> OnClosing(object parameter)
        {
            return Empty.TrueTask;
        }

        protected virtual void OnClosed(object parameter)
        {
        }

        protected virtual bool RaiseClosing(object parameter)
        {
            var handler = Closing;
            if (handler == null)
                return true;
            var args = new ViewModelClosingEventArgs(this, parameter);
            handler(this, args);
            return !args.Cancel;
        }

        protected virtual void RaiseClosed(object parameter)
        {
            Closed?.Invoke(this, new ViewModelClosedEventArgs(this, parameter));
        }

        private Task<bool> CloseInternal(object parameter)
        {
            OnClosed(parameter);
            RaiseClosed(parameter);
            return Empty.TrueTask;
        }

        private Task ExecuteClose(object o)
        {
            return this.GetIocContainer(true)
                .Get<INavigationDispatcher>()
                .NavigatingFromAsync(new NavigationContext(NavigationType.Undefined, NavigationMode.Back, this, this.GetParentViewModel(), this), o)
                .WithTaskExceptionHandler(this);
        }

        #endregion

        #region Overrides of ViewModelBase

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
