using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.UnitTest.Views.Internal
{
    public class TestViewManagerListener : IViewManagerListener, IHasPriority
    {
        #region Properties

        public Action<IViewManager, IView, IViewModelBase, IReadOnlyMetadataContext?>? OnViewInitialized { get; set; }

        public Action<IViewManager, IView, IViewModelBase, IReadOnlyMetadataContext?>? OnViewCleared { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IViewManagerListener.OnViewInitialized(IViewManager viewManager, IView view, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            OnViewInitialized?.Invoke(viewManager, view, viewModel, metadata);
        }

        void IViewManagerListener.OnViewCleared(IViewManager viewManager, IView view, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            OnViewCleared?.Invoke(viewManager, view, viewModel, metadata);
        }

        #endregion
    }
}