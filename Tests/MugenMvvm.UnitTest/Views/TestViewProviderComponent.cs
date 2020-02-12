using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.UnitTest.Views
{
    public class TestViewProviderComponent : IViewProviderComponent, IHasPriority
    {
        #region Properties

        public Func<IViewModelBase, IReadOnlyMetadataContext?, IReadOnlyList<IView>?>? TryGetViews { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyList<IView>? IViewProviderComponent.TryGetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            return TryGetViews?.Invoke(viewModel, metadata);
        }

        #endregion
    }
}