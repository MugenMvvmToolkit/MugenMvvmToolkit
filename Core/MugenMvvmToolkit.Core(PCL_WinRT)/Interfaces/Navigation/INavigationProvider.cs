#region Copyright
// ****************************************************************************
// <copyright file="INavigationProvider.cs">
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
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    /// <summary>
    ///     Represent the interface for navigation provider.
    /// </summary>
    public interface INavigationProvider : IDisposableObject
    {
        /// <summary>
        ///     Gets the active view model.
        /// </summary>
        [CanBeNull]
        IViewModel CurrentViewModel { get; }

        /// <summary>
        ///     Gets the current content.
        /// </summary>
        [CanBeNull]
        object CurrentContent { get; }

        /// <summary>
        ///     Gets the cache policy, if any.
        /// </summary>
        [CanBeNull]
        INavigationCachePolicy CachePolicy { get; }

        /// <summary>
        ///     Indicates whether the navigator can navigate back.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        ///     Indicates whether the navigator can navigate forward.
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        ///     Navigates back.
        /// </summary>
        void GoBack();

        /// <summary>
        ///     Navigates forward.
        /// </summary>
        void GoForward();

        /// <summary>
        ///     Navigates using the specified data context.
        /// </summary>
        /// <param name="callback">The specified callback, if any.</param>
        /// <param name="context">
        ///     The specified <see cref="IDataContext" />.
        /// </param>
        void Navigate([CanBeNull] IOperationCallback callback, [NotNull] IDataContext context);

        /// <summary>
        ///     Occurs after view model was navigated.
        /// </summary>
        event EventHandler<INavigationProvider, NavigatedEventArgs> Navigated;
    }
}