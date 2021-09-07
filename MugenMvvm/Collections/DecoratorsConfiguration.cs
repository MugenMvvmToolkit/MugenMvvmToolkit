using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct DecoratorsConfiguration
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

        public DecoratorsConfiguration Add(IComponent<IReadOnlyObservableCollection> decorator, int? priority = null) => Add(decorator, priority, out _);

        public DecoratorsConfiguration Add(IComponent<IReadOnlyObservableCollection> decorator, int? priority, out ActionToken removeToken)
        {
            if (Collection == null)
                ExceptionManager.ThrowObjectNotInitialized(typeof(DecoratorsConfiguration));
            removeToken = Collection.AddComponent(decorator);
            return UpdatePriority(priority);
        }

        public DecoratorsConfiguration UpdatePriority(int? priority = null) => new(Collection!, priority ?? Priority - Step, Step);
    }
}