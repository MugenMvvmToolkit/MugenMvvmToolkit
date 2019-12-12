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

        private static IMetadataContextKey<Guid>? _id;
        private static IMetadataContextKey<ViewModelLifecycleState>? _lifecycleState;
        private static IMetadataContextKey<bool>? _broadcastAllMessages;
        private static IMetadataContextKey<BusyMessageHandlerType>? _busyMessageHandlerType;
        private static IMetadataContextKey<IViewModelBase?>? _parentViewModel;
        private static IMetadataContextKey<bool>? _noState;
        private static IMetadataContextKey<Func<IViewModelBase, IReadOnlyMetadataContext, CancellationToken, PresenterResult>>? _closeHandler;
        private static IMetadataContextKey<Type>? _type;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<Guid> Id
        {
            get => _id ??= GetBuilder<Guid>(nameof(Id)).DefaultValue(GetViewModelIdDefaultValue).Serializable().Build();
            set => _id = value;
        }

        [AllowNull]
        public static IMetadataContextKey<ViewModelLifecycleState> LifecycleState
        {
            get => _lifecycleState ??= GetBuilder<ViewModelLifecycleState>(nameof(LifecycleState))
                    .NotNull()
                    .Serializable()
                    .DefaultValue(ViewModelLifecycleState.Disposed)
                    .Build();
            set => _lifecycleState = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> BroadcastAllMessages
        {
            get => _broadcastAllMessages ??= GetBuilder<bool>(nameof(BroadcastAllMessages)).Serializable().Build();
            set => _broadcastAllMessages = value;
        }

        [AllowNull]
        public static IMetadataContextKey<BusyMessageHandlerType> BusyMessageHandlerType
        {
            get => _busyMessageHandlerType ??= GetBuilder<BusyMessageHandlerType>(nameof(BusyMessageHandlerType)).Serializable().Build();
            set => _busyMessageHandlerType = value;
        }

        [AllowNull]
        public static IMetadataContextKey<IViewModelBase?> ParentViewModel
        {
            get => _parentViewModel ??= GetBuilder<IViewModelBase?>(nameof(ParentViewModel))
                .Serializable()
                .Getter((context, k, o) => (IViewModelBase?)(o as IWeakReference)?.Target)
                .Setter((context, k, oldValue, newValue) => newValue?.ToWeakReference())
                .Build();
            set => _parentViewModel = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> NoState
        {
            get => _noState ??= GetBuilder<bool>(nameof(NoState)).Serializable().Build();
            set => _noState = value;
        }

        [AllowNull]
        public static IMetadataContextKey<Func<IViewModelBase, IReadOnlyMetadataContext, CancellationToken, PresenterResult>> CloseHandler
        {
            get => _closeHandler ??= GetBuilder<Func<IViewModelBase, IReadOnlyMetadataContext, CancellationToken, PresenterResult>>(nameof(CloseHandler))
                    .NotNull()
                    .Build();
            set => _closeHandler = value;
        }

        [AllowNull]
        public static IMetadataContextKey<Type> Type
        {
            get => _type ??= GetBuilder<Type>(nameof(Type)).NotNull().Build();
            set => _type = value;
        }

        #endregion

        #region Methods

        private static Guid GetViewModelIdDefaultValue(IReadOnlyMetadataContext ctx, IMetadataContextKey<Guid> key, Guid value)
        {
            if (value == Guid.Empty && ctx is IMetadataContext context)
                return context.GetOrAdd(key, Guid.NewGuid());
            return value;
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(ViewModelMetadata), name);
        }

        #endregion
    }
}