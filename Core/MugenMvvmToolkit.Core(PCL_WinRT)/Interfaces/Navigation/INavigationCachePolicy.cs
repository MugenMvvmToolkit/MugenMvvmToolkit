#region Copyright
// ****************************************************************************
// <copyright file="INavigationCachePolicy.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    /// <summary>
    ///     Represents the view model navigation cache policy.
    /// </summary>
    public interface INavigationCachePolicy
    {
        /// <summary>
        ///     Tries to save a view model in the cache.
        /// </summary>
        void TryCacheViewModel([NotNull] INavigationContext navigationContext, [NotNull] object view,
            [NotNull] IViewModel viewModel);

        /// <summary>
        ///     Tries to get view model from cache, and delete it from the cache.
        /// </summary>
        IViewModel TryTakeViewModelFromCache([NotNull] INavigationContext navigationContext, [NotNull] object view);

        /// <summary>
        ///     Removes the view model from cache.
        /// </summary>
        bool Invalidate([NotNull] IViewModel viewModel);

        /// <summary>
        ///     Clears the cache.
        /// </summary>
        void Invalidate();
    }
}