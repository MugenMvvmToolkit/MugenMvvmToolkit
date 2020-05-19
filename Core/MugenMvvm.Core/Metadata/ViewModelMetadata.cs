using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Presenters;

namespace MugenMvvm.Metadata
{
    public static class ViewModelMetadata
    {
        #region Fields

        private static IMetadataContextKey<Guid, Guid>? _id;
        private static IMetadataContextKey<ViewModelLifecycleState, ViewModelLifecycleState>? _lifecycleState;
        private static IMetadataContextKey<IViewModelBase?, IViewModelBase?>? _parentViewModel;
        private static IMetadataContextKey<bool, bool>? _noState;
        private static IMetadataContextKey<Func<IViewModelBase, IReadOnlyMetadataContext, CancellationToken, PresenterResult>, Func<IViewModelBase, IReadOnlyMetadataContext, CancellationToken, PresenterResult>>? _closeHandler;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<Guid, Guid> Id
        {
            get => _id ??= GetBuilder(_id, nameof(Id)).DefaultValue(GetViewModelIdDefaultValue).Serializable().Build();
            set => _id = value;
        }

        [AllowNull]
        public static IMetadataContextKey<ViewModelLifecycleState, ViewModelLifecycleState> LifecycleState
        {
            get => _lifecycleState ??= GetBuilder(_lifecycleState, nameof(LifecycleState))
                .NotNull()
                .Serializable()
                .DefaultValue(ViewModelLifecycleState.Disposed)
                .Build();
            set => _lifecycleState = value;
        }

        [AllowNull]
        public static IMetadataContextKey<IViewModelBase?, IViewModelBase?> ParentViewModel
        {
            get => _parentViewModel ??= GetBuilder(_parentViewModel, nameof(ParentViewModel))
                .Serializable()
                .Getter((context, k, o) => (IViewModelBase?)(o as IWeakReference)?.Target)
                .Setter((context, k, oldValue, newValue) => newValue?.ToWeakReference())
                .Build();
            set => _parentViewModel = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool, bool> NoState
        {
            get => _noState ??= GetBuilder(_noState, nameof(NoState)).Serializable().Build();
            set => _noState = value;
        }

        [AllowNull]
        public static IMetadataContextKey<Func<IViewModelBase, IReadOnlyMetadataContext, CancellationToken, PresenterResult>, Func<IViewModelBase, IReadOnlyMetadataContext, CancellationToken, PresenterResult>> CloseHandler
        {
            get => _closeHandler ??= GetBuilder(_closeHandler, nameof(CloseHandler))
                .NotNull()
                .Build();
            set => _closeHandler = value;
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