using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.IoC;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.IoC
{
    public abstract class IocContainerBase<TContainer, TInternalContainer> : IIocContainer
        where TContainer : class, IIocContainer
        where TInternalContainer : class
    {
        #region Fields

        private IComponentCollection<IIocContainerListener>? _listeners;
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

        public bool IsListenersInitialized => _listeners != null;

        public IComponentCollection<IIocContainerListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    MugenExtensions.LazyInitialize(ref _listeners, GetListenersCollection());
                return _listeners;
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
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnDisposed(this);
            this.RemoveAllListeners();
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
            return CanResolveInternal(service, TryGetName(metadata), metadata);
        }

        public object Get(Type service, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            TryGetNameAndParameters(metadata, out var name, out var parameters);
            return GetInternal(service, name, parameters, metadata);
        }

        public IEnumerable<object> GetAll(Type service, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            TryGetNameAndParameters(metadata, out var name, out var parameters);
            return GetAllInternal(service, name, parameters, metadata);
        }

        public void BindToConstant(Type service, object? instance, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            BindToConstantInternal(service, instance, TryGetName(metadata), metadata);
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBindToConstant(this, service, instance, metadata.DefaultIfNull());
        }

        public void BindToType(Type service, Type typeTo, IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(typeTo, nameof(typeTo));
            Should.NotBeNull(lifecycle, nameof(lifecycle));
            TryGetNameAndParameters(metadata, out var name, out var parameters);
            BindToTypeInternal(service, typeTo, lifecycle, name, parameters, metadata);
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBindToType(this, service, typeTo, lifecycle, metadata.DefaultIfNull());
        }

        public void BindToMethod(Type service, Func<IIocContainer, IReadOnlyCollection<IIocParameter>, IReadOnlyMetadataContext, object> methodBindingDelegate,
            IocDependencyLifecycle lifecycle, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(methodBindingDelegate, nameof(methodBindingDelegate));
            Should.NotBeNull(lifecycle, nameof(lifecycle));
            TryGetNameAndParameters(metadata, out var name, out var parameters);
            BindToMethodInternal(service, methodBindingDelegate, lifecycle, name, parameters, metadata);
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBindToMethod(this, service, methodBindingDelegate, lifecycle, metadata.DefaultIfNull());
        }

        public void Unbind(Type service, IReadOnlyMetadataContext? metadata = null)
        {
            NotBeDisposed();
            Should.NotBeNull(service, nameof(service));
            if (!UnbindInternal(service, TryGetName(metadata), metadata))
                return;

            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnUnbind(this, service, metadata.DefaultIfNull());
        }

        #endregion

        #region Methods

        public TContainer CreateChild(IReadOnlyMetadataContext? metadata = null)
        {
            var childContainer = CreateChildInternal(metadata);
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnChildContainerCreated(this, childContainer, metadata.DefaultIfNull());
            return childContainer;
        }

        protected abstract void OnDispose();

        protected abstract TContainer CreateChildInternal(IReadOnlyMetadataContext? metadata);

        protected abstract bool CanResolveInternal(Type service, string? name, IReadOnlyMetadataContext? metadata);

        protected abstract object GetInternal(Type service, string? name, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext? metadata);

        protected abstract IEnumerable<object> GetAllInternal(Type service, string? name, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext? metadata);

        protected abstract void BindToConstantInternal(Type service, object? instance, string? name, IReadOnlyMetadataContext? metadata);

        protected abstract void BindToTypeInternal(Type service, Type typeTo, IocDependencyLifecycle lifecycle, string? name, IReadOnlyCollection<IIocParameter> parameters,
            IReadOnlyMetadataContext? metadata);

        protected abstract void BindToMethodInternal(Type service, Func<IIocContainer, IReadOnlyCollection<IIocParameter>, IReadOnlyMetadataContext, object> methodBindingDelegate,
            IocDependencyLifecycle lifecycle, string? name, IReadOnlyCollection<IIocParameter> parameters, IReadOnlyMetadataContext? metadata);

        protected abstract bool UnbindInternal(Type service, string? name, IReadOnlyMetadataContext? metadata);

        protected virtual void OnActivated(Type service, object? instance, IReadOnlyMetadataContext? metadata)
        {
            var listeners = this.GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnActivated(this, service, instance, metadata.DefaultIfNull());
        }

        protected virtual IComponentCollection<IIocContainerListener> GetListenersCollection()
        {
            return new OrderedArrayComponentCollection<IIocContainerListener>(this);
        }

        private static string? TryGetName(IReadOnlyMetadataContext? context)
        {
            if (context == null || context.Count == 0)
                return null;
            return context.Get(IocMetadata.Name);
        }

        private static void TryGetNameAndParameters(IReadOnlyMetadataContext? context, out string? name, out IReadOnlyCollection<IIocParameter> parameters)
        {
            if (context == null || context.Count == 0)
            {
                name = null;
                parameters = null;
            }
            else
            {
                name = context.Get(IocMetadata.Name);
                parameters = context.Get(IocMetadata.Parameters);
            }
        }

        protected void NotBeDisposed()
        {
            if (_state != 0)
                ExceptionManager.ThrowObjectDisposed(GetType());
        }

        #endregion
    }
}