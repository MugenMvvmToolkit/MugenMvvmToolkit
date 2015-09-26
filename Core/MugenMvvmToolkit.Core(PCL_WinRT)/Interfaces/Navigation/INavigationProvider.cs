#region Copyright

// ****************************************************************************
// <copyright file="INavigationProvider.cs">
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
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    public interface INavigationProvider : IDisposable
    {
        [CanBeNull]
        IViewModel CurrentViewModel { get; }

        [CanBeNull]
        object CurrentContent { get; }

        [NotNull]
        Task CurrentNavigationTask { get; }

        [CanBeNull]
        INavigationCachePolicy CachePolicy { get; }

        bool CanGoBack { get; }

        bool CanGoForward { get; }

        void GoBack();

        void GoForward();

        Task NavigateAsync([CanBeNull] IOperationCallback callback, [NotNull] IDataContext context);

        void OnNavigated([NotNull] IViewModel viewModel, NavigationMode mode, [CanBeNull] IDataContext context);

        event EventHandler<INavigationProvider, NavigatedEventArgs> Navigated;
    }
}
