#region Copyright

// ****************************************************************************
// <copyright file="IDynamicViewModelPresenter.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Presenters
{
    public interface IDynamicViewModelPresenter
    {
        int Priority { get; }

        [CanBeNull]
        IAsyncOperation TryShowAsync([NotNull] IDataContext context, [NotNull]IViewModelPresenter parentPresenter);

        [CanBeNull]
        Task<bool> TryCloseAsync([NotNull] IDataContext context, [NotNull]IViewModelPresenter parentPresenter);
    }

    public interface IRestorableDynamicViewModelPresenter : IDynamicViewModelPresenter
    {
        bool Restore([NotNull] IDataContext context, [NotNull] IViewModelPresenter parentPresenter);
    }
}
