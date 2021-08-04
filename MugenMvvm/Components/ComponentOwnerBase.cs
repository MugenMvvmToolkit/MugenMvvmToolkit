﻿using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class ComponentOwnerBase<T> : IComponentOwner<T> where T : class
    {
        private readonly IComponentCollectionManager? _componentCollectionManager;
        private IComponentCollection? _components;

        protected ComponentOwnerBase(IComponentCollectionManager? componentCollectionManager)
        {
            _componentCollectionManager = componentCollectionManager;
        }

        internal ComponentOwnerBase(IComponentCollectionManager? componentCollectionManager, IComponentOwner<T>? owner) =>
            _componentCollectionManager = componentCollectionManager ?? (owner as ComponentOwnerBase<T>)?._componentCollectionManager;

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components => _components ?? ComponentCollectionManager.EnsureInitialized(ref _components, this);

        protected IComponentCollectionManager ComponentCollectionManager => _componentCollectionManager.DefaultIfNull();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ItemOrArray<TComponent> GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class
        {
            if (_components == null)
                return default;
            return _components.Get<TComponent>(metadata);
        }
    }
}