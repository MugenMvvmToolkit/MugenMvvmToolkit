using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct DecoratorsConfiguration<T>
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DecoratorsConfiguration<TNewType> Cast<TNewType>() => new(Collection, Priority);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DecoratorsConfiguration<T> AddDecorator(IComponent<IReadOnlyObservableCollection> decorator, int? priority = null) => AddDecorator<T>(decorator, priority);

        public DecoratorsConfiguration<TNewType> AddDecorator<TNewType>(IComponent<IReadOnlyObservableCollection> decorator, int? priority = null)
        {
            if (Collection == null)
                ExceptionManager.ThrowObjectNotInitialized(typeof(DecoratorsConfiguration<T>));
            Collection.AddComponent(decorator);
            return new DecoratorsConfiguration<TNewType>(Collection!, priority ?? Priority - 1);
        }
    }
}