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

        public Func<object, Type, IReadOnlyMetadataContext?, IViewModelBase?>? TryGetViewModel { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IViewModelBase? IViewModelProviderComponent.TryGetViewModel<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetViewModel?.Invoke(request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}