using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Metadata
{
    public static class ViewModelMetadata
    {
        #region Fields

        private static IMetadataContextKey<Guid> _id;
        private static IMetadataContextKey<ViewModelLifecycleState> _lifecycleState;
        private static IMetadataContextKey<bool> _broadcastAllMessages;
        private static IMetadataContextKey<BusyMessageHandlerType> _busyMessageHandlerType;
        private static IMetadataContextKey<IViewModelBase?> _parentViewModel;
        private static IMetadataContextKey<bool> _noState;
        private static IMetadataContextKey<Func<IViewModelBase, IMetadataContext, IPresenterResult>?> _closeHandler;
        private static IMetadataContextKey<Type?> _type;

        #endregion

        #region Properties

        public static IMetadataContextKey<Guid> Id
        {
            get => _id ??= GetBuilder<Guid>(nameof(Id)).DefaultValue(GetViewModelIdDefaultValue).Serializable().Build();
            set => _id = value;
        }

        public static IMetadataContextKey<ViewModelLifecycleState> LifecycleState
        {
            get => _lifecycleState ??= GetBuilder<ViewModelLifecycleState>(nameof(LifecycleState))
                    .NotNull()
                    .Serializable()
                    .DefaultValue(ViewModelLifecycleState.Disposed)
                    .Build();
            set => _lifecycleState = value;
        }

        public static IMetadataContextKey<bool> BroadcastAllMessages
        {
            get => _broadcastAllMessages ??= GetBuilder<bool>(nameof(BroadcastAllMessages)).Serializable().Build();
            set => _broadcastAllMessages = value;
        }

        public static IMetadataContextKey<BusyMessageHandlerType> BusyMessageHandlerType
        {
            get => _busyMessageHandlerType ??= GetBuilder<BusyMessageHandlerType>(nameof(BusyMessageHandlerType)).Serializable().Build();
            set => _busyMessageHandlerType = value;
        }

        public static IMetadataContextKey<IViewModelBase?> ParentViewModel
        {
            get => _parentViewModel ??= GetBuilder<IViewModelBase?>(nameof(ParentViewModel))
                    .NotNull()
                    .Serializable()
                    .Getter((context, k, o) => (IViewModelBase?)(o as IWeakReference)?.Target)
                    .Setter((context, k, oldValue, newValue) => newValue?.ToWeakReference())
                    .Build();
            set => _parentViewModel = value;
        }

        public static IMetadataContextKey<bool> NoState
        {
            get => _noState ??= GetBuilder<bool>(nameof(NoState)).Serializable().Build();
            set => _noState = value;
        }

        public static IMetadataContextKey<Func<IViewModelBase, IMetadataContext, IPresenterResult>?> CloseHandler
        {
            get => _closeHandler ??= GetBuilder<Func<IViewModelBase, IMetadataContext, IPresenterResult>?>(nameof(CloseHandler))
                    .NotNull()
                    .Build();
            set => _closeHandler = value;
        }

        public static IMetadataContextKey<Type?> Type
        {
            get => _type ??= GetBuilder<Type?>(nameof(Type)).NotNull().Build();
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