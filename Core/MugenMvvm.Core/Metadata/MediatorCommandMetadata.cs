using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class MediatorCommandMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool> _allowMultipleExecution;
        private static IMetadataContextKey<IReadOnlyCollection<string>?> _ignoreProperties;
        private static IMetadataContextKey<CommandExecutionMode> _executionMode;
        private static IMetadataContextKey<ThreadExecutionMode?> _eventThreadMode;
        private static IMetadataContextKey<Delegate?> _execute;
        private static IMetadataContextKey<Delegate?> _canExecute;
        private static IMetadataContextKey<IReadOnlyCollection<object>?> _notifiers;

        #endregion

        #region Properties

        public static IMetadataContextKey<Delegate?> Execute
        {
            get => _execute ??= GetBuilder<Delegate?>(nameof(Execute)).NotNull().Build();
            set => _execute = value;
        }

        public static IMetadataContextKey<Delegate?> CanExecute
        {
            get => _canExecute ??= GetBuilder<Delegate?>(nameof(CanExecute)).NotNull().Build();
            set => _canExecute = value;
        }

        public static IMetadataContextKey<IReadOnlyCollection<object>?> Notifiers
        {
            get => _notifiers ??= GetBuilder<IReadOnlyCollection<object>?>(nameof(Notifiers)).NotNull().Build();
            set => _notifiers = value;
        }

        public static IMetadataContextKey<bool> AllowMultipleExecution
        {
            get => _allowMultipleExecution ??= GetBuilder<bool>(nameof(AllowMultipleExecution)).Serializable().Build();
            set => _allowMultipleExecution = value;
        }

        public static IMetadataContextKey<IReadOnlyCollection<string>?> IgnoreProperties
        {
            get => _ignoreProperties ??= GetBuilder<IReadOnlyCollection<string>?>(nameof(IgnoreProperties)).NotNull().Build();
            set => _ignoreProperties = value;
        }

        public static IMetadataContextKey<CommandExecutionMode> ExecutionMode
        {
            get => _executionMode ??= GetBuilder<CommandExecutionMode>(nameof(ExecutionMode)).Serializable().Build();
            set => _executionMode = value;
        }

        public static IMetadataContextKey<ThreadExecutionMode?> EventThreadMode
        {
            get => _eventThreadMode ??= GetBuilder<ThreadExecutionMode?>(nameof(EventThreadMode)).Serializable().NotNull().Build();
            set => _eventThreadMode = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(MediatorCommandMetadata), name);
        }

        #endregion
    }
}