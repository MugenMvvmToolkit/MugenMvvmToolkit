using MugenMvvm.Attributes;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public sealed class ComponentCollectionManager : IComponentCollectionManager
    {
        private IComponentCollection? _components;

        [Preserve(Conditional = true)]
        public ComponentCollectionManager()
        {
        }

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components => _components ?? this.EnsureInitialized(ref _components, this);

        public IComponentCollection TryGetComponentCollection(object owner, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(owner, nameof(owner));
            var result = GetComponentCollectionInternal(owner, metadata);
            _components.GetOrDefault<IComponentCollectionManagerListener>(metadata).OnComponentCollectionCreated(this, result, metadata);
            return result;
        }

        private IComponentCollection GetComponentCollectionInternal(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner != this)
            {
                var collection = _components.GetOrDefault<IComponentCollectionProviderComponent>(metadata).TryGetComponentCollection(this, owner, metadata);
                if (collection != null)
                    return collection;
            }

            return new ComponentCollection(owner);
        }
    }
}