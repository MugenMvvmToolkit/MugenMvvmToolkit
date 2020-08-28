using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Metadata
{
    public static class ViewModelMetadata
    {
        #region Fields

        private static IMetadataContextKey<Guid, Guid>? _id;
        private static IMetadataContextKey<ViewModelLifecycleState, ViewModelLifecycleState>? _lifecycleState;
        private static IMetadataContextKey<IViewModelBase?, IViewModelBase?>? _serializableViewModel;

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
        public static IMetadataContextKey<IViewModelBase?, IViewModelBase?> SerializableViewModel
        {
            get => _serializableViewModel ??= GetBuilder(_serializableViewModel, nameof(SerializableViewModel)).Serializable().Build();
            set => _serializableViewModel = value;
        }

        #endregion

        #region Methods

        private static Guid GetViewModelIdDefaultValue(IReadOnlyMetadataContext ctx, IMetadataContextKey<Guid, Guid> key, Guid value)
        {
            if (value == Guid.Empty && ctx is IMetadataContext context)
                return context.GetOrAdd(key, Guid.NewGuid());
            return value;
        }

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name) => MetadataContextKey.Create<TGet, TSet>(typeof(ViewModelMetadata), name);

        #endregion
    }
}