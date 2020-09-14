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
        #region Fields

        private readonly IViewModelManager? _viewModelManager;

        #endregion

        #region Constructors

        public TestViewModelProviderComponent(IViewModelManager? viewModelManager = null)
        {
            _viewModelManager = viewModelManager;
        }

        #endregion

        #region Properties

        public Func<object, IReadOnlyMetadataContext?, IViewModelBase?>? TryGetViewModel { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IViewModelBase? IViewModelProviderComponent.TryGetViewModel(IViewModelManager viewModelManager, object request, IReadOnlyMetadataContext? metadata)
        {
            _viewModelManager?.ShouldEqual(viewModelManager);
            return TryGetViewModel?.Invoke(request, metadata);
        }

        #endregion
    }
}