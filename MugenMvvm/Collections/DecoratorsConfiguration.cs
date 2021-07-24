using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct DecoratorsConfiguration
    {
        public readonly IReadOnlyObservableCollection Collection;
        public readonly int Priority;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DecoratorsConfiguration(IReadOnlyObservableCollection collection, int priority)
        {
            Should.NotBeNull(collection, nameof(collection));
            Collection = collection;
            Priority = priority;
        }

        public DecoratorsConfiguration AddDecorator(IComponent<IReadOnlyObservableCollection> decorator, int? priority = null)
        {
            if (Collection == null)
                ExceptionManager.ThrowObjectNotInitialized(typeof(DecoratorsConfiguration));
            Collection.AddComponent(decorator);
            return new DecoratorsConfiguration(Collection!, priority ?? Priority - 10);
        }
    }
}