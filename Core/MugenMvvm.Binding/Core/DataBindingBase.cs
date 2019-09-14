using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Core
{
    public abstract class DataBindingBase : IDataBinding, IComponentCollection<IComponent<IDataBinding>>, IBindingPathObserverListener, IReadOnlyMetadataContext
    {
        #region Fields

        private object? _components;
        private byte _sourceObserverCount;
        private short _state;
        private byte _targetObserverCount;

        private const short TargetUpdatingFlag = 1;
        private const short SourceUpdatingFlag = 1 << 1;

        private const short DisableEqualityCheckingTargetFlag = 1 << 2;
        private const short DisableEqualityCheckingSourceFlag = 1 << 3;

        private const short HasTargetValueInterceptorFlag = 1 << 4;
        private const short HasSourceValueInterceptorFlag = 1 << 5;

        private const short HasTargetListenerFlag = 1 << 6;
        private const short HasSourceListenerFlag = 1 << 7;

        private const short HasTargetValueSetterFlag = 1 << 8;
        private const short HasSourceValueSetterFlag = 1 << 9;

        private const short DisposedFlag = 1 << 10;

        #endregion

        #region Constructors

#pragma warning disable CS8618
        private DataBindingBase(IBindingPathObserver target)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
        }
#pragma warning restore CS8618

        protected DataBindingBase(IBindingPathObserver target, IBindingPathObserver source) : this(target)
        {
            Should.NotBeNull(source, nameof(source));
            SourceRaw = source;
        }

        protected DataBindingBase(IBindingPathObserver target, IBindingPathObserver[] sources) : this(target)
        {
            Should.NotBeNull(sources, nameof(sources));
            SourceRaw = sources;
        }

        #endregion

        #region Properties

        bool IComponentOwner<IComponentCollection<IComponent<IDataBinding>>>.HasComponents => false;

        public IComponentCollection<IComponent<IDataBinding>> Components => this;

        public bool HasComponents => _components != null;

        IComponentCollection<IComponent<IComponentCollection<IComponent<IDataBinding>>>> IComponentOwner<IComponentCollection<IComponent<IDataBinding>>>.Components
        {
            get
            {
                ExceptionManager.ThrowNotSupported(nameof(Components));
                return null!;
            }
        }

        object IComponentCollection<IComponent<IDataBinding>>.Owner => this;

        bool IComponentCollection<IComponent<IDataBinding>>.HasItems => HasComponents;

        protected virtual IReadOnlyMetadataContext Metadata => this;

        public DataBindingState State => CheckFlag(DisposedFlag) ? DataBindingState.Disposed : DataBindingState.Attached;

        public IBindingPathObserver Target { get; }

        public ItemOrList<IBindingPathObserver, IBindingPathObserver[]> Source
        {
            get
            {
                if (SourceRaw is IBindingPathObserver[] array)
                    return array;
                return new ItemOrList<IBindingPathObserver, IBindingPathObserver[]>((IBindingPathObserver)SourceRaw);
            }
        }

        protected object SourceRaw { get; }

        int IReadOnlyCollection<MetadataContextValue>.Count => 1;

        #endregion

        #region Implementation of interfaces

        void IBindingPathObserverListener.OnPathMembersChanged(IBindingPathObserver observer)
        {
            var isTarget = ReferenceEquals(Target, observer);
            var components = _components;
            if (isTarget)
            {
                if (components is IComponent<IDataBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IDataBindingTargetObserverListener)?.OnTargetPathMembersChanged(this, observer, Metadata);
                }
                else
                    (components as IDataBindingTargetObserverListener)?.OnTargetPathMembersChanged(this, observer, Metadata);
            }
            else
            {
                if (components is IComponent<IDataBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IDataBindingSourceObserverListener)?.OnSourcePathMembersChanged(this, observer, Metadata);
                }
                else
                    (components as IDataBindingSourceObserverListener)?.OnSourcePathMembersChanged(this, observer, Metadata);
            }
        }

        void IBindingPathObserverListener.OnLastMemberChanged(IBindingPathObserver observer)
        {
            var isTarget = ReferenceEquals(Target, observer);
            var components = _components;
            if (isTarget)
            {
                if (components is IComponent<IDataBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IDataBindingTargetObserverListener)?.OnTargetLastMemberChanged(this, observer, Metadata);
                }
                else
                    (components as IDataBindingTargetObserverListener)?.OnTargetLastMemberChanged(this, observer, Metadata);
            }
            else
            {
                if (components is IComponent<IDataBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IDataBindingSourceObserverListener)?.OnSourceLastMemberChanged(this, observer, Metadata);
                }
                else
                    (components as IDataBindingSourceObserverListener)?.OnSourceLastMemberChanged(this, observer, Metadata);
            }
        }

        void IBindingPathObserverListener.OnError(IBindingPathObserver observer, Exception exception)
        {
            var isTarget = ReferenceEquals(Target, observer);
            var components = _components;
            if (isTarget)
            {
                if (components is IComponent<IDataBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IDataBindingTargetObserverListener)?.OnTargetError(this, observer, exception, Metadata);
                }
                else
                    (components as IDataBindingTargetObserverListener)?.OnTargetError(this, observer, exception, Metadata);
            }
            else
            {
                if (components is IComponent<IDataBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IDataBindingSourceObserverListener)?.OnSourceError(this, observer, exception, Metadata);
                }
                else
                    (components as IDataBindingSourceObserverListener)?.OnSourceError(this, observer, exception, Metadata);
            }
        }

        bool IComponentCollection<IComponent<IDataBinding>>.Add(IComponent<IDataBinding> component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(component, nameof(component));

            var defaultListener = CallbackInvokerComponentCollectionComponent.GetComponentCollectionListener<IComponent<IDataBinding>>();
            if (!defaultListener.OnAdding(this, component, metadata))
                return false;

            if (_components == null)
            {
                _components = component;
                return true;
            }

            if (_components is IComponent<IDataBinding>[] items)
            {
                MugenExtensions.AddOrdered(ref items, component, this);
                _components = items;
            }
            else
            {
                _components = MugenExtensions.GetComponentPriority(_components, this) >= MugenExtensions.GetComponentPriority(component, this)
                    ? new[] { (IComponent<IDataBinding>)_components, component }
                    : new[] { component, (IComponent<IDataBinding>)_components };
            }

            OnComponentAdded(component);
            defaultListener.OnAdded(this, component, metadata);
            return true;
        }

        bool IComponentCollection<IComponent<IDataBinding>>.Remove(IComponent<IDataBinding> component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(component, nameof(component));

            var defaultListener = CallbackInvokerComponentCollectionComponent.GetComponentCollectionListener<IComponent<IDataBinding>>();
            if (!defaultListener.OnRemoving(this, component, metadata) || !RemoveComponent(component))
                return false;

            OnComponentRemoved(component);
            defaultListener.OnRemoved(this, component, metadata);
            return true;
        }

        bool IComponentCollection<IComponent<IDataBinding>>.Clear(IReadOnlyMetadataContext? metadata)
        {
            var defaultListener = CallbackInvokerComponentCollectionComponent.GetComponentCollectionListener<IComponent<IDataBinding>>();
            if (!defaultListener.OnClearing(this, metadata))
                return false;

            var components = _components;
            _components = null;
            ItemOrList<IComponent<IDataBinding>?, IComponent<IDataBinding>[]> oldItems;
            if (components is IComponent<IDataBinding>[] array)
            {
                oldItems = array;
                for (var i = 0; i < array.Length; i++)
                    OnComponentRemoved(array[i]);
            }
            else
            {
                var component = (IComponent<IDataBinding>?)components;
                oldItems = new ItemOrList<IComponent<IDataBinding>?, IComponent<IDataBinding>[]>(component);
                if (component != null)
                    OnComponentRemoved(component);
            }

            defaultListener.OnCleared(this, oldItems, metadata);
            return true;
        }

        IComponent<IDataBinding>[] IComponentCollection<IComponent<IDataBinding>>.GetItems()
        {
            if (_components == null)
                return Default.EmptyArray<IComponent<IDataBinding>>();
            if (_components is IComponent<IDataBinding>[] components)
                return components;
            return new[] { (IComponent<IDataBinding>)_components };
        }

        public void Dispose()
        {
            if (CheckFlag(DisposedFlag))
                return;
            SetFlag(DisposedFlag);
            OnDispose();
            MugenBindingService.BindingManager.OnLifecycleChanged(this, DataBindingLifecycleState.Disposed, Metadata);
            if (_targetObserverCount != 0)
                Target.RemoveListener(this);
            Target.Dispose();
            if (SourceRaw is IBindingPathObserver source)
            {
                if (_sourceObserverCount != 0)
                    source.RemoveListener(this);
                source.Dispose();
            }
            else
            {
                var sources = (IBindingPathObserver[])SourceRaw;
                for (var i = 0; i < sources.Length; i++)
                {
                    var observer = sources[i];
                    if (_sourceObserverCount != 0)
                        observer.RemoveListener(this);
                    observer.Dispose();
                }
            }

            Components.Clear();
        }

        public bool TryGet<T>(IMetadataContextKey<T> key, out T value)
        {
            Should.NotBeNull(key, nameof(key));
            if (BindingMetadata.DisableEqualityCheckingSource.Equals(key))
            {
                value = MugenExtensions.FromBoolNoBox<T>(CheckFlag(DisableEqualityCheckingSourceFlag));
                return true;
            }

            if (BindingMetadata.DisableEqualityCheckingTarget.Equals(key))
            {
                value = MugenExtensions.FromBoolNoBox<T>(CheckFlag(DisableEqualityCheckingTargetFlag));
                return true;
            }

            if (BindingMetadata.Binding.Equals(key))
            {
                value = (T)(object)this;
                return true;
            }

            return TryGetInternal(key, out value);
        }

        public bool Set<T>(IMetadataContextKey<T> key, T value)
        {
            Should.NotBeNull(key, nameof(key));
            if (BindingMetadata.DisableEqualityCheckingSource.Equals(key))
            {
                if (MugenExtensions.ToBoolNoBox(value))
                    SetFlag(DisableEqualityCheckingSourceFlag);
                else
                    ClearFlag(DisableEqualityCheckingSourceFlag);
                return true;
            }

            if (BindingMetadata.DisableEqualityCheckingTarget.Equals(key))
            {
                if (MugenExtensions.ToBoolNoBox(value))
                    SetFlag(DisableEqualityCheckingTargetFlag);
                else
                    ClearFlag(DisableEqualityCheckingTargetFlag);
                return true;
            }

            return SetInternal(key, value);
        }

        public void UpdateTarget()
        {
            try
            {
                if (CheckFlag(TargetUpdatingFlag))
                    return;

                SetFlag(TargetUpdatingFlag);
                var success = UpdateTargetInternal(out var newValue);
                if (CheckFlag(HasTargetListenerFlag))
                {
                    if (success)
                        OnTargetUpdated(newValue);
                    else
                        OnTargetUpdateCanceled();
                }
            }
            catch (Exception e)
            {
                if (CheckFlag(HasTargetListenerFlag))
                    OnTargetUpdateFailed(e);
            }
            finally
            {
                ClearFlag(TargetUpdatingFlag);
            }
        }

        public void UpdateSource()
        {
            try
            {
                if (CheckFlag(SourceUpdatingFlag))
                    return;

                SetFlag(SourceUpdatingFlag);
                var success = UpdateSourceInternal(out var newValue);
                if (CheckFlag(HasSourceListenerFlag))
                {
                    if (success)
                        OnSourceUpdated(newValue);
                    else
                        OnSourceUpdateCanceled();
                }
            }
            catch (Exception e)
            {
                if (CheckFlag(HasSourceListenerFlag))
                    OnSourceUpdateFailed(e);
            }
            finally
            {
                ClearFlag(SourceUpdatingFlag);
            }
        }

        IEnumerator<MetadataContextValue> IEnumerable<MetadataContextValue>.GetEnumerator()
        {
            yield return MetadataContextValue.Create(BindingMetadata.Binding, this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IReadOnlyMetadataContext)this).GetEnumerator();
        }

        bool IReadOnlyMetadataContext.TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue)
        {
            if (TryGet(contextKey, out value))
                return true;

            value = contextKey.GetDefaultValue(this, defaultValue);
            return false;
        }

        bool IReadOnlyMetadataContext.Contains(IMetadataContextKey contextKey)
        {
            return BindingMetadata.Binding.Equals(contextKey);
        }

        #endregion

        #region Methods

        protected abstract object? GetSourceValue(IBindingMemberInfo lastMember);

        protected virtual bool UpdateSourceInternal(out object? newValue)
        {
            if (SourceRaw is IBindingPathObserver observer)
                return SetSourceValue(observer, out newValue);
            newValue = null;
            return false;
        }

        protected virtual object? GetTargetValue(IBindingMemberInfo lastMember)
        {
            return Target.GetLastMember(Metadata).GetLastMemberValue(metadata: Metadata);
        }

        protected virtual bool UpdateTargetInternal(out object? newValue)
        {
            return SetTargetValue(Target, out newValue);
        }

        protected virtual void OnTargetUpdateFailed(Exception error)
        {
            var components = _components;
            if (components is IComponent<IDataBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IDataBindingTargetListener)?.OnTargetUpdateFailed(this, error, Metadata);
            }
            else
                (components as IDataBindingTargetListener)?.OnTargetUpdateFailed(this, error, Metadata);
        }

        protected virtual void OnTargetUpdateCanceled()
        {
            var components = _components;
            if (components is IComponent<IDataBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IDataBindingTargetListener)?.OnTargetUpdateCanceled(this, Metadata);
            }
            else
                (components as IDataBindingTargetListener)?.OnTargetUpdateCanceled(this, Metadata);
        }

        protected virtual void OnTargetUpdated(object? newValue)
        {
            var components = _components;
            if (components is IComponent<IDataBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IDataBindingTargetListener)?.OnTargetUpdated(this, newValue, Metadata);
            }
            else
                (components as IDataBindingTargetListener)?.OnTargetUpdated(this, newValue, Metadata);
        }

        protected virtual void OnSourceUpdateFailed(Exception error)
        {
            var components = _components;
            if (components is IComponent<IDataBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IDataBindingSourceListener)?.OnSourceUpdateFailed(this, error, Metadata);
            }
            else
                (components as IDataBindingSourceListener)?.OnSourceUpdateFailed(this, error, Metadata);
        }

        protected virtual void OnSourceUpdateCanceled()
        {
            var components = _components;
            if (components is IComponent<IDataBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IDataBindingSourceListener)?.OnSourceUpdateCanceled(this, Metadata);
            }
            else
                (components as IDataBindingSourceListener)?.OnSourceUpdateCanceled(this, Metadata);
        }

        protected virtual void OnSourceUpdated(object? newValue)
        {
            var components = _components;
            if (components is IComponent<IDataBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IDataBindingSourceListener)?.OnSourceUpdated(this, newValue, Metadata);
            }
            else
                (components as IDataBindingSourceListener)?.OnSourceUpdated(this, newValue, Metadata);
        }

        protected virtual object? InterceptTargetValue(in BindingPathLastMember targetMembers, object? value)
        {
            var components = _components;
            if (components is IComponent<IDataBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueInterceptorDataBindingComponent interceptor)
                        value = interceptor.InterceptTargetValue(targetMembers, value, Metadata);
                }
            }
            else if (components is ITargetValueInterceptorDataBindingComponent interceptor)
                value = interceptor.InterceptTargetValue(targetMembers, value, Metadata);

            return value;
        }

        protected virtual object? InterceptSourceValue(in BindingPathLastMember sourceMembers, object? value)
        {
            var components = _components;
            if (components is IComponent<IDataBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueInterceptorDataBindingComponent interceptor)
                        value = interceptor.InterceptSourceValue(sourceMembers, value, Metadata);
                }
            }
            else if (components is ISourceValueInterceptorDataBindingComponent interceptor)
                value = interceptor.InterceptSourceValue(sourceMembers, value, Metadata);

            return value;
        }

        protected virtual bool TrySetTargetValue(in BindingPathLastMember targetMembers, object? newValue)
        {
            var components = _components;
            if (components is IComponent<IDataBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueSetterDataBindingComponent setter && setter.TrySetTargetValue(targetMembers, newValue, Metadata))
                        return true;
                }
            }
            else if (components is ITargetValueSetterDataBindingComponent setter && setter.TrySetTargetValue(targetMembers, newValue, Metadata))
                return true;

            return false;
        }

        protected virtual bool TrySetSourceValue(in BindingPathLastMember sourceMembers, object? newValue)
        {
            var components = _components;
            if (components is IComponent<IDataBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueSetterDataBindingComponent setter && setter.TrySetSourceValue(sourceMembers, newValue, Metadata))
                        return true;
                }
            }
            else if (components is ISourceValueSetterDataBindingComponent setter && setter.TrySetSourceValue(sourceMembers, newValue, Metadata))
                return true;

            return false;
        }

        protected virtual void OnDispose()
        {
        }

        protected virtual bool TryGetInternal<T>(IMetadataContextKey<T> key, out T value)
        {
            value = default!;
            return false;
        }

        protected virtual bool SetInternal<T>(IMetadataContextKey<T> key, T value)
        {
            return false;
        }

        protected bool SetTargetValue(IBindingPathObserver target, out object? newValue)
        {
            var pathLastMember = target.GetLastMember(Metadata);
            pathLastMember.ThrowIfError();

            if (!pathLastMember.IsAvailable)
            {
                newValue = null;
                return false;
            }

            newValue = GetSourceValue(pathLastMember.LastMember);
            if (newValue.IsUnsetValueOrDoNothing())
                return false;

            if (CheckFlag(HasTargetValueInterceptorFlag))
            {
                newValue = InterceptTargetValue(pathLastMember, newValue);
                if (newValue.IsUnsetValueOrDoNothing())
                    return false;
            }

            newValue = MugenBindingService.GlobalBindingValueConverter.Convert(newValue, pathLastMember.LastMember.Type, pathLastMember.LastMember, Metadata);
            if (!CheckFlag(DisableEqualityCheckingTargetFlag) && pathLastMember.LastMember.CanRead)
            {
                var oldValue = pathLastMember.LastMember.GetValue(pathLastMember.PenultimateValue, null, Metadata);
                if (Equals(oldValue, newValue))
                    return false;
            }

            if (!CheckFlag(HasTargetValueSetterFlag) || !TrySetTargetValue(pathLastMember, newValue))
                pathLastMember.LastMember.SetValue(pathLastMember.PenultimateValue, newValue, Metadata);
            return true;
        }

        protected bool SetSourceValue(IBindingPathObserver sourceObserver, out object? newValue)
        {
            var pathLastMember = sourceObserver.GetLastMember(Metadata);
            pathLastMember.ThrowIfError();

            if (!pathLastMember.IsAvailable)
            {
                newValue = null;
                return false;
            }

            newValue = GetTargetValue(pathLastMember.LastMember);
            if (newValue.IsUnsetValueOrDoNothing())
                return false;

            if (CheckFlag(HasSourceValueInterceptorFlag))
            {
                newValue = InterceptSourceValue(pathLastMember, newValue);
                if (newValue.IsUnsetValueOrDoNothing())
                    return false;
            }

            newValue = MugenBindingService.GlobalBindingValueConverter.Convert(newValue, pathLastMember.LastMember.Type, pathLastMember.LastMember, Metadata);
            if (!CheckFlag(DisableEqualityCheckingSourceFlag) && pathLastMember.LastMember.CanRead)
            {
                var oldValue = pathLastMember.LastMember.GetValue(pathLastMember.PenultimateValue, null, Metadata);
                if (Equals(oldValue, newValue))
                    return false;
            }

            if (!CheckFlag(HasSourceValueSetterFlag) || !TrySetSourceValue(pathLastMember, newValue))
                pathLastMember.LastMember.SetValue(pathLastMember.PenultimateValue, newValue, Metadata);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckFlag(short flag)
        {
            return (_state & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetFlag(short flag)
        {
            _state |= flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ClearFlag(short flag)
        {
            _state = (byte)(_state & ~flag);
        }

        private bool RemoveComponent(IComponent<IDataBinding> component)
        {
            if (_components == null)
                return false;

            if (ReferenceEquals(component, _components))
            {
                _components = null;
                return true;
            }

            if (!(_components is IComponent<IDataBinding>[] items))
                return false;

            if (items.Length == 2)
            {
                if (ReferenceEquals(items[0], component))
                {
                    _components = items[1];
                    return true;
                }

                if (ReferenceEquals(items[1], component))
                {
                    _components = items[0];
                    return true;
                }
            }
            else if (MugenExtensions.Remove(ref items, component))
            {
                _components = items;
                return true;
            }

            return false;
        }

        private void OnComponentAdded(IComponent<IDataBinding> component)
        {
            if (component is ISourceValueInterceptorDataBindingComponent)
                SetFlag(HasSourceValueInterceptorFlag);
            if (component is ITargetValueInterceptorDataBindingComponent)
                SetFlag(HasTargetValueInterceptorFlag);
            if (component is IDataBindingSourceListener)
                SetFlag(HasSourceListenerFlag);
            if (component is IDataBindingTargetListener)
                SetFlag(HasTargetListenerFlag);
            if (component is ITargetValueSetterDataBindingComponent)
                SetFlag(HasTargetValueSetterFlag);
            if (component is ISourceValueSetterDataBindingComponent)
                SetFlag(HasSourceValueSetterFlag);
            if (component is IDataBindingTargetObserverListener && ++_targetObserverCount == 1)
                Target.AddListener(this);
            if (!(component is IDataBindingSourceObserverListener) || ++_sourceObserverCount != 1)
                return;

            if (SourceRaw is IBindingPathObserver source)
                source.AddListener(this);
            else
            {
                var observers = (IBindingPathObserver[])SourceRaw;
                for (var i = 0; i < observers.Length; i++)
                    observers[i].AddListener(this);
            }
        }

        private void OnComponentRemoved(IComponent<IDataBinding> component)
        {
            if (CheckFlag(DisposedFlag))
                return;
            if (component is IDataBindingTargetObserverListener && --_targetObserverCount == 0)
                Target.RemoveListener(this);
            if (component is IDataBindingSourceObserverListener && --_sourceObserverCount == 0)
            {
                if (SourceRaw is IBindingPathObserver source)
                    source.RemoveListener(this);
                else
                {
                    var observers = (IBindingPathObserver[])SourceRaw;
                    for (var i = 0; i < observers.Length; i++)
                        observers[i].RemoveListener(this);
                }
            }
        }

        #endregion
    }
}