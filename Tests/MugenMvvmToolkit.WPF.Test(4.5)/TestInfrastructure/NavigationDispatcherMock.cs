#region Copyright

// ****************************************************************************
// <copyright file="NavigationDispatcherMock.cs">
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
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class NavigationDispatcherMock : INavigationDispatcher
    {
        #region Properties

        public Func<INavigationContext, Task<bool>> OnNavigatingFromAsync { get; set; }

        public Action<INavigationContext> OnNavigated { get; set; }

        public Action<INavigationContext, Exception> OnNavigationFailed { get; set; }

        public Action<INavigationContext> OnNavigationCanceled { get; set; }

        #endregion

        #region Methods

        public void RaiseNavigated(NavigatedEventArgs args)
        {
            Navigated?.Invoke(this, args);
        }

        #endregion

        #region Implementation of interfaces

        Task<bool> INavigationDispatcher.OnNavigatingAsync(INavigationContext context)
        {
            return OnNavigatingFromAsync?.Invoke(context) ?? Empty.TrueTask;
        }

        void INavigationDispatcher.OnNavigated(INavigationContext context)
        {
            OnNavigated?.Invoke(context);
        }

        void INavigationDispatcher.OnNavigationFailed(INavigationContext context, Exception exception)
        {
            OnNavigationFailed?.Invoke(context, exception);
        }

        void INavigationDispatcher.OnNavigationCanceled(INavigationContext context)
        {
            OnNavigationCanceled?.Invoke(context);
        }

        public event EventHandler<INavigationDispatcher, NavigatedEventArgs> Navigated;

        public IDictionary<NavigationType, IList<IViewModel>> GetOpenedViewModels(IDataContext context = null)
        {
            throw new NotImplementedException();
        }

        public IList<IViewModel> GetOpenedViewModels(NavigationType type, IDataContext context = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}