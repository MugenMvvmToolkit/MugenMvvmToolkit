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
        protected Binding(IMemberPathObserver target, object? sourceRaw)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            SourceRaw = sourceRaw;
        }
#pragma warning restore CS8618

        public Binding(IMemberPathObserver target, IMemberPathObserver source)
            : this(target, sourceRaw: source)
        {
            Should.NotBeNull(source, nameof(source));
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

        IReadOnlyMetadataContext IMetadataOwner<IReadOnlyMetadataContext>.Metadata => this;

        public BindingState State => _targetObserverCount == byte.MaxValue ? BindingState.Disposed : BindingState.Attached;

        public IMemberPathObserver Target { get; }

        public ItemOrList<IMemberPathObserver?, IMemberPathObserver[]> Source => ItemOrList<IMemberPathObserver?, IMemberPathObserver[]>.FromRawValue(SourceRaw);

        protected object? SourceRaw { get; }

        int IReadOnlyCollection<MetadataContextValue>.Count => GetMetadataCount();

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            var targetObserverCount = _targetObserverCount;
            if (targetObserverCount == byte.MaxValue)
                return;
            _targetObserverCount = byte.MaxValue;
            OnDispose();
            MugenBindingService.BindingManager.OnLifecycleChanged(this, BindingLifecycleState.Disposed, this);
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
                var sources = (IMemberPathObserver[]?)SourceRaw;
                if (sources != null)
                {
                    for (var i = 0; i < sources.Length; i++)
                    {
                        var observer = sources[i];
                        if (_sourceObserverCount != 0)
                            observer.RemoveListener(this);
                        observer.Dispose();
                    }
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

            if (!MugenExtensions.OnAddingDefaultHandler(this, component, metadata))
                return false;

            if (_components == null)
                _components = component;
            else if (_components is IComponent<IBinding>[] items)
            {
                MugenExtensions.AddOrdered(ref items, component, this);
                _components = items;
            }
            else
                _components = MergeComponents((IComponent<IBinding>)_components, component);

            OnComponentAdded(component);
            MugenExtensions.OnAddedDefaultHandler(this, component, metadata);
            return true;
        }

        bool IComponentCollection<IComponent<IBinding>>.Remove(IComponent<IBinding> component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(component, nameof(component));
            if (State == BindingState.Disposed)
                return false;

            if (!MugenExtensions.OnRemovingDefaultHandler(this, component, metadata) || !RemoveComponent(component))
                return false;

            OnComponentRemoved(component);
            MugenExtensions.OnRemovedDefaultHandler(this, component, metadata);
            return true;
        }

        bool IComponentCollection<IComponent<IBinding>>.Clear(IReadOnlyMetadataContext? metadata)
        {
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
                    MugenExtensions.OnRemovedDefaultHandler(this, component, metadata);
                }
            }
            else
            {
                var component = (IComponent<IBinding>?)components;
                if (component != null)
                {
                    if (isValid)
                        OnComponentRemoved(component);
                    MugenExtensions.OnRemovedDefaultHandler(this, component, metadata);
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
                        (c[i] as IBindingTargetObserverListener)?.OnTargetPathMembersChanged(this, observer, this);
                }
                else
                    (components as IBindingTargetObserverListener)?.OnTargetPathMembersChanged(this, observer, this);
            }
            else
            {
                if (components is IComponent<IBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingSourceObserverListener)?.OnSourcePathMembersChanged(this, observer, this);
                }
                else
                    (components as IBindingSourceObserverListener)?.OnSourcePathMembersChanged(this, observer, this);
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
                        (c[i] as IBindingTargetObserverListener)?.OnTargetLastMemberChanged(this, observer, this);
                }
                else
                    (components as IBindingTargetObserverListener)?.OnTargetLastMemberChanged(this, observer, this);
            }
            else
            {
                if (components is IComponent<IBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingSourceObserverListener)?.OnSourceLastMemberChanged(this, observer, this);
                }
                else
                    (components as IBindingSourceObserverListener)?.OnSourceLastMemberChanged(this, observer, this);
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
                        (c[i] as IBindingTargetObserverListener)?.OnTargetError(this, observer, exception, this);
                }
                else
                    (components as IBindingTargetObserverListener)?.OnTargetError(this, observer, exception, this);
            }
            else
            {
                if (components is IComponent<IBinding>[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingSourceObserverListener)?.OnSourceError(this, observer, exception, this);
                }
                else
                    (components as IBindingSourceObserverListener)?.OnSourceError(this, observer, exception, this);
            }
        }

        IEnumerator<MetadataContextValue> IEnumerable<MetadataContextValue>.GetEnumerator()
        {
            return GetMetadataEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetMetadataEnumerator();
        }

        bool IReadOnlyMetadataContext.TryGet<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue)
        {
            return TryGetMetadata(contextKey, out value, defaultValue);
        }

        bool IReadOnlyMetadataContext.Contains(IMetadataContextKey contextKey)
        {
            return ContainsMetadata(contextKey);
        }

        #endregion

        #region Methods

        public void AddOrderedComponents(ItemOrList<IComponent<IBinding>?, IComponent<IBinding>[]> components, IReadOnlyMetadataContext? metadata)
        {
            var list = components.List;
            var item = components.Item;

            if (list == null)
            {
                if (item == null)
                    return;

                if (MugenExtensions.OnAddingDefaultHandler(this, item, metadata))
                {
                    OnComponentAdded(item);
                    MugenExtensions.OnAddedDefaultHandler(this, item, metadata);
                    MergeComponents(item);
                }
                return;
            }

            int currentLength = 0;
            for (var i = 0; i < list.Length; i++)
            {
                var component = list[i];
                if (!MugenExtensions.OnAddingDefaultHandler(this, component, metadata))
                    continue;

                OnComponentAdded(component);
                MugenExtensions.OnAddedDefaultHandler(this, component, metadata);
                list[currentLength++] = list[i];
            }

            if (currentLength == 1)
                MergeComponents(list[0]);
            else if (currentLength != 0)
            {
                if (_components != null)
                {
                    if (_components is IComponent<IBinding>[] array)
                    {
                        var newSize = array.Length + currentLength;
                        if (newSize != list.Length)
                            Array.Resize(ref list, newSize);
                        for (int i = 0; i < array.Length; i++)
                            MugenExtensions.AddOrdered(list, array[i], currentLength++, this);
                        _components = list;
                        return;
                    }

                    if (list.Length == currentLength)
                    {
                        _components = MergeComponents(list, (IComponent<IBinding>)_components);
                        return;
                    }

                    MugenExtensions.AddOrdered(list, (IComponent<IBinding>)_components, currentLength, this);
                    ++currentLength;
                }

                if (list.Length != currentLength)
                    Array.Resize(ref list, currentLength);
                _components = list;
            }
        }

        protected virtual object? GetSourceValue(MemberPathLastMember targetMember)
        {
            return ((IMemberPathObserver)SourceRaw!).GetLastMember(this).GetValue(this);
        }

        protected virtual bool UpdateSourceInternal(out object? newValue)
        {
            if (SourceRaw is IMemberPathObserver sourceObserver)
            {
                var pathLastMember = sourceObserver.GetLastMember(this);
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

                newValue = MugenBindingService.GlobalValueConverter.Convert(newValue, pathLastMember.Member.Type, pathLastMember.Member, this);

                if (!CheckFlag(HasSourceValueSetterFlag) || !TrySetSourceValue(sourceObserver, pathLastMember, newValue))
                    pathLastMember.SetValue(newValue, this);
                return true;
            }
            newValue = null;
            return false;
        }

        protected virtual object? GetTargetValue(MemberPathLastMember sourceMember)
        {
            return Target.GetLastMember(this).GetValue(this);
        }

        protected virtual bool UpdateTargetInternal(out object? newValue)
        {
            var pathLastMember = Target.GetLastMember(this);
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
                newValue = InterceptTargetValue(Target, pathLastMember, newValue);
                if (newValue.IsUnsetValueOrDoNothing())
                    return false;
            }

            newValue = MugenBindingService.GlobalValueConverter.Convert(newValue, pathLastMember.Member.Type, pathLastMember.Member, this);

            if (!CheckFlag(HasTargetValueSetterFlag) || !TrySetTargetValue(Target, pathLastMember, newValue))
                pathLastMember.SetValue(newValue, this);
            return true;
        }

        protected virtual int GetMetadataCount() => 1;

        protected virtual IEnumerator<MetadataContextValue> GetMetadataEnumerator()
        {
            yield return MetadataContextValue.Create(BindingMetadata.Binding, this);
        }

        protected virtual bool TryGetMetadata<T>(IMetadataContextKey<T> contextKey, out T value, T defaultValue)
        {
            if (BindingMetadata.Binding.Equals(contextKey))
            {
                value = (T)(object)this;
                return true;
            }

            value = contextKey.GetDefaultValue(this, defaultValue);
            return false;
        }

        protected virtual bool ContainsMetadata(IMetadataContextKey contextKey)
        {
            return BindingMetadata.Binding.Equals(contextKey);
        }

        protected void OnTargetUpdateFailed(Exception error)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdateFailed(this, error, this);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdateFailed(this, error, this);
        }

        protected void OnTargetUpdateCanceled()
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdateCanceled(this, this);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdateCanceled(this, this);
        }

        protected void OnTargetUpdated(object? newValue)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdated(this, newValue, this);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdated(this, newValue, this);
        }

        protected void OnSourceUpdateFailed(Exception error)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdateFailed(this, error, this);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdateFailed(this, error, this);
        }

        protected void OnSourceUpdateCanceled()
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdateCanceled(this, this);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdateCanceled(this, this);
        }

        protected void OnSourceUpdated(object? newValue)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdated(this, newValue, this);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdated(this, newValue, this);
        }

        protected object? InterceptTargetValue(IMemberPathObserver targetObserver, MemberPathLastMember targetMember, object? value)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueInterceptorBindingComponent interceptor)
                        value = interceptor.InterceptTargetValue(targetObserver, targetMember, value, this);
                }
            }
            else if (components is ITargetValueInterceptorBindingComponent interceptor)
                value = interceptor.InterceptTargetValue(targetObserver, targetMember, value, this);

            return value;
        }

        protected object? InterceptSourceValue(IMemberPathObserver sourceObserver, MemberPathLastMember sourceMember, object? value)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueInterceptorBindingComponent interceptor)
                        value = interceptor.InterceptSourceValue(sourceObserver, sourceMember, value, this);
                }
            }
            else if (components is ISourceValueInterceptorBindingComponent interceptor)
                value = interceptor.InterceptSourceValue(sourceObserver, sourceMember, value, this);

            return value;
        }

        protected bool TrySetTargetValue(IMemberPathObserver targetObserver, MemberPathLastMember targetMember, object? newValue)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueSetterBindingComponent setter && setter.TrySetTargetValue(targetObserver, targetMember, newValue, this))
                        return true;
                }
            }
            else if (components is ITargetValueSetterBindingComponent setter && setter.TrySetTargetValue(targetObserver, targetMember, newValue, this))
                return true;

            return false;
        }

        protected bool TrySetSourceValue(IMemberPathObserver sourceObserver, MemberPathLastMember sourceMember, object? newValue)
        {
            var components = _components;
            if (components is IComponent<IBinding>[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueSetterBindingComponent setter && setter.TrySetSourceValue(sourceObserver, sourceMember, newValue, this))
                        return true;
                }
            }
            else if (components is ISourceValueSetterBindingComponent setter && setter.TrySetSourceValue(sourceObserver, sourceMember, newValue, this))
                return true;

            return false;
        }

        protected virtual void OnDispose()
        {
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

        private void MergeComponents(IComponent<IBinding> component)
        {
            if (_components == null)
                _components = component;
            else if (_components is IComponent<IBinding>[] array)
                _components = MergeComponents(array, component);
            else
                _components = MergeComponents((IComponent<IBinding>)_components, component);
        }

        private object MergeComponents(IComponent<IBinding> c1, IComponent<IBinding> c2)
        {
            return MugenExtensions.GetComponentPriority(c1, this) >= MugenExtensions.GetComponentPriority(c2, this)
                ? new[] { c1, c2 }
                : new[] { c2, c1 };
        }

        private object MergeComponents(IComponent<IBinding>[] components, IComponent<IBinding> component)
        {
            MugenExtensions.AddOrdered(ref components, component, this);
            return components;
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
                var observers = (IMemberPathObserver[]?)SourceRaw;
                if (observers != null)
                {
                    for (var i = 0; i < observers.Length; i++)
                        observers[i].AddListener(this);
                }
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
                    var observers = (IMemberPathObserver[]?)SourceRaw;
                    if (observers != null)
                    {
                        for (var i = 0; i < observers.Length; i++)
                            observers[i].RemoveListener(this);
                    }
                }
            }
        }

        #endregion
    }
}