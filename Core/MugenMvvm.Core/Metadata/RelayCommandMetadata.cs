using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class RelayCommandMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool> _allowMultipleExecution;
        private static IMetadataContextKey<IReadOnlyCollection<string>?> _ignoreProperties;
        private static IMetadataContextKey<CommandExecutionMode> _executionMode;
        private static IMetadataContextKey<ThreadExecutionMode?> _eventThreadMode;

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

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(RelayCommandMetadata), name);
        }

        #endregion
    }
}