#region Copyright

// ****************************************************************************
// <copyright file="CloseableViewModel.cs">
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

using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.ViewModels
{
    [BaseViewModel(Priority = 8)]
    public abstract class CloseableViewModel : ViewModelBase, ICloseableViewModel
    {
        #region Fields

        private ICommand _closeCommand;

        #endregion

        #region Constructors

        protected CloseableViewModel()
        {
            _closeCommand = RelayCommandBase.FromAsyncHandler<object>(ExecuteCloseAsync, CanCloseInternal, false, this);
        }

        #endregion

        #region Properties

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

        #endregion

        #region Methods

        protected virtual bool CanClose(object parameter)
        {
            return true;
        }

        protected virtual Task<bool> OnClosing([NotNull] IDataContext context)
        {
            return Empty.TrueTask;
        }

        protected virtual void OnClosed([NotNull] IDataContext context)
        {
        }

        private bool CanCloseInternal(object parameter)
        {
            var func = Settings.Metadata.GetData(ViewModelConstants.CanCloseHandler);
            if (func != null && !func(this, parameter))
                return false;
            return CanClose(parameter);
        }

        private Task ExecuteCloseAsync(object o)
        {
            return this.CloseAsync(o);
        }

        #endregion

        #region Implementation of interfaces

        Task<bool> ICloseableViewModel.OnClosingAsync(IDataContext context)
        {
            return OnClosing(context ?? DataContext.Empty);
        }

        void ICloseableViewModel.OnClosed(IDataContext context)
        {
            OnClosed(context ?? DataContext.Empty);
        }

        #endregion
    }
}