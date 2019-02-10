using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Serialization;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class NavigationInternalMetadata
    {
        #region Fields

        private static IMetadataContextKey<object?> _restoredWindowView;
        private static IMetadataContextKey<bool> _isRestorableCallback;
        private static IMetadataContextKey<IList<INavigationCallbackInternal>?> _showingCallbacks;
        private static IMetadataContextKey<IList<INavigationCallbackInternal>?> _closingCallbacks;
        private static IMetadataContextKey<IList<INavigationCallbackInternal?>?> _closeCallbacks;
        private static IMetadataContextKey<NavigationType?> _viewModelFromNavigationType;
        private static IMetadataContextKey<NavigationType?> _viewModelToNavigationType;

        #endregion

        #region Properties

        public static IMetadataContextKey<object?> RestoredWindowView
        {
            get
            {
                if (_restoredWindowView == null)
                    _restoredWindowView = GetBuilder<object?>(nameof(RestoredWindowView)).NotNull().Build();
                return _restoredWindowView;
            }
            set => _restoredWindowView = value;
        }

        public static IMetadataContextKey<bool> IsRestorableCallback
        {
            get
            {
                if (_isRestorableCallback == null)
                    _isRestorableCallback = GetBuilder<bool>(nameof(IsRestorableCallback)).Serializable().Build();
                return _isRestorableCallback;
            }
            set => _isRestorableCallback = value;
        }

        public static IMetadataContextKey<IList<INavigationCallbackInternal>?> ShowingCallbacks
        {
            get
            {
                if (_showingCallbacks == null)
                    _showingCallbacks = GetBuilder<IList<INavigationCallbackInternal>?>(nameof(ShowingCallbacks)).Build();
                return _showingCallbacks;
            }
            set => _showingCallbacks = value;
        }

        public static IMetadataContextKey<IList<INavigationCallbackInternal>?> ClosingCallbacks
        {
            get
            {
                if (_closingCallbacks == null)
                    _closingCallbacks = GetBuilder<IList<INavigationCallbackInternal>?>(nameof(ClosingCallbacks)).Build();
                return _closingCallbacks;
            }
            set => _closingCallbacks = value;
        }

        public static IMetadataContextKey<IList<INavigationCallbackInternal?>?> CloseCallbacks
        {
            get
            {
                if (_closeCallbacks == null)
                {
                    _closeCallbacks = GetBuilder<IList<INavigationCallbackInternal?>?>(nameof(CloseCallbacks))
                        .SerializableConverter(CloseCallbacksSerializableConverter)
                        .Serializable(CanSerializeCloseCallbacks)
                        .Build();
                }

                return _closeCallbacks;
            }
            set => _closeCallbacks = value;
        }

        public static IMetadataContextKey<NavigationType?> ViewModelFromNavigationType
        {
            get
            {
                if (_viewModelFromNavigationType == null)
                    _viewModelFromNavigationType = GetBuilder<NavigationType?>(nameof(ViewModelFromNavigationType)).Build();
                return _viewModelFromNavigationType;
            }
            set => _viewModelFromNavigationType = value;
        }

        public static IMetadataContextKey<NavigationType?> ViewModelToNavigationType
        {
            get
            {
                if (_viewModelToNavigationType == null)
                    _viewModelToNavigationType = GetBuilder<NavigationType?>(nameof(ViewModelToNavigationType)).Build();
                return _viewModelToNavigationType;
            }
            set => _viewModelToNavigationType = value;
        }

        #endregion

        #region Methods

        private static object? CloseCallbacksSerializableConverter(IMetadataContextKey<IList<INavigationCallbackInternal?>?> key, object? value, ISerializationContext arg3)
        {
            var callbacks = (IList<INavigationCallbackInternal>)value;
            if (callbacks == null)
                return null;
            lock (callbacks)
            {
                return callbacks.Where(callback => callback.IsSerializable).ToList();
            }
        }

        private static bool CanSerializeCloseCallbacks(IMetadataContextKey<IList<INavigationCallbackInternal?>?> key, object? value, ISerializationContext context)
        {
            var callbacks = (IList<INavigationCallbackInternal>)value;
            return callbacks != null && callbacks.Any(callback => callback != null && callback.IsSerializable);
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(NavigationInternalMetadata), name);
        }

        #endregion
    }
}