using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class ThreadDispatcherMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool> _alwaysAsync;

        #endregion

        #region Properties

        public static IMetadataContextKey<bool> AlwaysAsync
        {
            get
            {
                if (_alwaysAsync == null)
                    _alwaysAsync = GetBuilder<bool>(nameof(AlwaysAsync)).Serializable().Build();
                return _alwaysAsync;
            }
            set => _alwaysAsync = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(ThreadDispatcherMetadata), name);
        }

        #endregion
    }
}