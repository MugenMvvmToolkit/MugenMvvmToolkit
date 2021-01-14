using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using Should;

namespace MugenMvvm.UnitTests.ViewModels.Internal
{
    public class TestViewModelProviderComponent : IViewModelProviderComponent, IHasPriority
    {
        private readonly IViewModelManager? _viewModelManager;

        public TestViewModelProviderComponent(IViewModelManager? viewModelManager = null)
        {
            _viewModelManager = viewModelManager;
        }

        public Func<object, IReadOnlyMetadataContext?, IViewModelBase?>? TryGetViewModel { get; set; }

        public int Priority { get; set; }

        IViewModelBase? IViewModelProviderComponent.TryGetViewModel(IViewModelManager viewModelManager, object request, IReadOnlyMetadataContext? metadata)
        {
            _viewModelManager?.ShouldEqual(viewModelManager);
            return TryGetViewModel?.Invoke(request, metadata);
        }
    }
}