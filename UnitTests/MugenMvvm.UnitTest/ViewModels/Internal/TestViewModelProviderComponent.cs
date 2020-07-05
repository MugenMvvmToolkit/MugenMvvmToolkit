using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;

namespace MugenMvvm.UnitTest.ViewModels.Internal
{
    public class TestViewModelProviderComponent : IViewModelProviderComponent, IHasPriority
    {
        #region Properties

        public Func<IViewModelManager, object, Type, IReadOnlyMetadataContext?, IViewModelBase?>? TryGetViewModel { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IViewModelBase? IViewModelProviderComponent.TryGetViewModel<TRequest>(IViewModelManager viewModelManager, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetViewModel?.Invoke(viewModelManager, request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}