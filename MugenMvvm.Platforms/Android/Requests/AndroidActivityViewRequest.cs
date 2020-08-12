using System;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Requests
{
    public class AndroidActivityViewRequest : ViewModelViewRequest
    {
        #region Constructors

        public AndroidActivityViewRequest(IViewModelBase viewModel, IViewMapping mapping, Action startActivity)
            : base(viewModel, null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(mapping, nameof(mapping));
            Should.NotBeNull(startActivity, nameof(startActivity));
            StartActivity = startActivity;
            Mapping = mapping;
        }

        #endregion

        #region Properties

        public Action StartActivity { get; }

        public IViewMapping Mapping { get; }

        #endregion
    }
}