using System;
using System.Threading.Tasks;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Infrastructure.Views
{
    public class ViewManager : HasListenersBase<IViewManagerListener>, IViewManager
    {
        #region Implementation of interfaces

        public Task<object> GetViewAsync(IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext? context = null)
        {
            Should.NotBeNull(viewMappingInfo, nameof(viewMappingInfo));
        }

        public Task InitializeViewAsync(object view, IViewModel viewModel, IReadOnlyMetadataContext? context = null)
        {
            throw new NotImplementedException();
        }

        public Task CleanupViewAsync(IViewModel viewModel, IReadOnlyMetadataContext? context = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        protected virtual object GetView(IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext context)
        {
            //            object viewObj = ToolkitServiceProvider.Get(viewMapping.ViewType);
            //            if (ApplicationSettings.ViewManagerDisposeView)
            //                ToolkitServiceProvider.AttachedValueProvider.SetValue(viewObj, ViewManagerCreatorPath, null);
            //            if (Tracer.TraceInformation)
            //                Tracer.Info("The view {0} for the view-model {1} was created.", viewObj.GetType(), viewMapping.ViewModelType);
            //            return viewObj;
        }

        #endregion
    }
}