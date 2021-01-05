using System.Collections.Generic;
using MugenMvvm.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Navigation;

namespace MugenMvvm.Metadata
{
    internal static class InternalMetadata
    {
        #region Fields

        private static IMetadataContextKey<HashSet<string>>? _openedNavigationProviders;
        private static IMetadataContextKey<List<IView>>? _views;
        private static IMetadataContextKey<bool>? _isDisposed;
        private static IMetadataContextKey<string>? _createdId;
        private static IMetadataContextKey<object?>? _view;
        private static IMetadataContextKey<Dictionary<string, IViewModelPresenterMediator>>? _mediators;
        private static IMetadataContextKey<List<NavigationCallback?>>? _showingCallbacks;
        private static IMetadataContextKey<List<NavigationCallback?>>? _closingCallbacks;
        private static IMetadataContextKey<List<NavigationCallback?>>? _closeCallbacks;
        private static IMetadataContextKey<SortedList<string, object?>>? _attachedValuesKey;
        private static IMetadataContextKey<CommandEventHandler>? _commandEventHandler;

        #endregion

        #region Properties

        public static IMetadataContextKey<CommandEventHandler> CommandEventHandler
            => _commandEventHandler ??= GetBuilder(_commandEventHandler, nameof(CommandEventHandler)).Build();

        public static IMetadataContextKey<HashSet<string>> OpenedNavigationProviders
            => _openedNavigationProviders ??= GetBuilder(_openedNavigationProviders, nameof(OpenedNavigationProviders)).Serializable().Build();

        public static IMetadataContextKey<SortedList<string, object?>> AttachedValuesKey
            => _attachedValuesKey ??= GetBuilder(_attachedValuesKey, nameof(AttachedValuesKey)).Build();

        public static IMetadataContextKey<List<NavigationCallback?>> ShowingCallbacks
            => _showingCallbacks ??= GetBuilder(_showingCallbacks, nameof(ShowingCallbacks)).Serializable().Build();

        public static IMetadataContextKey<List<NavigationCallback?>> ClosingCallbacks
            => _closingCallbacks ??= GetBuilder(_closingCallbacks, nameof(ClosingCallbacks)).Serializable().Build();

        public static IMetadataContextKey<List<NavigationCallback?>> CloseCallbacks
            => _closeCallbacks ??= GetBuilder(_closeCallbacks, nameof(CloseCallbacks)).Serializable().Build();

        public static IMetadataContextKey<Dictionary<string, IViewModelPresenterMediator>> Mediators
            => _mediators ??= GetBuilder(_mediators, nameof(Mediators)).Build();

        public static IMetadataContextKey<bool> IsDisposed => _isDisposed ??= GetBuilder(_isDisposed, nameof(IsDisposed)).Build();

        public static IMetadataContextKey<List<IView>> Views => _views ??= GetBuilder(_views, nameof(Views)).Build();

        public static IMetadataContextKey<string> CreatedId => _createdId ??= GetBuilder(_createdId, nameof(CreatedId)).Build();

        public static IMetadataContextKey<object?> View => _view ??= GetBuilder(_view, nameof(View)).Build();

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(IMetadataContextKey<T>? _, string name) => MetadataContextKey.Create<T>(typeof(InternalMetadata), name);

        #endregion
    }
}