using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Collections
{
    [Preserve(AllMembers = true)]
    internal sealed class ReadOnlyObservableCollectionDebuggerProxy<T>
    {
        private readonly IReadOnlyObservableCollection<T> _collection;

        public ReadOnlyObservableCollectionDebuggerProxy(IReadOnlyObservableCollection<T> collection)
        {
            _collection = collection;
        }

        public IEnumerable<T> Items => _collection.ToArray();

        public IEnumerable<object?> DecoratedItems => _collection.DecoratedItems().ToArray();
    }
}