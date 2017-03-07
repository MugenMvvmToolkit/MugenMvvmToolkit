#region Copyright

// ****************************************************************************
// <copyright file="INavigationDispatcher.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    public interface INavigationDispatcher
    {
//        IDictionary<NavigationType, IViewModel> GetTopViewModels(IDataContext context = null);

        //todo get TopViewModels
//        IViewModel GetTopViewModel(NavigationType navigationType, IDataContext context = null);

        Task<bool> OnNavigatingFromAsync([NotNull] INavigationContext context);

        void OnNavigated([NotNull]INavigationContext context);

        void OnNavigationFailed([NotNull]INavigationContext context, [NotNull] Exception exception);

        void OnNavigationCanceled([NotNull]INavigationContext context);

        event EventHandler<INavigationDispatcher, NavigatedEventArgs> Navigated;
    }
}