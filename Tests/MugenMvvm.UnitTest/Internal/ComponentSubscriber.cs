using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.UnitTest.Internal
{
    public ref struct ComponentSubscriber
    {
        #region Fields

        private readonly IComponentOwner _componentOwner;
        private readonly IComponent[] _components;

        #endregion

        #region Constructors

        private ComponentSubscriber(IComponentOwner componentOwner, IComponent[] components)
        {
            _componentOwner = componentOwner;
            _components = components;
            foreach (var component in components)
                componentOwner.Components.Add(component);
        }

        #endregion

        #region Methods

        public static ComponentSubscriber Subscribe<T>(params IComponent<T>[] components) where T : class, IComponentOwner
        {
            return Subscribe(MugenService.Instance<T>(), components);
        }

        public static ComponentSubscriber Subscribe<T>(T componentOwner, params IComponent<T>[] components) where T : class, IComponentOwner
        {
            return new ComponentSubscriber(componentOwner, components);
        }

        public void Dispose()
        {
            if (_componentOwner == null)
                return;
            foreach (var component in _components)
                _componentOwner.Components.Remove(component);
        }

        #endregion
    }
}