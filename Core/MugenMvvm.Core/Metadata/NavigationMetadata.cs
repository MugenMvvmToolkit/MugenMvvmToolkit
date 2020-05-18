using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class NavigationMetadata
    {
        #region Fields

        private static IMetadataContextKey<object, object>? _target;
        private static IMetadataContextKey<string, string>? _viewName;
        private static IMetadataContextKey<bool, bool>? _isModal;
        private static IMetadataContextKey<DateTime, DateTime>? _navigationDate;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<object, object> Target
        {
            get => _target ??= GetBuilder(_target, nameof(Target)).NotNull().Serializable().Build();
            set => _target = value;
        }

        [AllowNull]
        public static IMetadataContextKey<string, string> ViewName
        {
            get => _viewName ??= GetBuilder(_viewName, nameof(ViewName)).Serializable().NotNull().Build();
            set => _viewName = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool, bool> IsModal
        {
            get => _isModal ??= GetBuilder(_isModal, nameof(IsModal)).Serializable().Build();
            set => _isModal = value;
        }


        [AllowNull]
        public static IMetadataContextKey<DateTime, DateTime> NavigationDate
        {
            get => _navigationDate ??= GetBuilder(_navigationDate, nameof(NavigationDate)).Build();
            set => _navigationDate = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name)
        {
            return MetadataContextKey.Create<TGet, TSet>(typeof(NavigationMetadata), name);
        }

        #endregion
    }
}