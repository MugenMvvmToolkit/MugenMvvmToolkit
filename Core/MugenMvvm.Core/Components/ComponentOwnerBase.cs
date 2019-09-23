﻿using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Components
{
    public abstract class ComponentOwnerBase<T> : IComponentOwner<T> where T : class
    {
        #region Fields

        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private IComponentCollection<IComponent<T>>? _components;

        #endregion

        #region Constructors

        protected ComponentOwnerBase(IComponentCollectionProvider? componentCollectionProvider)
        {
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider => _componentCollectionProvider.ServiceIfNull();

        public bool HasComponents => _components != null && _components.HasItems;

        public IComponentCollection<IComponent<T>> Components
        {
            get
            {
                if (_components == null)
                    ComponentCollectionProvider.LazyInitialize(ref _components, this);
                return _components!;
            }
        }

        #endregion

        #region Methods

        protected IComponent<T>[] GetComponents()
        {
            if (_components == null)
                return Default.EmptyArray<IComponent<T>>();
            return _components.GetItems();
        }

        #endregion
    }
}