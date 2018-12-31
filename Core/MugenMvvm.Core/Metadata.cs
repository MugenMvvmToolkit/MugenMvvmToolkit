using System;
using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm
{
    public static class ApplicationMetadata
    {
    }

    public static class ViewModelMetadata
    {
        #region Fields

        private static IMetadataContextKey<Guid> _id;
        private static IMetadataContextKey<ViewModelLifecycleState> _lifecycleState;
        private static IMetadataContextKey<bool> _broadcastAllMessages;
        private static IMetadataContextKey<BusyMessageHandlerType> _busyMessageHandlerType;
        private static IMetadataContextKey<IViewModel> _parentViewModel;

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
                    _lifecycleState = GetBuilder<ViewModelLifecycleState>(nameof(LifecycleState)).NotNull().Serializable().Build();
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

        public static IMetadataContextKey<IViewModel?> ParentViewModel
        {
            get
            {
                if (_parentViewModel == null)
                {
                    _parentViewModel = GetBuilder<IViewModel?>(nameof(ParentViewModel))
                        .NotNull()
                        .Serializable()
                        .Getter((context, o) => (IViewModel) ((WeakReference) o).Target)
                        .Setter((context, model) => MugenExtensions.GetWeakReference(model))
                        .Build();
                }

                return _parentViewModel;
            }
            set => _parentViewModel = value;
        }

        #endregion

        #region Methods

        private static Guid GetViewModelIdDefaultValue(IReadOnlyMetadataContext ctx, Guid value)
        {
            if (ctx is IMetadataContext context)
            {
                value = Guid.NewGuid();
                lock (Id)
                {
                    context.Set(Id, value);
                }
            }

            return value;
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(ViewModelMetadata), name);
        }

        #endregion
    }

    public static class RelayCommandMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool> _allowMultipleExecution;
        private static IMetadataContextKey<IReadOnlyCollection<string>> _ignoreProperties;
        private static IMetadataContextKey<CommandExecutionMode> _executionMode;
        private static IMetadataContextKey<ThreadExecutionMode> _eventThreadMode;
        private static IMetadataContextKey<Func<string>> _displayName;

        #endregion

        #region Properties

        public static IMetadataContextKey<bool> AllowMultipleExecution
        {
            get
            {
                if (_allowMultipleExecution == null)
                    _allowMultipleExecution = GetBuilder<bool>(nameof(AllowMultipleExecution)).Serializable().Build();
                return _allowMultipleExecution;
            }
            set => _allowMultipleExecution = value;
        }

        public static IMetadataContextKey<IReadOnlyCollection<string>?> IgnoreProperties
        {
            get
            {
                if (_ignoreProperties == null)
                    _ignoreProperties = GetBuilder<IReadOnlyCollection<string>?>(nameof(IgnoreProperties)).NotNull().Build();
                return _ignoreProperties;
            }
            set => _ignoreProperties = value;
        }

        public static IMetadataContextKey<CommandExecutionMode> ExecutionMode
        {
            get
            {
                if (_executionMode == null)
                    _executionMode = GetBuilder<CommandExecutionMode>(nameof(ExecutionMode)).Serializable().Build();
                return _executionMode;
            }
            set => _executionMode = value;
        }

        public static IMetadataContextKey<ThreadExecutionMode?> EventThreadMode
        {
            get
            {
                if (_eventThreadMode == null)
                    _eventThreadMode = GetBuilder<ThreadExecutionMode?>(nameof(EventThreadMode)).Serializable().NotNull().Build();
                return _eventThreadMode;
            }
            set => _eventThreadMode = value;
        }

        public static IMetadataContextKey<Func<string>?> DisplayName
        {
            get
            {
                if (_displayName == null)
                    _displayName = GetBuilder<Func<string>?>(nameof(DisplayName)).NotNull().Build();
                return _displayName;
            }
            set => _displayName = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(RelayCommandMetadata), name);
        }

        #endregion
    }

    public static class SerializationMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool> _noCache;

        #endregion

        #region Properties

        public static IMetadataContextKey<bool> NoCache
        {
            get
            {
                if (_noCache == null)
                    _noCache = GetBuilder<bool>(nameof(NoCache)).Serializable().Build();
                return _noCache;
            }
            set => _noCache = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(SerializationMetadata), name);
        }

        #endregion
    }
}