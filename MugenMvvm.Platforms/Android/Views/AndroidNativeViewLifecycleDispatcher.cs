using Java.Lang;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Constants;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Views
{
    public sealed class AndroidNativeViewLifecycleDispatcher : Object, IAndroidNativeLifecycleDispatcher
    {
        #region Fields

        private readonly IViewManager? _viewManager;

        #endregion

        #region Constructors

        public AndroidNativeViewLifecycleDispatcher(IViewManager? viewManager = null)
        {
            _viewManager = viewManager;
        }

        #endregion

        #region Properties

        public int Priority => PriorityConstants.PreInitializer + 1;

        #endregion

        #region Implementation of interfaces

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
                var request = new CancelableRequest(false, state);
                _viewManager.DefaultIfNull().OnLifecycleChanged(target, viewLifecycleState, request);
                return !request.Cancel;
            }

            return true;
        }

        #endregion
    }
}