using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.IoC.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.IoC
{
    public abstract class IocContainerBase<TContainer, TInternalContainer> : IIocContainer
        where TContainer : class, IIocContainer
        where TInternalContainer : class
    {
        #region Fields

        private IComponentCollection<IComponent<IIocContainer>>? _components;
        private int _state;

        // ReSharper disable once StaticMemberInGenericType
        private static int _idCounter;

        #endregion

        #region Constructors

        protected IocContainerBase(TContainer? parent)
        {
            Id = Interlocked.Increment(ref _idCounter);
            Parent = parent;
        }

        #endregion

        #region Properties

        public bool HasComponents => _components != null && _components.HasItems;

        public IComponentCollection<IComponent<IIocContainer>> Components
        {
            get
            {
                if (_components == null)
                    MugenExtensions.LazyInitialize(ref _components, GetListenersCollection());
                return _components;
            }
        }

        public int Id { get; protected set; }

        public TContainer? Parent { get; }

        public abstract TInternalContainer Container { get; }

        IIocContainer? IIocContainer.Parent => Parent;

        object IIocContainer.Container => Container;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, int.MaxValue) == int.MaxValue)
                return;
            OnDispose();
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IIocContainerListener)?.OnDisposed(this);
            this.ClearComponents();
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return Get(serviceType);
        }

        IIocContainer IIocContainer.CreateChild(IReadOnlyMetadataContext? metadata)
        {
            return CreateChild(metadata);
        }

        public bool CanResolve(Type service, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            return CanResolveInternal(service, metadata);
        }

        public object Get(Type service, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            return GetInternal(service, metadata);
        }

        public IEnumerable<object> GetAll(Type service, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            return GetAllInternal(service, metadata);
        }

        public void BindToConstant(Type service, object? instance, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            BindToConstantInternal(service, instance, metadata);
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IIocContainerListener)?.OnBindToConstant(this, service, instance, metadata);
        }

        public void BindToType(Type service, Type typeTo, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(typeTo, nameof(typeTo));
            Should.NotBeNull(lifecycle, nameof(lifecycle));
            BindToTypeInternal(service, typeTo, lifecycle, metadata);
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IIocContainerListener)?.OnBindToType(this, service, typeTo, lifecycle, metadata);
        }

        public void BindToMethod(Type service, IocBindingDelegate bindingDelegate, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(bindingDelegate, nameof(bindingDelegate));
            Should.NotBeNull(lifecycle, nameof(lifecycle));
            BindToMethodInternal(service, bindingDelegate, lifecycle, metadata);
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IIocContainerListener)?.OnBindToMethod(this, service, bindingDelegate, lifecycle, metadata);
        }

        public void Unbind(Type service, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            if (!UnbindInternal(service, metadata))
                return;

            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IIocContainerListener)?.OnUnbind(this, service, metadata);
        }

        #endregion

        #region Methods

        public TContainer CreateChild(IReadOnlyMetadataContext? metadata = null)
        {
            var childContainer = CreateChildInternal(metadata);
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IIocContainerListener)?.OnChildContainerCreated(this, childContainer, metadata);
            return childContainer;
        }

        protected abstract void OnDispose();

        protected abstract TContainer CreateChildInternal(IReadOnlyMetadataContext? metadata);

        protected abstract bool CanResolveInternal(Type service, IReadOnlyMetadataContext? metadata);

        protected abstract object GetInternal(Type service, IReadOnlyMetadataContext? metadata);

        protected abstract IEnumerable<object> GetAllInternal(Type service, IReadOnlyMetadataContext? metadata);

        protected abstract void BindToConstantInternal(Type service, object? instance, IReadOnlyMetadataContext? metadata);

        protected abstract void BindToTypeInternal(Type service, Type typeTo, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata);

        protected abstract void BindToMethodInternal(Type service, IocBindingDelegate bindingDelegate, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata);

        protected abstract bool UnbindInternal(Type service, IReadOnlyMetadataContext? metadata);

        protected virtual void OnActivated(Type service, object? member, object? instance, IReadOnlyMetadataContext? bindingMetadata, IReadOnlyMetadataContext? metadata)
        {
            var components = this.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IIocContainerListener)?.OnActivated(this, service, member, instance, bindingMetadata, metadata);
        }

        protected virtual IComponentCollection<IComponent<IIocContainer>> GetListenersCollection()
        {
            return new OrderedArrayComponentCollection<IComponent<IIocContainer>>(this);
        }

        protected void NotBeDisposed()
        {
            if (_state != 0)
                ExceptionManager.ThrowObjectDisposed(GetType());
        }

        #endregion
    }
}