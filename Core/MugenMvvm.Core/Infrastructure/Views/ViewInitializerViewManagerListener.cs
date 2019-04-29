using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Infrastructure.Views
{
    public class ViewInitializerViewManagerListener : IViewManagerListener//todo listen wrappers from metadata
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewInitializerViewManagerListener(IViewModelDispatcher viewModelDispatcher, IViewDataContextProvider dataContextProvider, int priority = 1)
        {
            Should.NotBeNull(viewModelDispatcher, nameof(viewModelDispatcher));
            Should.NotBeNull(dataContextProvider, nameof(dataContextProvider));
            ViewModelDispatcher = viewModelDispatcher;
            DataContextProvider = dataContextProvider;
            Priority = priority;
        }

        #endregion

        #region Properties

        public IViewModelDispatcher ViewModelDispatcher { get; }

        public IViewDataContextProvider DataContextProvider { get; }

        public int Priority { get; }

        #endregion

        #region Implementation of interfaces

        public int GetPriority(object source)
        {
            return Priority;
        }

        public virtual void OnViewModelCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
        {
        }

        public virtual void OnViewCreated(IViewManager viewManager, object view, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
        }

        public virtual void OnViewInitialized(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            ViewModelDispatcher.Subscribe(viewModel, viewInfo.View, ThreadExecutionMode.Main, metadata);
            DataContextProvider.SetDataContext(viewInfo.View, viewModel, metadata);
            (viewInfo.View as IInitializableView)?.Initialize(viewModel, viewInfo, metadata);
        }

        public virtual void OnViewCleared(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            ViewModelDispatcher.Unsubscribe(viewModel, viewInfo.View, metadata); //todo move to cleanup task
            DataContextProvider.SetDataContext(viewInfo.View, null, metadata);
            (viewInfo.View as ICleanableView)?.Cleanup(metadata);
            viewInfo.ClearMetadata(true);
        }

        #endregion
    }
}