#region Copyright

// ****************************************************************************
// <copyright file="IDynamicViewModelPresenter.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Presenters
{
    public interface IDynamicViewModelPresenter
    {
        int Priority { get; }

        [CanBeNull]
        INavigationOperation TryShowAsync([NotNull] IViewModel viewModel, [NotNull] IDataContext context, [NotNull]IViewModelPresenter parentPresenter);

        [CanBeNull]
        Task<bool> TryCloseAsync([NotNull] IViewModel viewModel, [NotNull] IDataContext context, [NotNull]IViewModelPresenter parentPresenter);
    }

    public interface IRestorableDynamicViewModelPresenter : IDynamicViewModelPresenter
    {
        bool Restore([NotNull] IViewModel viewModel, [NotNull] IDataContext context,
            [CanBeNull] IViewModelPresenter parentPresenter);
    }
}
