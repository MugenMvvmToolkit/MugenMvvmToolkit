using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class IocMetadata
    {
        #region Fields

        private static IMetadataContextKey<string?>? _name;
        private static IMetadataContextKey<IReadOnlyCollection<IIocParameter>?>? _parameters;
        private static IMetadataContextKey<IocConditionDelegate?> _condition;

        #endregion

        #region Properties

        public static IMetadataContextKey<string?> Name
        {
            get => _name ??= GetBuilder<string?>(nameof(Name)).Serializable().Build();
            set => _name = value;
        }

        public static IMetadataContextKey<IReadOnlyCollection<IIocParameter>?> Parameters
        {
            get => _parameters ??= GetBuilder<IReadOnlyCollection<IIocParameter>?>(nameof(Parameters)).Serializable().Build();
            set => _parameters = value;
        }

        public static IMetadataContextKey<IocConditionDelegate?> Condition
        {
            get => _condition ??= GetBuilder<IocConditionDelegate?>(nameof(Condition)).Build();
            set => _condition = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(IocMetadata), name);
        }

        #endregion
    }
}