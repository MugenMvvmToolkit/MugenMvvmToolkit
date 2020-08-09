using System;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public readonly struct TestComponentSubscriber : IDisposable
    {
        #region Fields

        private readonly IComponentOwner _componentOwner;
        private readonly IComponent[] _components;

        #endregion

        #region Constructors

        private TestComponentSubscriber(IComponentOwner componentOwner, IComponent[] components)
        {
            _componentOwner = componentOwner;
            _components = components;
            foreach (var component in components)
                componentOwner.Components.Add(component);
        }

        #endregion

        #region Methods

        public static TestComponentSubscriber Subscribe<T>(params IComponent<T>[] components) where T : class, IComponentOwner => Subscribe(MugenService.Instance<T>(), components);

        public static TestComponentSubscriber Subscribe<T>(T componentOwner, params IComponent<T>[] components) where T : class, IComponentOwner => new TestComponentSubscriber(componentOwner, components);

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