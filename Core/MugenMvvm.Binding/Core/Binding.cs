using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
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
    public class Binding : IBinding, IComponentCollection<IComponent<IBinding>>, IMemberPathObserverListener, IReadOnlyMetadataContext, IComparer<IComponent<IBinding>>
    {
        #region Fields

        private object? _components;
        private byte _sourceObserverCount;
        private byte _state;
        private byte _targetObserverCount;

        private const byte TargetUpdatingFlag = 1;
        private const byte SourceUpdatingFlag = 1 << 1;

        private const byte HasTargetValueInterceptorFlag = 1 << 2;
        private const byte HasSourceValueInterceptorFlag = 1 << 3;

        private const byte HasTargetListenerFlag = 1 << 4;
        private const byte HasSourceListenerFlag = 1 << 5;

        private const byte HasTargetValueSetterFlag = 1 << 6;
        private const byte HasSourceValueSetterFlag = 1 << 7;

        #endregion

        #region Constructors

#pragma warning disable CS8618
        protected Binding(IMemberPathObserver target, object sourceRaw)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(sourceRaw, nameof(sourceRaw));
            Target = target;
            SourceRaw = sourceRaw;
        }
#pragma warning restore CS8618

        public Binding(IMemberPathObserver target, IMemberPathObserver source)
            : this(target, sourceRaw: source)
        {
        }

        #endregion

        #region Properties

        bool IComponentOwner<IComponentCollection<IComponent<IBinding>>>.HasComponents => false;

        public IComponentCollection<IComponent<IBinding>> Components => this;

        public bool HasComponents => _components != null;

        IComponentCollection<IComponent<IComponentCollection<IComponent<IBinding>>>> IComponentOwner<IComponentCollection<IComponent<IBinding>>>.Components
        {
            get
            {
                ExceptionManager.ThrowNotSupported(nameof(Components));
                return null!;
            }
        }

        object IComponentCollection<IComponent<IBinding>>.Owner => this;

        bool IComponentCollection<IComponent<IBinding>>.HasItems => HasComponents;

        bool IMetadataOwner<IReadOnlyMetadataContext>.HasMetadata => true;

        IReadOnlyMetadataContext IMetadataOwner<IReadOnlyMetadataContext>.Metadata => Metadata;

        protected virtual IReadOnlyMetadataContext Metadata => this;

        public BindingState State => _targetObserverCount == byte.MaxValue ? BindingState.Disposed : BindingState.Attached;

        public IMemberPathObserver Target { get; }

        public ItemOrList<IMemberPathObserver, IMemberPathObserver[]> Source
        {
            get
            {
                if (SourceRaw is IMemberPathObserver[] array)
                    return array;
                return new ItemOrList<IMemberPathObserver, IMemberPathObserver[]>((IMemberPathObserver)SourceRaw);
            }
        }

        protected object SourceRaw { get; }

        int IReadOnlyCollection<MetadataContextValue>.Count => 1;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            var targetObserverCount = _targetObserverCount;
            if (targetObserverCount == byte.MaxValue)
                return;
            _targetObserverCount = byte.MaxValue;
            OnDispose();
            MugenBindingService.BindingManager.OnLifecycleChanged(this, BindingLifecycleState.Disposed, Metadata);
            if (targetObserverCount != 0)
                Target.RemoveListener(this);
            Target.Dispose();
            if (SourceRaw is IMemberPathObserver source)
            {
                if (_sourceObserverCount != 0)
                    source.RemoveListener(this);
                source.Dispose();
            }
            else
            {
                var sources = (IMemberPathObserver[])SourceRaw;
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

        int IComparer<IComponent<IBinding>>.Compare(IComponent<IBinding> x, IComponent<IBinding> y)
        {
            return MugenExtensions.GetComponentPriority(y, this).CompareTo(MugenExtensions.GetComponentPriority(x, this));
        }

        bool IComponentCollection<IComponent<IBinding>>.Add(IComponent<IBinding> component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(component, nameof(component));
            if (State == BindingState.Disposed)
                return false;

            var defaultListener = CallbackInvokerComponentCollectionComponent.GetComponentCollectionListener<IComponent<IBinding>>();
            if (!defaultListener.OnAdding(this, component, metadata))
                return false;

            if (_components == null)
                _components = component;
            else if (_components is IComponent<IBinding>[] items)
            {
                MugenExtensions.AddOrdered(ref items, component, this);
                _components = items;
            }
            else
            {
                _components = MugenExtensions.GetComponentPriority(_components, this) >= MugenExtensions.GetComponentPriority(component, this)
                    ? new[] { (IComponent<IBinding>)_components, component }
                    : new[] { component, (IComponent<IBinding>)_components };
            }

            OnComponentAdded(component);
            defaultListener.OnAdded(this, component, metadata);
            return true;
        }

        bool IComponentCollection<IComponent<IBinding>>.Remove(IComponent<IBinding> component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(component, nameof(component));
            if (State == BindingState.Disposed)
                return false;

            var defaultListener = CallbackInvokerComponentCollectionComponent.GetComponentCollectionListener<IComponent<IBinding>>();
            if (!defaultListener.OnRemoving(this, component, metadata) || !RemoveComponent(component))
                return false;

            OnComponentRemoved(component);
            defaultListener.OnRemoved(this, component, metadata);
            return true;
        }

        bool IComponentCollection<IComponent<IBinding>>.Clear(IReadOnlyMetadataContext? metadata)
        {
            var defaultListener = CallbackInvokerComponentCollectionComponent.GetComponentCollectionListener<IComponent<IBinding>>();
            var components = _components;
            _components = null;
            var isValid = State != BindingState.Disposed;
            if (components is IComponent<IBinding>[] array)
            {
                for (var i = 0; i < array.Length; i++)
                {
                    var component = array[i];
                    if (isValid)
                        OnComponentRemoved(component);
                    defaultListener.OnRemoved(this, component, metadata);
                }
            }
            else
            {
                var component = (IComponent<IBinding>?)components;
                if (component != null)
                {
                    if (isValid)
                        OnComponentRemoved(component);
                    defaultListener.OnRemoved(this, component, metadata);
                }
            }
            return true;
        }

        IComponent<IBinding>[] IComponentCollection<IComponent<IBinding>>.GetItems()
        {
            if (_components == null)
                return Default.EmptyArray<IComponent<IBinding>>();
            if (_components is IComponent<IBinding>[] components)
                return components;
            return new[] { (IComponent<IBinding>)_components };
        }

        void IMemberPathObserverListener.OnPathMembersChanged(IMemberPathObserver observer)
        {
            var isTarget = ReferenceEquals(Target, observer);
            var components = _components;
            if (isTarget)
            {
                if (components is IComponent<IBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingTargetObserverListener)?.OnTargetPathMembersChanged(this, observer, Metadata);
                }
                else
                    (components as IBindingTargetObserverListener)?.OnTargetPathMembersChanged(this, observer, Metadata);
            }
            else
            {
                if (components is IComponent<IBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingSourceObserverListener)?.OnSourcePathMembersChanged(this, observer, Metadata);
                }
                else
                    (components as IBindingSourceObserverListener)?.OnSourcePathMembersChanged(this, observer, Metadata);
            }
        }

        void IMemberPathObserverListener.OnLastMemberChanged(IMemberPathObserver observer)
        {
            var isTarget = ReferenceEquals(Target, observer);
            var components = _components;
            if (isTarget)
            {
                if (components is IComponent<IBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingTargetObserverListener)?.OnTargetLastMemberChanged(this, observer, Metadata);
                }
                else
                    (components as IBindingTargetObserverListener)?.OnTargetLastMemberChanged(this, observer, Metadata);
            }
            else
            {
                if (components is IComponent<IBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingSourceObserverListener)?.OnSourceLastMemberChanged(this, observer, Metadata);
                }
                else
                    (components as IBindingSourceObserverListener)?.OnSourceLastMemberChanged(this, observer, Metadata);
            }
        }

        void IMemberPathObserverListener.OnError(IMemberPathObserver observer, Exception exception)
        {
            var isTarget = ReferenceEquals(Target, observer);
            var components = _components;
            if (isTarget)
            {
                if (components is IComponent<IBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingTargetObserverListener)?.OnTargetError(this, observer, exception, Metadata);
                }
                else
                    (components as IBindingTargetObserverListener)?.OnTargetError(this, observer, exception, Metadata);
            }
            else
            {
                if (components is IComponent<IBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingSourceObserverListener)?.OnSourceError(this, observer, exception, Metadata);
                }
                else
                    (components as IBindingSourceObserverListener)?.OnSourceError(this, observer, exception, Metadata);
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
            if (BindingMetadata.Binding.Equals(contextKey))
            {
                value = (T)(object)this;
                return true;
            }

            value = contextKey.GetDefaultValue(this, defaultValue);
            return false;
        }

        bool IReadOnlyMetadataContext.Contains(IMetadataContextKey contextKey)
        {
            return BindingMetadata.Binding.Equals(contextKey);
        }

        #endregion

        #region Methods

        public void SetComponents(ItemOrList<IComponent<IBinding>, IComponent<IBinding>[]> components, IReadOnlyMetadataContext? metadata)
        {
            var list = components.List;
            var item = components.Item;
            if (item == null && list == null)
                return;

            if (_components != null)
            {
                if (item == null)
                {
                    for (int i = 0; i < list!.Length; i++)
                        Components.Add(list[i]);
                }
                else
                    Components.Add(item);
                return;
            }

            var defaultListener = CallbackInvokerComponentCollectionComponent.GetComponentCollectionListener<IComponent<IBinding>>();
            if (list == null || list.Length == 1)
            {
                var component = item ?? list![0];
                if (defaultListener.OnAdding(this, component, metadata))
                {
                    OnComponentAdded(component);
                    defaultListener.OnAdded(this, component, metadata);
                    _components = component;
                }
            }
            else
            {
                int currentLength = 0;
                for (var i = 0; i < list.Length; i++)
                {
                    var component = list[i];
                    if (!defaultListener.OnAdding(this, component, metadata))
                        continue;

                    OnComponentAdded(component);
                    defaultListener.OnAdded(this, component, metadata);
                    list[currentLength++] = list[i];
                }

                if (currentLength == 1)
                    _components = list[0];
                else if (currentLength != 0)
                {
                    if (list.Length - currentLength != 0)
                        Array.Resize(ref list, currentLength);
                    _components = list;
                }
            }
        }

        protected virtual object? GetSourceValue(MemberPathLastMember targetMember)
        {
            return ((IMemberPathObserver)SourceRaw).GetLastMember(Metadata).GetLastMemberValue(Metadata);
        }

        protected virtual bool UpdateSourceInternal(out object? newValue)
        {
            if (SourceRaw is IMemberPathObserver observer)
                return SetSourceValue(observer, out newValue);
            newValue = null;
            return false;
        }

        protected virtual object? GetTargetValue(MemberPathLastMember sourceMember)
        {
            return Target.GetLastMember(Metadata).GetLastMemberValue(Metadata);
        }

        protected virtual bool UpdateTargetInternal(out object? newValue)
        {
            return SetTargetValue(Target, out newValue);
        }

        protected virtual void OnTargetUpdateFailed(Exception error)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdateFailed(this, error, Metadata);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdateFailed(this, error, Metadata);
        }

        protected virtual void OnTargetUpdateCanceled()
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdateCanceled(this, Metadata);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdateCanceled(this, Metadata);
        }

        protected virtual void OnTargetUpdated(object? newValue)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdated(this, newValue, Metadata);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdated(this, newValue, Metadata);
        }

        protected virtual void OnSourceUpdateFailed(Exception error)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdateFailed(this, error, Metadata);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdateFailed(this, error, Metadata);
        }

        protected virtual void OnSourceUpdateCanceled()
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdateCanceled(this, Metadata);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdateCanceled(this, Metadata);
        }

        protected virtual void OnSourceUpdated(object? newValue)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdated(this, newValue, Metadata);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdated(this, newValue, Metadata);
        }

        protected virtual object? InterceptTargetValue(IMemberPathObserver targetObserver, MemberPathLastMember targetMember, object? value)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueInterceptorBindingComponent interceptor)
                        value = interceptor.InterceptTargetValue(targetObserver, targetMember, value, Metadata);
                }
            }
            else if (components is ITargetValueInterceptorBindingComponent interceptor)
                value = interceptor.InterceptTargetValue(targetObserver, targetMember, value, Metadata);

            return value;
        }

        protected virtual object? InterceptSourceValue(IMemberPathObserver sourceObserver, MemberPathLastMember sourceMember, object? value)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueInterceptorBindingComponent interceptor)
                        value = interceptor.InterceptSourceValue(sourceObserver, sourceMember, value, Metadata);
                }
            }
            else if (components is ISourceValueInterceptorBindingComponent interceptor)
                value = interceptor.InterceptSourceValue(sourceObserver, sourceMember, value, Metadata);

            return value;
        }

        protected virtual bool TrySetTargetValue(IMemberPathObserver targetObserver, MemberPathLastMember targetMember, object? newValue)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueSetterBindingComponent setter && setter.TrySetTargetValue(targetObserver, targetMember, newValue, Metadata))
                        return true;
                }
            }
            else if (components is ITargetValueSetterBindingComponent setter && setter.TrySetTargetValue(targetObserver, targetMember, newValue, Metadata))
                return true;

            return false;
        }

        protected virtual bool TrySetSourceValue(IMemberPathObserver sourceObserver, MemberPathLastMember sourceMember, object? newValue)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueSetterBindingComponent setter && setter.TrySetSourceValue(sourceObserver, sourceMember, newValue, Metadata))
                        return true;
                }
            }
            else if (components is ISourceValueSetterBindingComponent setter && setter.TrySetSourceValue(sourceObserver, sourceMember, newValue, Metadata))
                return true;

            return false;
        }

        protected virtual void OnDispose()
        {
        }

        protected bool SetTargetValue(IMemberPathObserver targetObserver, out object? newValue)
        {
            var pathLastMember = targetObserver.GetLastMember(Metadata);
            pathLastMember.ThrowIfError();

            if (!pathLastMember.IsAvailable)
            {
                newValue = null;
                return false;
            }

            newValue = GetSourceValue(pathLastMember);
            if (newValue.IsUnsetValueOrDoNothing())
                return false;

            if (CheckFlag(HasTargetValueInterceptorFlag))
            {
                newValue = InterceptTargetValue(targetObserver, pathLastMember, newValue);
                if (newValue.IsUnsetValueOrDoNothing())
                    return false;
            }

            newValue = MugenBindingService.GlobalValueConverter.Convert(newValue, pathLastMember.LastMember.Type, pathLastMember.LastMember, Metadata);

            if (!CheckFlag(HasTargetValueSetterFlag) || !TrySetTargetValue(targetObserver, pathLastMember, newValue))
                pathLastMember.SetLastMemberValue(newValue, Metadata);
            return true;
        }

        protected bool SetSourceValue(IMemberPathObserver sourceObserver, out object? newValue)
        {
            var pathLastMember = sourceObserver.GetLastMember(Metadata);
            pathLastMember.ThrowIfError();

            if (!pathLastMember.IsAvailable)
            {
                newValue = null;
                return false;
            }

            newValue = GetTargetValue(pathLastMember);
            if (newValue.IsUnsetValueOrDoNothing())
                return false;

            if (CheckFlag(HasSourceValueInterceptorFlag))
            {
                newValue = InterceptSourceValue(sourceObserver, pathLastMember, newValue);
                if (newValue.IsUnsetValueOrDoNothing())
                    return false;
            }

            newValue = MugenBindingService.GlobalValueConverter.Convert(newValue, pathLastMember.LastMember.Type, pathLastMember.LastMember, Metadata);

            if (!CheckFlag(HasSourceValueSetterFlag) || !TrySetSourceValue(sourceObserver, pathLastMember, newValue))
                pathLastMember.SetLastMemberValue(newValue, Metadata);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckFlag(byte flag)
        {
            return (_state & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetFlag(byte flag)
        {
            _state |= flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ClearFlag(byte flag)
        {
            _state = (byte)(_state & ~flag);
        }

        private bool RemoveComponent(IComponent<IBinding> component)
        {
            if (_components == null)
                return false;

            if (ReferenceEquals(component, _components))
            {
                _components = null;
                return true;
            }

            if (!(_components is IComponent<IBinding>[] items))
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

        private void OnComponentAdded(IComponent<IBinding> component)
        {
            if (component is ISourceValueInterceptorBindingComponent)
                SetFlag(HasSourceValueInterceptorFlag);
            if (component is ITargetValueInterceptorBindingComponent)
                SetFlag(HasTargetValueInterceptorFlag);
            if (component is IBindingSourceListener)
                SetFlag(HasSourceListenerFlag);
            if (component is IBindingTargetListener)
                SetFlag(HasTargetListenerFlag);
            if (component is ITargetValueSetterBindingComponent)
                SetFlag(HasTargetValueSetterFlag);
            if (component is ISourceValueSetterBindingComponent)
                SetFlag(HasSourceValueSetterFlag);
            if (component is IBindingTargetObserverListener && ++_targetObserverCount == 1)
                Target.AddListener(this);
            if (!(component is IBindingSourceObserverListener) || ++_sourceObserverCount != 1)
                return;

            if (SourceRaw is IMemberPathObserver source)
                source.AddListener(this);
            else
            {
                var observers = (IMemberPathObserver[])SourceRaw;
                for (var i = 0; i < observers.Length; i++)
                    observers[i].AddListener(this);
            }
        }

        private void OnComponentRemoved(IComponent<IBinding> component)
        {
            if (component is IBindingTargetObserverListener && --_targetObserverCount == 0)
                Target.RemoveListener(this);
            if (component is IBindingSourceObserverListener && --_sourceObserverCount == 0)
            {
                if (SourceRaw is IMemberPathObserver source)
                    source.RemoveListener(this);
                else
                {
                    var observers = (IMemberPathObserver[])SourceRaw;
                    for (var i = 0; i < observers.Length; i++)
                        observers[i].RemoveListener(this);
                }
            }
        }

        #endregion
    }
}