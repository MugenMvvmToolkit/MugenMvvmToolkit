using System;
using System.Threading.Tasks;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ViewManagerMock : IViewManager
    {
        #region Properties

        public Func<IViewModel, string, object> GetViewDelegate { get; set; }

        public Func<IViewModel, IDataContext, object> GetViewRawDelegate { get; set; }

        public Action<IViewModel, object> InitializeViewForViewModelDelegate { get; set; }

        public Action<IViewModel> CleanupViewOnlyVmDelegate { get; set; }

        public Action<IViewModel, object> CleanupViewDelegate { get; set; }

        #endregion

        #region Implementation of IViewManager

        Task<object> IViewManager.GetViewAsync(IViewModel viewModel, IDataContext dataContext)
        {
            if (GetViewRawDelegate == null)
                return ToolkitExtensions.FromResult(GetViewDelegate(viewModel, dataContext.GetData(InitializationConstants.ViewName)));
            return ToolkitExtensions.FromResult(GetViewRawDelegate(viewModel, dataContext));
        }

        public Task<object> GetViewAsync(IViewMappingItem viewMapping, IDataContext context = null)
        {
            throw new NotImplementedException();
        }

        Task IViewManager.InitializeViewAsync(IViewModel viewModel, object view, IDataContext dataContext)
        {
            if (InitializeViewForViewModelDelegate != null)
                InitializeViewForViewModelDelegate(viewModel, view);
            return Empty.FalseTask;
        }

        Task IViewManager.CleanupViewAsync(IViewModel viewModel, IDataContext dataContext)
        {
            if (CleanupViewOnlyVmDelegate != null)
                CleanupViewOnlyVmDelegate(viewModel);
            return Empty.FalseTask;
        }

        public event EventHandler<IViewManager, ViewCreatedEventArgs> ViewCreated
            ;
        public event EventHandler<IViewManager, ViewInitializedEventArgs> ViewInitialized;

        public event EventHandler<IViewManager, ViewClearedEventArgs> ViewCleared;

        #endregion
    }
}
