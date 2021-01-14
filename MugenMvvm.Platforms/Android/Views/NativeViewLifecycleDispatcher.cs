using Java.Lang;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Constants;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Views
{
    public sealed class NativeViewLifecycleDispatcher : Object, INativeLifecycleDispatcher
    {
        private readonly IViewManager? _viewManager;

        public NativeViewLifecycleDispatcher(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
        }

        public int Priority => PriorityConstants.PreInitializer + 1;

        public void OnLifecycleChanged(Object target, int lifecycleState, Object? state)
        {
            var viewLifecycleState = AndroidViewLifecycleState.TryParseNativeChanged(lifecycleState);
            if (viewLifecycleState != null)
                _viewManager.DefaultIfNull().OnLifecycleChanged(target, viewLifecycleState, state);
        }

        public bool OnLifecycleChanging(Object target, int lifecycleState, Object? state)
        {
            var viewLifecycleState = AndroidViewLifecycleState.TryParseNativeChanging(lifecycleState);
            if (viewLifecycleState != null)
            {
                var request = new CancelableRequest(null, state);
                _viewManager.DefaultIfNull().OnLifecycleChanged(target, viewLifecycleState, request);
                return !request.Cancel.GetValueOrDefault();
            }

            return true;
        }
    }
}