using System;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ViewModelProviderMock : IViewModelProvider
    {
        #region Proeprties

        public Func<GetViewModelDelegate<IViewModel>, IDataContext, IViewModel> GetViewModel { get; set; }

        public Func<Type, IDataContext, IViewModel> GetViewModelType { get; set; }

        public Action<IViewModel, IDataContext> InitializeViewModel { get; set; }

        #endregion

        #region Implementation of IViewModelProvider

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="getViewModel">The specified delegate to create view model.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        IViewModel IViewModelProvider.GetViewModel(GetViewModelDelegate<IViewModel> getViewModel,
            IDataContext dataContext)
        {
            return GetViewModel(getViewModel, dataContext);
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="viewModelType">The type of view model.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        IViewModel IViewModelProvider.GetViewModel(Type viewModelType, IDataContext dataContext)
        {
            return GetViewModelType(viewModelType, dataContext);
        }

        /// <summary>
        ///     Initializes the specified <see cref="IViewModel" />, use this method if you have created an
        ///     <see cref="IViewModel" />
        ///     without using the GetViewModel method.
        /// </summary>
        /// <param name="viewModel">
        ///     The specified <see cref="IViewModel" />.
        /// </param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        void IViewModelProvider.InitializeViewModel(IViewModel viewModel, IDataContext dataContext)
        {
            InitializeViewModel(viewModel, dataContext);
        }

        /// <summary>
        ///     Preserves the view model state.
        /// </summary>
        public IDataContext PreserveViewModel(IViewModel viewModel, IDataContext dataContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Restores the view model from state context.
        /// </summary>
        /// <param name="viewModelState">The specified state <see cref="IDataContext" />.</param>
        /// <param name="throwOnError">
        ///     <c>true</c> to throw an exception if the view model cannot be restored; <c>false</c> to return null.
        /// </param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        public IViewModel RestoreViewModel(IDataContext viewModelState, IDataContext dataContext, bool throwOnError)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}