//using MugenMvvm.Enums;
//using MugenMvvm.Interfaces.Metadata;
//using MugenMvvm.Interfaces.ViewModels;
//using MugenMvvm.Interfaces.ViewModels.Infrastructure;
//using MugenMvvm.Interfaces.Views;
//using MugenMvvm.Interfaces.Views.Infrastructure;
//
//namespace MugenMvvm.Infrastructure.Views
//{
//    public class ViewInitializerViewManagerListener : IViewManagerListener
//    {
//        #region Constructors
//
//        public ViewInitializerViewManagerListener(IViewModelDispatcher viewModelDispatcher, IViewDataContextProvider dataContextProvider)
//        {
//            Should.NotBeNull(viewModelDispatcher, nameof(viewModelDispatcher));
//            Should.NotBeNull(dataContextProvider, nameof(dataContextProvider));
//            ViewModelDispatcher = viewModelDispatcher;
//            DataContextProvider = dataContextProvider;
//        }
//
//        #endregion
//
//        #region Properties
//
//        protected IViewModelDispatcher ViewModelDispatcher { get; }
//
//        protected IViewDataContextProvider DataContextProvider { get; }
//
//        #endregion
//
//        #region Implementation of interfaces
//
//        public void OnViewCreated(IViewManager viewManager, object view, IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext metadata)
//        {
//        }
//
//        public void OnViewInitialized(IViewManager viewManager, object view, IViewModel viewModel, IReadOnlyMetadataContext metadata)
//        {
//            var oldView = viewModel.GetCurrentView<object>();
//            if (ReferenceEquals(oldView, MugenExtensions.GetUnderlyingView<object>(view)))
//                return;
//
//            viewModel.Metadata.Set(ViewModelMetadata.View, view);
//            ViewModelDispatcher.Subscribe(viewModel, view, ThreadExecutionMode.Main, metadata);
//            DataContextProvider.SetDataContext(view, viewModel, metadata);
//            (view as IInitializableView)?.Initialize(viewModel, metadata);
//        }
//
//        public void OnViewCleared(IViewManager viewManager, object view, IViewModel viewModel, IReadOnlyMetadataContext metadata)
//        {
//            viewModel.Metadata.Remove(ViewModelMetadata.View);
//            ViewModelDispatcher.Unsubscribe(viewModel, view, metadata);
//            DataContextProvider.SetDataContext(view, null, metadata);
//            (view as IInitializableView)?.Initialize(viewModel, metadata);
//        }
//
//        #endregion
//    }
//}