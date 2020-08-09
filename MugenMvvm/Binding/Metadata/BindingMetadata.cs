using System.Diagnostics.CodeAnalysis;
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

        private static IMetadataContextKey<IBinding, IBinding>? _binding;
        private static IMetadataContextKey<object, object>? _eventArgs;
        private static IMetadataContextKey<bool, bool>? _suppressHolderRegistration;
        private static IMetadataContextKey<bool, bool>? _isMultiBinding;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<IBinding, IBinding> Binding
        {
            get => _binding ??= GetBuilder(_binding, nameof(Binding)).Build();
            set => _binding = value;
        }

        [AllowNull]
        public static IMetadataContextKey<object, object> EventArgs
        {
            get => _eventArgs ??= GetBuilder(_eventArgs, nameof(EventArgs)).Build();
            set => _eventArgs = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool, bool> SuppressHolderRegistration
        {
            get => _suppressHolderRegistration ??= GetBuilder(_suppressHolderRegistration, nameof(SuppressHolderRegistration)).Build();
            set => _suppressHolderRegistration = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool, bool> IsMultiBinding
        {
            get => _isMultiBinding ??= GetBuilder(_isMultiBinding, nameof(IsMultiBinding)).Build();
            set => _isMultiBinding = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<TGet, TSet> GetBuilder<TGet, TSet>(IMetadataContextKey<TGet, TSet>? _, string name) => MetadataContextKey.Create<TGet, TSet>(typeof(BindingMetadata), name);

        #endregion
    }
}