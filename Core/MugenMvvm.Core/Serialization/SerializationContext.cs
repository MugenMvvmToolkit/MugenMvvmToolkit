using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Serialization
{
    public sealed class SerializationContext : MetadataOwnerBase, ISerializationContext
    {
        #region Constructors

        public SerializationContext(object target, IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(metadata, metadataContextProvider)
        {
            Target = target;
        }

        #endregion

        #region Properties

        [field: ThreadStatic]
        public static ISerializationContext? Current { get; private set; }

        public object Target { get; }

        #endregion

        #region Methods

        public static ActionToken Begin(ISerializationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            if (Current != null)
                ExceptionManager.ThrowObjectInitialized(typeof(SerializationContext), nameof(Current));
            Current = context;
            return new ActionToken((_, __) => Current = null);
        }

        #endregion
    }
}