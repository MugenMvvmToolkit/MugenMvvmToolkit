#region Copyright

// ****************************************************************************
// <copyright file="INavigationCachePolicy.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    public interface INavigationCachePolicy
    {
        void TryCacheViewModel([NotNull] INavigationContext navigationContext, [NotNull] object view,
            [NotNull] IViewModel viewModel);

        IViewModel TryTakeViewModelFromCache([NotNull] INavigationContext navigationContext, [NotNull] object view);

        IList<IViewModel> GetViewModels([CanBeNull] IDataContext context);

        bool Invalidate([NotNull] IViewModel viewModel, [CanBeNull] IDataContext context);

        IList<IViewModel> Invalidate([CanBeNull] IDataContext context);
    }
}
