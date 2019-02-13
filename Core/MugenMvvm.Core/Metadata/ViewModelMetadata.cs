﻿using System;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Presenters.Results;
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
        private static IMetadataContextKey<IViewModelBase> _parentViewModel;
        private static IMetadataContextKey<bool> _noState;
        private static IMetadataContextKey<Func<INavigationDispatcher, IViewModelBase, IReadOnlyMetadataContext, IChildViewModelPresenterResult>?> _closeHandler;

        #endregion

        #region Properties

        public static IMetadataContextKey<Guid> Id
        {
            get
            {
                if (_id == null)
                    _id = GetBuilder<Guid>(nameof(Id)).DefaultValue(GetViewModelIdDefaultValue).Serializable().Build();
                return _id;
            }
            set => _id = value;
        }

        public static IMetadataContextKey<ViewModelLifecycleState> LifecycleState
        {
            get
            {
                if (_lifecycleState == null)
                {
                    _lifecycleState = GetBuilder<ViewModelLifecycleState>(nameof(LifecycleState))
                           .NotNull()
                           .Serializable()
                           .DefaultValue(ViewModelLifecycleState.Disposed)
                           .Build();
                }
                return _lifecycleState;
            }
            set => _lifecycleState = value;
        }

        public static IMetadataContextKey<bool> BroadcastAllMessages
        {
            get => _broadcastAllMessages;
            set
            {
                if (_broadcastAllMessages == null)
                    _broadcastAllMessages = GetBuilder<bool>(nameof(BroadcastAllMessages)).Serializable().Build();
                _broadcastAllMessages = value;
            }
        }

        public static IMetadataContextKey<BusyMessageHandlerType> BusyMessageHandlerType
        {
            get
            {
                if (_busyMessageHandlerType == null)
                    _busyMessageHandlerType = GetBuilder<BusyMessageHandlerType>(nameof(BusyMessageHandlerType)).Serializable().Build();
                return _busyMessageHandlerType;
            }
            set => _busyMessageHandlerType = value;
        }

        public static IMetadataContextKey<IViewModelBase?> ParentViewModel
        {
            get
            {
                if (_parentViewModel == null)
                {
                    _parentViewModel = GetBuilder<IViewModelBase?>(nameof(ParentViewModel))
                        .NotNull()
                        .Serializable()
                        .Getter((context, k, o) => ((SerializableWeakReference)o).GetTarget<IViewModelBase>())
                        .Setter((context, k, oldValue, newValue) => new SerializableWeakReference(newValue))
                        .Build();
                }

                return _parentViewModel;
            }
            set => _parentViewModel = value;
        }

        public static IMetadataContextKey<bool> NoState
        {
            get
            {
                if (_noState == null)
                    _noState = GetBuilder<bool>(nameof(NoState)).Serializable().Build();
                return _noState;
            }
            set => _noState = value;
        }

        public static IMetadataContextKey<Func<INavigationDispatcher, IViewModelBase, IReadOnlyMetadataContext, IChildViewModelPresenterResult>?> CloseHandler
        {
            get
            {
                if (_closeHandler == null)
                {
                    _closeHandler = GetBuilder<Func<INavigationDispatcher, IViewModelBase, IReadOnlyMetadataContext, IChildViewModelPresenterResult>?>(nameof(CloseHandler))
                           .NotNull()
                           .Build();
                }
                return _closeHandler;
            }
            set => _closeHandler = value;
        }

        #endregion

        #region Methods

        private static Guid GetViewModelIdDefaultValue(IReadOnlyMetadataContext ctx, IMetadataContextKey<Guid> key, Guid value)
        {
            if (ctx is IMetadataContext context)
                return context.GetOrAdd(Id, Guid.NewGuid());
            return value;
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(ViewModelMetadata), name);
        }

        #endregion
    }
}