using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IChildAttachedValueProvider : IHasPriority
    {
        bool TryGetOrAddAttachedDictionary(IAttachedValueProvider parentProvider, object item, bool required, out LightDictionaryBase<string, object?>? dictionary);

        bool TryClear(IAttachedValueProvider parentProvider, object item, out bool result);
    }
}