using System;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Requests
{
    public sealed class ActivityViewRequest<TState> : ViewModelViewRequest, IActivityViewRequest
    {
        #region Fields

        private readonly Action<TState> _startActivity;
        private readonly TState _state;

        #endregion

        #region Constructors

        public ActivityViewRequest(IViewModelBase viewModel, IViewMapping mapping, Action<TState> startActivity, TState state)
            : base(viewModel, null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(mapping, nameof(mapping));
            Should.NotBeNull(startActivity, nameof(startActivity));
            _startActivity = startActivity;
            _state = state;
            Mapping = mapping;
        }

        #endregion

        #region Properties

        public IViewMapping Mapping { get; }

        #endregion

        #region Implementation of interfaces

        public void StartActivity() => _startActivity(_state);

        #endregion
    }
}