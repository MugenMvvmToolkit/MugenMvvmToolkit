using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IChildAttachedValueProvider : IHasPriority
    {
        bool TryGetOrAddAttachedDictionary<TItem>(IAttachedValueProvider parentProvider, TItem item, bool required, out IAttachedValueProviderDictionary? dictionary)
            where TItem : class;
    }
}