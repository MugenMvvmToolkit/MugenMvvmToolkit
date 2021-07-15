using System;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;
using MugenMvvm.Views;

namespace MugenMvvm.Android.Requests
{
    public sealed class ActivityViewRequest<TState> : ViewModelViewRequest, IActivityViewRequest
    {
        private readonly Func<ViewInfo, ViewLifecycleState, object?, TState, IReadOnlyMetadataContext?, bool> _isTargetActivity;
        private readonly Action<TState> _startActivity;
        private readonly TState _state;

        public ActivityViewRequest(IViewModelBase viewModel, IViewMapping mapping, Action<TState> startActivity,
            Func<ViewInfo, ViewLifecycleState, object?, TState, IReadOnlyMetadataContext?, bool> isTargetActivity, TState state)
            : base(viewModel, null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(mapping, nameof(mapping));
            Should.NotBeNull(startActivity, nameof(startActivity));
            Should.NotBeNull(isTargetActivity, nameof(isTargetActivity));
            _startActivity = startActivity;
            _state = state;
            _isTargetActivity = isTargetActivity;
            Mapping = mapping;
        }

        public IViewMapping Mapping { get; }

        public bool IsTargetActivity(ViewInfo view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata) =>
            _isTargetActivity(view, lifecycleState, state, _state, metadata);

        public void StartActivity() => _startActivity(_state);
    }
}