#region Copyright

// ****************************************************************************
// <copyright file="NavigableViewModelMock.cs">
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

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.TestViewModels
{
    public class NavigableViewModelMock : ViewModelBase, INavigableViewModel, ICloseableViewModel, ISelectable
    {
        #region Properties

        public Func<INavigationContext, Task<bool>> OnNavigatingFromDelegate { get; set; }

        public Func<IDataContext, Task<bool>> ClosingDelegate { get; set; }

        public Action<IDataContext> ClosedDelegate { get; set; }

        public Action<INavigationContext> OnNavigatedFromDelegate { get; set; }

        public Action<INavigationContext> OnNavigatedToDelegate { get; set; }

        #region Implementation of ICloseableViewModel

        public ICommand CloseCommand { get; set; }

        #endregion

        #endregion

        #region Overrides of NavigableViewModel

        public Task<bool> OnNavigatingFrom(INavigationContext context)
        {
            if (OnNavigatingFromDelegate != null)
                return OnNavigatingFromDelegate(context);
            return Empty.TrueTask;
        }

        public void OnNavigatedFrom(INavigationContext context)
        {
            OnNavigatedFromDelegate?.Invoke(context);
        }

        public void OnNavigatedTo(INavigationContext context)
        {
            OnNavigatedToDelegate?.Invoke(context);
        }

        #endregion

        #region Implementation of ISelectable

        public bool IsSelected { get; set; }

        public Task<bool> OnClosingAsync(IDataContext context)
        {
            return ClosingDelegate?.Invoke(context);
        }

        public void OnClosed(IDataContext context)
        {
            ClosedDelegate?.Invoke(context);
        }

        #endregion
    }
}