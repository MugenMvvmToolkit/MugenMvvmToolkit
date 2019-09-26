using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ViewMetadata
    {
        #region Fields

        private static IMetadataContextKey<IComponentCollection<object>?>? _wrappers;

        #endregion

        #region Properties

        public static IMetadataContextKey<IComponentCollection<object>?> Wrappers
        {
            get => _wrappers ??= GetBuilder<IComponentCollection<object>?>(nameof(Wrappers)).Build();
            set => _wrappers = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(ViewMetadata), name);
        }

        #endregion
    }
}