using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class NavigationMetadata
    {
        private static IMetadataContextKey<string>? _viewName;
        private static IMetadataContextKey<object>? _owner;
        private static IMetadataContextKey<bool>? _nonModal;
        private static IMetadataContextKey<bool>? _animated;
        private static IMetadataContextKey<bool>? _forceClose;
        private static IMetadataContextKey<NavigationType>? _navigationType;
        private static IMetadataContextKey<bool>? _clearBackStack;
        private static IMetadataContextKey<bool>? _isRestoration;
        private static IMetadataContextKey<DateTime>? _navigationDate;

        [AllowNull]
        public static IMetadataContextKey<object> Owner
        {
            get => _owner ??= GetBuilder(_owner, nameof(Owner)).NotNull().Build();
            set => _owner = value;
        }

        [AllowNull]
        public static IMetadataContextKey<NavigationType> NavigationType
        {
            get => _navigationType ??= GetBuilder(_navigationType, nameof(NavigationType)).NotNull().Build();
            set => _navigationType = value;
        }

        [AllowNull]
        public static IMetadataContextKey<string> ViewName
        {
            get => _viewName ??= GetBuilder(_viewName, nameof(ViewName)).Serializable().NotNull().Build();
            set => _viewName = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> ForceClose
        {
            get => _forceClose ??= GetBuilder(_forceClose, nameof(ForceClose)).Build();
            set => _forceClose = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> Modal
        {
            get => _nonModal ??= GetBuilder(_nonModal, nameof(Modal)).DefaultValue(true).Serializable().Build();
            set => _nonModal = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> Animated
        {
            get => _animated ??= GetBuilder(_animated, nameof(Animated)).DefaultValue(true).Serializable().Build();
            set => _animated = value;
        }

        [AllowNull]
        public static IMetadataContextKey<DateTime> NavigationDate
        {
            get => _navigationDate ??= GetBuilder(_navigationDate, nameof(NavigationDate)).Build();
            set => _navigationDate = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> ClearBackStack
        {
            get => _clearBackStack ??= GetBuilder(_clearBackStack, nameof(ClearBackStack)).Build();
            set => _clearBackStack = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> IsRestoration
        {
            get => _isRestoration ??= GetBuilder(_isRestoration, nameof(IsRestoration)).Build();
            set => _isRestoration = value;
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(IMetadataContextKey<T>? _, string name) => MetadataContextKey.Create<T>(typeof(NavigationMetadata), name);
    }
}