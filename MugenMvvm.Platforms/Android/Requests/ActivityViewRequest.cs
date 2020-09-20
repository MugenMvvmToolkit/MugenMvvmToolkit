using System;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Requests
{
    public interface IAndroidActivityViewRequest
    {

    }

    public class ActivityViewRequest : ViewModelViewRequest
    {
        #region Constructors

        public ActivityViewRequest(IViewModelBase viewModel, IViewMapping mapping, Action startActivity)
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