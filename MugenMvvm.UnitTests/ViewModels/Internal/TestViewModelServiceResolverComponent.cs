using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using Should;

namespace MugenMvvm.UnitTests.ViewModels.Internal
{
    public class TestViewModelServiceResolverComponent : IViewModelServiceResolverComponent, IHasPriority
    {
        private readonly IViewModelManager? _viewModelManager;

        public TestViewModelServiceResolverComponent(IViewModelManager? viewModelManager = null)
        {
            _viewModelManager = viewModelManager;
        }

        public Func<IViewModelBase, object, IReadOnlyMetadataContext?, object?>? TryGetService { get; set; }

        public int Priority { get; set; }

        object? IViewModelServiceResolverComponent.TryGetService(IViewModelManager viewModelManager, IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata)
        {
            _viewModelManager?.ShouldEqual(viewModelManager);
            return TryGetService?.Invoke(viewModel, request, metadata);
        }
    }
}