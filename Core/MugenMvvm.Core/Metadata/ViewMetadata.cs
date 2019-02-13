using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ViewMetadata
    {
        #region Fields

        private static IMetadataContextKey<ICollection<object>> _wrappers;

        #endregion

        #region Properties

        public static IMetadataContextKey<ICollection<object>> Wrappers
        {
            get
            {
                if (_wrappers == null)
                    _wrappers = GetBuilder<ICollection<object>>(nameof(Wrappers)).Build();
                return _wrappers;
            }
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