using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueProviderDictionary : IReadOnlyCollection<KeyValuePair<string, object>>
    {
        object? this[string key] { get; set; }

        bool ContainsKey(string key);

        void Add(string key, object? value);

        bool Remove(string key);

        bool TryGetValue(string key, out object? value);
    }
}