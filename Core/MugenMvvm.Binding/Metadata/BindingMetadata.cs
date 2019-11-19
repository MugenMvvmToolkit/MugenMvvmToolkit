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

        private static IMetadataContextKey<bool>? _isMultiBinding;
        private static IMetadataContextKey<IBinding?>? _binding;
        private static IMetadataContextKey<object?>? _eventArgs;
        private static IMetadataContextKey<bool>? _suppressHolderRegistration;

        #endregion

        #region Properties

        public static IMetadataContextKey<IBinding?> Binding
        {
            get => _binding ??= GetBuilder<IBinding?>(nameof(Binding)).Build();
            set => _binding = value;
        }

        public static IMetadataContextKey<object?> EventArgs
        {
            get => _eventArgs ??= GetBuilder<object?>(nameof(EventArgs)).Build();
            set => _eventArgs = value;
        }

        public static IMetadataContextKey<bool> IsMultiBinding
        {
            get => _isMultiBinding ??= GetBuilder<bool>(nameof(IsMultiBinding)).Build();
            set => _isMultiBinding = value;
        }

        public static IMetadataContextKey<bool> SuppressHolderRegistration
        {
            get => _suppressHolderRegistration ??= GetBuilder<bool>(nameof(SuppressHolderRegistration)).Build();
            set => _suppressHolderRegistration = value;
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