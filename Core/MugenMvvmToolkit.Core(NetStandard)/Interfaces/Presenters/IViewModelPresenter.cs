#region Copyright

// ****************************************************************************
// <copyright file="IViewModelPresenter.cs">
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

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Presenters
{
    public interface IViewModelPresenter
    {
        [NotNull]
        ICollection<IDynamicViewModelPresenter> DynamicPresenters { get; }

        [NotNull]
        Task WaitCurrentNavigationsAsync(IDataContext context = null);

        [NotNull]
        IAsyncOperation ShowAsync([NotNull] IDataContext context);

        Task<bool> CloseAsync([NotNull] IDataContext context);

        void Restore([NotNull] IDataContext context);
    }
}
