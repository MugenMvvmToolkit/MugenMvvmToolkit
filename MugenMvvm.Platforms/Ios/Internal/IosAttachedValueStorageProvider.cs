using System.Collections.Generic;
using Foundation;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal.Components;

namespace MugenMvvm.Ios.Internal
{
    public class IosAttachedValueStorageProvider : AttachedValueStorageProviderBase<NSObject>, IHasPriority
    {
        public int Priority { get; set; } = InternalComponentPriority.MetadataOwnerAttachedValueProvider;

        protected override IDictionary<string, object?>? GetAttachedDictionary(NSObject item, bool optional) => IosAttachedValueHolder.Get(item, optional)?.GetValues(optional);

        protected override bool ClearInternal(NSObject item)
        {
            var dictionary = IosAttachedValueHolder.Get(item, true)?.GetValues(true);
            if (dictionary == null)
                return false;
            dictionary.Clear();
            return true;
        }
    }
}