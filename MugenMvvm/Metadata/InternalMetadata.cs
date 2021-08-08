using System.Collections.Generic;
using MugenMvvm.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Navigation;

namespace MugenMvvm.Metadata
{
    internal static class InternalMetadata
    {
        private static IMetadataContextKey<HashSet<string>>? _openedNavigationProviders;
        private static IMetadataContextKey<object?>? _views;
        private static IMetadataContextKey<bool>? _isDisposed;
        private static IMetadataContextKey<bool>? _isInitialized;
        private static IMetadataContextKey<string>? _createdId;
        private static IMetadataContextKey<object?>? _view;
        private static IMetadataContextKey<object?>? _allowForceExecuteCommands;
        private static IMetadataContextKey<Dictionary<string, IViewModelPresenterMediator>>? _mediators;
        private static IMetadataContextKey<List<NavigationCallback?>>? _showingCallbacks;
        private static IMetadataContextKey<List<NavigationCallback?>>? _closingCallbacks;
        private static IMetadataContextKey<List<NavigationCallback?>>? _closeCallbacks;
        private static IMetadataContextKey<SortedList<string, object?>>? _attachedValuesKey;
        private static IMetadataContextKey<PropertyChangedCommandObserver>? _commandNotifier;
        private static IMetadataContextKey<SynchronizationCommandExecutorDecorator>? _commandDecorator;

        public static IMetadataContextKey<object?> AllowForceExecuteCommands =>
            _allowForceExecuteCommands ??= GetBuilder(_allowForceExecuteCommands, nameof(AllowForceExecuteCommands)).Build();

        public static IMetadataContextKey<PropertyChangedCommandObserver> CommandNotifier
            => _commandNotifier ??= GetBuilder(_commandNotifier, nameof(CommandNotifier)).Build();

        public static IMetadataContextKey<SynchronizationCommandExecutorDecorator> ExecutionDecorator
            => _commandDecorator ??= GetBuilder(_commandDecorator, nameof(ExecutionDecorator)).Build();

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

        public static IMetadataContextKey<bool> IsInitialized => _isInitialized ??= GetBuilder(_isInitialized, nameof(IsInitialized)).Build();

        public static IMetadataContextKey<object?> Views => _views ??= GetBuilder(_views, nameof(Views)).Build();

        public static IMetadataContextKey<string> CreatedId => _createdId ??= GetBuilder(_createdId, nameof(CreatedId)).Build();

        public static IMetadataContextKey<object?> View => _view ??= GetBuilder(_view, nameof(View)).Build();

        private static MetadataContextKey.Builder<T> GetBuilder<T>(IMetadataContextKey<T>? _, string name) => MetadataContextKey.Create<T>(typeof(InternalMetadata), name);
    }
}