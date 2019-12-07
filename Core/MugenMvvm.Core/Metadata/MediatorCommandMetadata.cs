using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class MediatorCommandMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool>? _allowMultipleExecution;
        private static IMetadataContextKey<IReadOnlyCollection<string>>? _ignoreProperties;
        private static IMetadataContextKey<CommandExecutionMode>? _executionMode;
        private static IMetadataContextKey<ThreadExecutionMode>? _eventThreadMode;
        private static IMetadataContextKey<IExecutorCommandMediatorComponent>? _executor;
        private static IMetadataContextKey<IReadOnlyCollection<object>>? _notifiers;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<IExecutorCommandMediatorComponent> Executor
        {
            get => _executor ??= GetBuilder<IExecutorCommandMediatorComponent>(nameof(Executor)).NotNull().Build();
            set => _executor = value;
        }

        [AllowNull]
        public static IMetadataContextKey<IReadOnlyCollection<object>> Notifiers
        {
            get => _notifiers ??= GetBuilder<IReadOnlyCollection<object>>(nameof(Notifiers)).NotNull().Build();
            set => _notifiers = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> AllowMultipleExecution
        {
            get => _allowMultipleExecution ??= GetBuilder<bool>(nameof(AllowMultipleExecution)).Serializable().Build();
            set => _allowMultipleExecution = value;
        }

        [AllowNull]
        public static IMetadataContextKey<IReadOnlyCollection<string>> IgnoreProperties
        {
            get => _ignoreProperties ??= GetBuilder<IReadOnlyCollection<string>>(nameof(IgnoreProperties)).NotNull().Build();
            set => _ignoreProperties = value;
        }

        [AllowNull]
        public static IMetadataContextKey<CommandExecutionMode> ExecutionMode
        {
            get => _executionMode ??= GetBuilder<CommandExecutionMode>(nameof(ExecutionMode)).Serializable().Build();
            set => _executionMode = value;
        }

        [AllowNull]
        public static IMetadataContextKey<ThreadExecutionMode> EventThreadMode
        {
            get => _eventThreadMode ??= GetBuilder<ThreadExecutionMode>(nameof(EventThreadMode)).Serializable().NotNull().Build();
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