using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Navigation;

namespace MugenMvvm.Metadata
{
    internal static class InternalMetadata
    {
        #region Fields

        private static IMetadataContextKey<List<IView>, List<IView>>? _views;
        private static IMetadataContextKey<bool, bool>? _isDisposed;
        private static IMetadataContextKey<string, string>? _createdId;
        private static IMetadataContextKey<object?, object?>? _view;
        private static IMetadataContextKey<Dictionary<string, IViewModelPresenterMediator>, Dictionary<string, IViewModelPresenterMediator>>? _mediators;
        private static IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>>? _showingCallbacks;
        private static IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>>? _closingCallbacks;
        private static IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>>? _closeCallbacks;
        private static IMetadataContextKey<SortedList<string, object?>, SortedList<string, object?>>? _attachedValuesKey;

        #endregion

        #region Properties

        public static IMetadataContextKey<SortedList<string, object?>, SortedList<string, object?>> AttachedValuesKey
            => _attachedValuesKey ??= GetBuilder(_attachedValuesKey, nameof(AttachedValuesKey)).Build();

        public static IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> ShowingCallbacks
            => _showingCallbacks ??= GetBuilder(_showingCallbacks, nameof(ShowingCallbacks)).Serializable().Build();

        public static IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> ClosingCallbacks
            => _closingCallbacks ??= GetBuilder(_closingCallbacks, nameof(ClosingCallbacks)).Serializable().Build();

        public static IMetadataContextKey<List<NavigationCallback?>, List<NavigationCallback?>> CloseCallbacks
            => _closeCallbacks ??= GetBuilder(_closeCallbacks, nameof(CloseCallbacks)).Serializable().Build();

        public static IMetadataContextKey<Dictionary<string, IViewModelPresenterMediator>, Dictionary<string, IViewModelPresenterMediator>> Mediators
            => _mediators ??= GetBuilder(_mediators, nameof(Mediators)).Build();

        public static IMetadataContextKey<bool, bool> IsDisposed => _isDisposed ??= GetBuilder(_isDisposed, nameof(IsDisposed)).Build();

        public static IMetadataContextKey<List<IView>, List<IView>> Views => _views ??= GetBuilder(_views, nameof(Views)).Build();

        public static IMetadataContextKey<string, string> CreatedId => _createdId ??= GetBuilder(_createdId, nameof(CreatedId)).Build();

        public static IMetadataContextKey<object?, object?> View => _view ??= GetBuilder(_view, nameof(View)).Build();

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name) => MetadataContextKey.Create<TGet, TSet>(typeof(InternalMetadata), name);

        #endregion
    }
}