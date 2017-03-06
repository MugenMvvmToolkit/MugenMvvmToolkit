#region Copyright

// ****************************************************************************
// <copyright file="IViewManager.cs">
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IViewManager
    {
        Task<object> GetViewAsync([NotNull] IViewModel viewModel, IDataContext context = null);

        Task<object> GetViewAsync([NotNull] IViewMappingItem viewMapping, IDataContext context = null);

        Task InitializeViewAsync([NotNull] IViewModel viewModel, [NotNull] object view, IDataContext context = null);

        Task CleanupViewAsync([NotNull] IViewModel viewModel, IDataContext context = null);

        event EventHandler<IViewManager, ViewCreatedEventArgs> ViewCreated;

        event EventHandler<IViewManager, ViewInitializedEventArgs> ViewInitialized;

        event EventHandler<IViewManager, ViewClearedEventArgs> ViewCleared;
    }
}
