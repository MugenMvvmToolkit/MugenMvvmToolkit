using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct DecoratorsConfiguration<T>
    {
        public readonly IReadOnlyObservableCollection Collection;
        public readonly int Priority;
        public readonly int Step;

        public DecoratorsConfiguration(IReadOnlyObservableCollection collection, int priority, int step)
        {
            Should.NotBeNull(collection, nameof(collection));
            Step = step;
            Collection = collection;
            Priority = priority;
        }

        public IReadOnlyObservableCollection<TTo> CastCollectionTo<TTo>() => (IReadOnlyObservableCollection<TTo>) Collection;

        public SynchronizedObservableCollection<TTo> CastCollectionToSynchronized<TTo>() => (SynchronizedObservableCollection<TTo>) Collection;

        public DecoratorsConfiguration<TTo> For<TTo>() => new(Collection, Priority, Step);

        public DecoratorsConfiguration<T> Add(IComponent<IReadOnlyObservableCollection> decorator, int? priority = null) => Add(decorator, priority, out _);

        public DecoratorsConfiguration<T> Add(IComponent<IReadOnlyObservableCollection> decorator, int? priority, out ActionToken removeToken)
        {
            if (Collection == null)
                ExceptionManager.ThrowObjectNotInitialized(typeof(DecoratorsConfiguration<>));
            removeToken = Collection.AddComponent(decorator);
            return UpdatePriority(priority);
        }

        public DecoratorsConfiguration<T> UpdatePriority(int? priority = null) => new(Collection!, priority ?? Priority - Step, Step);

        public static implicit operator SynchronizedObservableCollection<T>(DecoratorsConfiguration<T> configuration) =>
            (SynchronizedObservableCollection<T>) configuration.Collection;

        public static implicit operator DecoratorsConfiguration<T>(DecoratorsConfiguration<object?> configuration) => configuration.For<T>();

        public static implicit operator DecoratorsConfiguration<object?>(DecoratorsConfiguration<T> configuration) => configuration.For<object?>();
    }
}