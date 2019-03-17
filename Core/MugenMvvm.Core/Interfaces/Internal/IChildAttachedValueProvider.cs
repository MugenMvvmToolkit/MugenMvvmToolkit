using MugenMvvm.Collections;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IChildAttachedValueProvider
    {
        bool TryGetOrAddAttachedDictionary(IAttachedValueProvider parentProvider, object item, bool required, out LightDictionaryBase<string, object?>? dictionary);

        bool TryClear(IAttachedValueProvider parentProvider, object item, out bool result);
    }
}