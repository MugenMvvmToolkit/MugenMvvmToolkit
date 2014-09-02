using System;
using System.Threading.Tasks;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ViewManagerMock : IViewManager
    {
        #region Properties

        public Func<IViewModel, string, IView> GetViewDelegate { get; set; }

        public Func<IViewModel, IDataContext, IView> GetViewRawDelegate { get; set; }

        public Action<IViewModel, IView> InitializeViewForViewModelDelegate { get; set; }

        public Action<IViewModel> CleanupViewOnlyVmDelegate { get; set; }

        public Action<IViewModel, IView> CleanupViewDelegate { get; set; }

        #endregion

        #region Implementation of IViewManager

        /// <summary>
        ///     Gets the type of view wrapper.
        /// </summary>
        public Type GetViewType(Type viewType, IDataContext dataContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Wraps the specified view object to a <see cref="IView" />.
        /// </summary>
        public IView WrapToView(object view, IDataContext dataContext)
        {
            return (IView)view;
        }

        /// <summary>
        ///     Gets an instance of <see cref="IView" /> for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model which is now initialized.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IView" />.
        /// </returns>
        Task<IView> IViewManager.GetViewAsync(IViewModel viewModel, IDataContext dataContext)
        {
            if (GetViewRawDelegate == null)
                return ToolkitExtensions.FromResult(GetViewDelegate(viewModel, dataContext.GetData(InitializationConstants.ViewName)));
            return ToolkitExtensions.FromResult(GetViewRawDelegate(viewModel, dataContext));
        }

        /// <summary>
        ///     Configures the specified view for the specified view-model.
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="view">The specified view.</param>
        Task IViewManager.InitializeViewAsync(IViewModel viewModel, IView view)
        {
            if (InitializeViewForViewModelDelegate != null)
                InitializeViewForViewModelDelegate(viewModel, view);
            return Empty.FalseTask;
        }

        /// <summary>
        ///     Clears view in the specified view-model
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        Task IViewManager.CleanupViewAsync(IViewModel viewModel)
        {
            if (CleanupViewOnlyVmDelegate != null)
                CleanupViewOnlyVmDelegate(viewModel);
            return Empty.FalseTask;
        }

        #endregion
    }
}