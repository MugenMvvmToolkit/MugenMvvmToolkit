using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ViewModelMetadata
    {
        #region Fields

        private static IMetadataContextKey<Guid, Guid>? _id;
        private static IMetadataContextKey<ViewModelLifecycleState, ViewModelLifecycleState>? _lifecycleState;
        private static IMetadataContextKey<bool, bool>? _noState;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<Guid, Guid> Id
        {
            get => _id ??= GetBuilder(_id, nameof(Id)).DefaultValue((context, key, arg3) => GetViewModelIdDefaultValue(context, key, arg3)).Serializable().Build();
            set => _id = value;
        }

        [AllowNull]
        public static IMetadataContextKey<ViewModelLifecycleState, ViewModelLifecycleState> LifecycleState
        {
            get => _lifecycleState ??= GetBuilder(_lifecycleState, nameof(LifecycleState)).NotNull().Build();
            set => _lifecycleState = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool, bool> NoState
        {
            get => _noState ??= GetBuilder(_noState, nameof(NoState)).Serializable().Build();
            set => _noState = value;
        }

        #endregion

        #region Methods

        private static Guid GetViewModelIdDefaultValue(IReadOnlyMetadataContext ctx, IMetadataContextKey<Guid, Guid> key, Guid value)
        {
            if (value == Guid.Empty && ctx is IMetadataContext context)
                return context.GetOrAdd(key, Guid.NewGuid());
            return value;
        }

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name)
        {
            return MetadataContextKey.Create<TGet, TSet>(typeof(ViewModelMetadata), name);
        }

        #endregion
    }
}