using Java.Lang;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Constants;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Views
{
    public sealed class NativeLifecycleDispatcher : Object, INativeLifecycleDispatcher
    {
        private readonly IViewManager? _viewManager;

        public NativeLifecycleDispatcher(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
        }

        public int Priority => PriorityConstant.PreInitializer + 1;

        public void OnLifecycleChanged(Object target, int lifecycleState, Object? state)
        {
            if (lifecycleState == NativeLifecycleState.AppBackground)
            {
                MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivating, state);
                MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, state);
            }
            else
            {
                var viewLifecycleState = AndroidViewLifecycleState.TryParseNativeChanged(lifecycleState);
                if (viewLifecycleState != null)
                    _viewManager.DefaultIfNull().OnLifecycleChanged(target, viewLifecycleState, state);
            }
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