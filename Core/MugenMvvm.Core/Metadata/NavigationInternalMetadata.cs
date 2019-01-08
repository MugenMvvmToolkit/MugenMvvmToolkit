using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class NavigationInternalMetadata
    {
        #region Fields

        private static IMetadataContextKey<INavigationWindowMediator?> _navigationWindowMediator;
        private static IMetadataContextKey<object?> _restoredWindowView;
        private static IMetadataContextKey<bool> _isRestorableCallback;

        #endregion

        #region Properties

        public static IMetadataContextKey<INavigationWindowMediator?> NavigationWindowMediator
        {
            get
            {
                if (_navigationWindowMediator == null)
                    _navigationWindowMediator = GetBuilder<INavigationWindowMediator?>(nameof(NavigationWindowMediator)).Build();
                return _navigationWindowMediator;
            }
            set => _navigationWindowMediator = value;
        }

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

        public static IMetadataContextKey<IList<INavigationCallbackInternal>?> ShowingCallbacks { get; set; }

        public static IMetadataContextKey<IList<INavigationCallbackInternal>?> ClosingCallbacks { get; set; }

        public static IMetadataContextKey<IList<INavigationCallbackInternal?>?> CloseCallbacks { get; set; }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(NavigationInternalMetadata), name);
        }

        #endregion
    }
}