using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class NavigationMetadata
    {
        #region Fields

        internal static readonly IMetadataContextKey<HashSet<string>, HashSet<string>> OpenedNavigationProviders = GetBuilder(OpenedNavigationProviders!, nameof(OpenedNavigationProviders))
            .Serializable()
            .Build();

        private static IMetadataContextKey<string, string>? _viewName;
        private static IMetadataContextKey<bool, bool>? _nonModal;
        private static IMetadataContextKey<bool, bool>? _clearBackStack;
        private static IMetadataContextKey<DateTime, DateTime>? _navigationDate;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<string, string> ViewName
        {
            get => _viewName ??= GetBuilder(_viewName, nameof(ViewName)).Serializable().NotNull().Build();
            set => _viewName = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool, bool> NonModal
        {
            get => _nonModal ??= GetBuilder(_nonModal, nameof(NonModal)).Serializable().Build();
            set => _nonModal = value;
        }

        [AllowNull]
        public static IMetadataContextKey<DateTime, DateTime> NavigationDate
        {
            get => _navigationDate ??= GetBuilder(_navigationDate, nameof(NavigationDate)).Build();
            set => _navigationDate = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool, bool> ClearBackStack
        {
            get => _clearBackStack ??= GetBuilder(_clearBackStack, nameof(ClearBackStack)).Build();
            set => _clearBackStack = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name) => MetadataContextKey.Create<TGet, TSet>(typeof(NavigationMetadata), name);

        #endregion
    }
}