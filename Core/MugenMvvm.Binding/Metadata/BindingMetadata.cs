using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Metadata
{
    public static class BindingMetadata
    {
        #region Fields

        public static readonly object UnsetValue = new object();
        public static readonly object DoNothing = new object();

        private static IMetadataContextKey<IDataBinding?> _binding;

        #endregion

        #region Properties

        public static IMetadataContextKey<IDataBinding?> Binding
        {
            get => _binding ??= GetBuilder<IDataBinding?>(nameof(Binding)).Build();//todo inline?
            set => _binding = value;
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