using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Metadata
{
    public static class BindingMetadata
    {
        #region Fields

        public static readonly object UnsetValue = new object();
        public static readonly object DoNothing = new object();

        private static IMetadataContextKey<IDataBinding?> _binding;
        private static IMetadataContextKey<bool> _disableEqualityCheckingTarget;
        private static IMetadataContextKey<bool> _disableEqualityCheckingSource;
        private static IMetadataContextKey<bool> _debugMode;

        #endregion

        #region Properties

        public static IMetadataContextKey<IDataBinding?> Binding
        {
            get => _binding ??= GetBuilder<IDataBinding?>(nameof(Binding)).Build();
            set => _binding = value;
        }

        public static IMetadataContextKey<bool> DisableEqualityCheckingTarget
        {
            get => _disableEqualityCheckingTarget ??= GetBuilder<bool>(nameof(DisableEqualityCheckingTarget)).Build();
            set => _disableEqualityCheckingTarget = value;
        }

        public static IMetadataContextKey<bool> DisableEqualityCheckingSource
        {
            get => _disableEqualityCheckingSource ??= GetBuilder<bool>(nameof(DisableEqualityCheckingSource)).Build();
            set => _disableEqualityCheckingSource = value;
        }

        public static IMetadataContextKey<bool> DebugMode
        {
            get => _debugMode ??= GetBuilder<bool>(nameof(DebugMode)).Build();
            set => _debugMode = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(BindingMetadata), name);
        }

        #endregion
    }
}