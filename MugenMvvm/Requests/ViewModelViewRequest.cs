using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Requests
{
    public class ViewModelViewRequest
    {
        #region Constructors

        public ViewModelViewRequest(IViewModelBase? viewModel, object? view)
        {
            ViewModel = viewModel;
            View = view;
        }

        #endregion

        #region Properties

        public object? View { get; set; }

        public IViewModelBase? ViewModel { get; set; }

        #endregion

        #region Methods

        [return: NotNullIfNotNull("viewModel")]
        [return: NotNullIfNotNull("view")]
        public static object? GetRequestOrRaw(IViewModelBase? viewModel, object? view)
        {
            if (viewModel != null && view != null)
                return new ViewModelViewRequest(viewModel, view);
            return viewModel ?? view;
        }

        [return: NotNullIfNotNull("request")]
        [return: NotNullIfNotNull("viewModel")]
        [return: NotNullIfNotNull("view")]
        public static object? GetRequestOrRaw(object? request, IViewModelBase? viewModel, object? view) //todo review
        {
            if (request is ViewModelViewRequest viewModelViewRequest)
            {
                viewModelViewRequest.ViewModel = viewModel;
                viewModelViewRequest.View = view;
                return viewModelViewRequest;
            }

            return GetRequestOrRaw(viewModel, view);
        }

        #endregion
    }
}