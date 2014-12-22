#region Copyright

// ****************************************************************************
// <copyright file="EmptyNavigationCachePolicy.cs">
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

using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Navigation
{
    /// <summary>
    ///     Represents the empty navigation cache policy.
    /// </summary>
    public sealed class EmptyNavigationCachePolicy : INavigationCachePolicy
    {
        #region Implementation of INavigationCachePolicy

        /// <summary>
        ///     Tries to save a view model in the cache.
        /// </summary>
        public void TryCacheViewModel(INavigationContext navigationContext, object view, IViewModel viewModel)
        {
        }

        /// <summary>
        ///     Tries to get view model from cache, and delete it from the cache.
        /// </summary>
        public IViewModel TryTakeViewModelFromCache(INavigationContext navigationContext, object view)
        {
            return null;
        }

        /// <summary>
        ///     Gets the cached view models.
        /// </summary>
        public IList<IViewModel> GetViewModels(IDataContext context)
        {
            return Empty.Array<IViewModel>();
        }

        /// <summary>
        ///     Removes the view model from cache.
        /// </summary>
        public bool Invalidate(IViewModel viewModel, IDataContext context)
        {
            return false;
        }

        /// <summary>
        ///     Clears the cache.
        /// </summary>
        public IList<IViewModel> Invalidate(IDataContext context)
        {
            return GetViewModels(context);
        }

        #endregion
    }
}