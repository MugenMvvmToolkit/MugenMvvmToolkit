using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Metadata
{
    public static class NavigationInternalMetadata //todo check unused metadata
    {
        #region Fields

        private static IMetadataContextKey<IViewInfo?>? _restoredView;
        private static IMetadataContextKey<bool>? _isRestorableCallback;
        private static IMetadataContextKey<bool>? _closeAll;
        private static IMetadataContextKey<INavigationCallback?>? _showingCallback;
        private static IMetadataContextKey<INavigationCallback?>? _closingCallback;
        private static IMetadataContextKey<INavigationCallback?>? _closeCallback;

        #endregion

        #region Properties

        public static IMetadataContextKey<IViewInfo?> RestoredView
        {
            get => _restoredView ??= GetBuilder<IViewInfo?>(nameof(RestoredView)).NotNull().Build();
            set => _restoredView = value;
        }

        public static IMetadataContextKey<bool> IsRestorableCallback
        {
            get => _isRestorableCallback ??= GetBuilder<bool>(nameof(IsRestorableCallback)).Serializable().Build();
            set => _isRestorableCallback = value;
        }

        public static IMetadataContextKey<bool> CloseAll
        {
            get => _closeAll ??= GetBuilder<bool>(nameof(CloseAll)).Build();
            set => _closeAll = value;
        }

        public static IMetadataContextKey<INavigationCallback?> ShowingCallback
        {
            get => _showingCallback ??= GetBuilder<INavigationCallback?>(nameof(ShowingCallback)).NotNull().Build();
            set => _showingCallback = value;
        }

        public static IMetadataContextKey<INavigationCallback?> ClosingCallback
        {
            get => _closingCallback ??= GetBuilder<INavigationCallback?>(nameof(ClosingCallback)).NotNull().Build();
            set => _closingCallback = value;
        }

        public static IMetadataContextKey<INavigationCallback?> CloseCallback
        {
            get => _closeCallback ??= GetBuilder<INavigationCallback?>(nameof(CloseCallback)).NotNull().Build();
            set => _closeCallback = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(NavigationInternalMetadata), name);
        }

        #endregion
    }
}