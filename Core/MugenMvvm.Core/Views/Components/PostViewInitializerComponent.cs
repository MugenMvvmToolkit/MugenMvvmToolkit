using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class PostViewInitializerComponent : IViewManagerListener, IHasPriority //todo listen wrappers from metadata
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public PostViewInitializerComponent()
        {
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public void OnViewInitialized(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            viewModel.TrySubscribe(viewInfo.View, ThreadExecutionMode.Main, metadata);
            (viewInfo.View as IInitializableView)?.Initialize(viewModel, viewInfo, metadata);
        }

        public void OnViewCleared(IViewManager viewManager, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            viewModel.TryUnsubscribe(viewInfo.View, metadata);
            (viewInfo.View as ICleanableView)?.Cleanup(metadata);
            viewInfo.ClearMetadata(true);
        }

        #endregion
    }
}