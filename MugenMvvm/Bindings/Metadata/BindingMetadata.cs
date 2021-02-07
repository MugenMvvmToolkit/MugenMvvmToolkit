using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Bindings.Metadata
{
    public static class BindingMetadata
    {
        public static object UnsetValue = new();
        public static object DoNothing = new();

        private static IMetadataContextKey<IBinding>? _binding;
        private static IMetadataContextKey<object>? _eventArgs;
        private static IMetadataContextKey<bool>? _suppressHolderRegistration;
        private static IMetadataContextKey<bool>? _isExpressionBinding;

        [AllowNull]
        public static IMetadataContextKey<IBinding> Binding
        {
            get => _binding ??= GetBuilder(_binding, nameof(Binding)).Build();
            set => _binding = value;
        }

        [AllowNull]
        public static IMetadataContextKey<object> EventArgs
        {
            get => _eventArgs ??= GetBuilder(_eventArgs, nameof(EventArgs)).Build();
            set => _eventArgs = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> SuppressHolderRegistration
        {
            get => _suppressHolderRegistration ??= GetBuilder(_suppressHolderRegistration, nameof(SuppressHolderRegistration)).Build();
            set => _suppressHolderRegistration = value;
        }

        [AllowNull]
        public static IMetadataContextKey<bool> IsExpressionBinding
        {
            get => _isExpressionBinding ??= GetBuilder(_isExpressionBinding, nameof(IsExpressionBinding)).Build();
            set => _isExpressionBinding = value;
        }

        private static MetadataContextKey.Builder<T> GetBuilder<T>(IMetadataContextKey<T>? _, string name) => MetadataContextKey.Create<T>(typeof(BindingMetadata), name);
    }
}