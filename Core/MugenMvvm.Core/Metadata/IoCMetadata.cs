using System.Collections.Generic;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class IoCMetadata
    {
        #region Fields

        private static IMetadataContextKey<string>? _name;
        private static IMetadataContextKey<IReadOnlyCollection<IIoCParameter>?>? _parameters;

        #endregion

        #region Properties

        public static IMetadataContextKey<string?> Name
        {
            get
            {
                if (_name == null)
                    _name = GetBuilder<string?>(nameof(Name)).Serializable().Build();
                return _name;
            }
            set => _name = value;
        }

        public static IMetadataContextKey<IReadOnlyCollection<IIoCParameter>?> Parameters
        {
            get
            {
                if (_parameters == null)
                    _parameters = GetBuilder<IReadOnlyCollection<IIoCParameter>?>(nameof(Parameters)).Serializable().Build();
                return _parameters;
            }
            set => _parameters = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(IoCMetadata), name);
        }

        #endregion
    }
}