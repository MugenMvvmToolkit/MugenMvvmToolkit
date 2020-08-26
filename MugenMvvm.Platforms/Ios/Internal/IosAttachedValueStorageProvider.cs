using System.Collections.Generic;
using Foundation;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.Ios.Internal
{
    public class IosAttachedValueStorageProvider : AttachedValueStorageProviderBase, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = InternalComponentPriority.MetadataOwnerAttachedValueProvider;

        #endregion

        #region Methods

        protected override bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata) => item is NSObject;

        protected override IDictionary<string, object?>? GetAttachedDictionary(object item, bool optional) => IosAttachedValueHolder.Get((NSObject) item, optional)?.GetValues(optional);

        protected override bool ClearInternal(object item)
        {
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
                return false;
            dictionary.Clear();
            return true;
        }

        #endregion
    }
}