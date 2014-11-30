using System;
using System.Threading.Tasks;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

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

        #endregion
    }
}