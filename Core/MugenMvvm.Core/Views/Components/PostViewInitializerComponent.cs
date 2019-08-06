using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public class PostViewInitializerComponent : IViewManagerListener //todo listen wrappers from metadata
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public PostViewInitializerComponent(IViewModelDispatcher viewModelDispatcher, int priority = 1)
        {
            Should.NotBeNull(viewModelDispatcher, nameof(viewModelDispatcher));
            ViewModelDispatcher = viewModelDispatcher;
            Priority = priority;
        }

        #endregion

        #region Properties

        public IViewModelDispatcher ViewModelDispatcher { get; }

        public int Priority { get; }

        #endregion

        #region Implementation of interfaces

        public int GetPriority(object source)
        {
            return Priority;
        }

        public virtual void OnViewModelCreated(IViewManager viewManager, IViewModelBase viewModel, object view, IMetadataContext metadata)
        {
        }

        public virtual void OnViewCreated(IViewManager viewManager, object view, IViewModelBase viewModel, IMetadataContext metadata)
        {
        }

        public virtual void OnViewInitialized(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            ViewModelDispatcher.Subscribe(viewModel, viewInfo.View, ThreadExecutionMode.Main, metadata);
            (viewInfo.View as IInitializableView)?.Initialize(viewModel, viewInfo, metadata);
        }

        public virtual void OnViewCleared(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            ViewModelDispatcher.Unsubscribe(viewModel, viewInfo.View, metadata);
            (viewInfo.View as ICleanableView)?.Cleanup(metadata);
            viewInfo.ClearMetadata();
        }

        #endregion
    }
}