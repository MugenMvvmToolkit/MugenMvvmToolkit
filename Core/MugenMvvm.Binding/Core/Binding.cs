using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Core
{
    public class Binding : IBinding, IComponentCollection, IMemberPathObserverListener, IReadOnlyMetadataContext, IComparer<object>
    {
        #region Fields

        private object? _components;
        private int _state;

        protected const int TargetUpdatingFlag = 1;
        protected const int SourceUpdatingFlag = 1 << 1;

        protected const int HasTargetValueInterceptorFlag = 1 << 2;
        protected const int HasSourceValueInterceptorFlag = 1 << 3;

        protected const int HasTargetListenerFlag = 1 << 4;
        protected const int HasSourceListenerFlag = 1 << 5;

        protected const int HasTargetValueSetterFlag = 1 << 6;
        protected const int HasSourceValueSetterFlag = 1 << 7;

        protected const int HasComponentChangingListener = 1 << 8;
        protected const int HasComponentChangedListener = 1 << 9;
        protected const int HasTargetObserverListener = 1 << 10;
        protected const int HasSourceObserverListener = 1 << 11;

        protected const int HasTargetValueGetterFlag = 1 << 12;
        protected const int HasSourceValueGetterFlag = 1 << 13;

        protected const int HasTargetLastMemberProviderFlag = 1 << 14;
        protected const int HasSourceLastMemberProviderFlag = 1 << 15;

        protected const int InvalidFlag = 1 << 30;
        protected const int DisposedFlag = 1 << 31;

        #endregion

        #region Constructors

        public Binding(IMemberPathObserver target, object? sourceRaw)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            SourceRaw = sourceRaw;
        }

        #endregion

        #region Properties

        public IComponentCollection Components => this;

        public bool HasComponents => _components != null;

        object IComponentCollection.Owner => this;

        int IComponentCollection.Count
        {
            get
            {
                if (_components == null)
                    return 0;
                if (_components is object[] array)
                    return array.Length;
                return 1;
            }
        }

        bool IMetadataOwner<IReadOnlyMetadataContext>.HasMetadata => true;

        IReadOnlyMetadataContext IMetadataOwner<IReadOnlyMetadataContext>.Metadata => this;

        public BindingState State
        {
            get
            {
                if (CheckFlag(InvalidFlag))
                    return BindingState.Invalid;
                if (CheckFlag(DisposedFlag))
                    return BindingState.Disposed;
                return BindingState.Valid;
            }
        }

        public IMemberPathObserver Target { get; }

        public ItemOrList<object?, object?[]> Source => ItemOrList<object?, object?[]>.FromRawValue(SourceRaw);

        protected object? SourceRaw { get; }

        int IReadOnlyCollection<MetadataContextValue>.Count => GetMetadataCount();

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (CheckFlag(DisposedFlag))
                return;
            SetFlag(DisposedFlag | SourceUpdatingFlag | TargetUpdatingFlag);
            OnDispose();
            MugenBindingService.BindingManager.OnLifecycleChanged(this, BindingLifecycleState.Disposed, this);
            if (CheckFlag(HasTargetObserverListener))
                Target.RemoveListener(this);
            Target.Dispose();
            switch (SourceRaw)
            {
                case IMemberPathObserver source:
                    {
                        if (CheckFlag(HasSourceObserverListener))
                            source.RemoveListener(this);
                        source.Dispose();
                        break;
                    }
                case object[] sources:
                    {
                        for (var i = 0; i < sources.Length; i++)
                        {
                            var observer = sources[i] as IMemberPathObserver;
                            if (observer == null)
                                continue;
                            if (CheckFlag(HasSourceObserverListener))
                                observer.RemoveListener(this);
                            observer.Dispose();
                        }

                        break;
                    }
            }

            Components.Clear(null);
        }

        public ItemOrList<object, object[]> GetComponents()
        {
            return ItemOrList<object, object[]>.FromRawValue(_components);
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

        int IComparer<object>.Compare(object x, object y)
        {
            return MugenExtensions.GetComponentPriority(y, this).CompareTo(MugenExtensions.GetComponentPriority(x, this));
        }

        bool IComponentCollection.Add(object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(component, nameof(component));
            if (CheckFlag(DisposedFlag))
                return false;

            if (!OnComponentAdding(component, metadata))
                return false;

            if (_components == null)
                _components = component;
            else if (_components is object[] items)
            {
                MugenExtensions.AddOrdered(ref items, component, this);
                _components = items;
            }
            else
            {
                _components = MugenExtensions.GetComponentPriority(_components, this) >= MugenExtensions.GetComponentPriority(component, this)
                      ? new[] { _components, component }
                      : new[] { component, _components };
            }

            OnComponentAdded(component, metadata);
            return true;
        }

        bool IComponentCollection.Remove(object component, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(component, nameof(component));
            if (CheckFlag(DisposedFlag))
                return false;

            if (!RemoveComponent(component, metadata))
                return false;

            OnComponentRemoved(component, true, metadata);
            return true;
        }

        bool IComponentCollection.Clear(IReadOnlyMetadataContext? metadata)
        {
            var components = _components;
            _components = null;
            var isValid = !CheckFlag(DisposedFlag);
            if (components is object[] array)
            {
                for (var i = 0; i < array.Length; i++)
                    OnComponentRemoved(array[i], isValid, metadata);
            }
            else
            {
                var component = components;
                if (component != null)
                    OnComponentRemoved(component, isValid, metadata);
            }
            return true;
        }

        TComponent[] IComponentCollection.Get<TComponent>(IReadOnlyMetadataContext? metadata)
        {
            Should.MethodBeSupported(false, nameof(IComponentCollection.Get));
            return null!;
        }

        void IMemberPathObserverListener.OnPathMembersChanged(IMemberPathObserver observer)
        {
            var isTarget = ReferenceEquals(Target, observer);
            var components = _components;
            if (isTarget)
            {
                if (components is object[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingTargetObserverListener)?.OnTargetPathMembersChanged(this, observer, this);
                }
                else
                    (components as IBindingTargetObserverListener)?.OnTargetPathMembersChanged(this, observer, this);
            }
            else
            {
                if (components is object[] c)
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
                if (components is object[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingTargetObserverListener)?.OnTargetLastMemberChanged(this, observer, this);
                }
                else
                    (components as IBindingTargetObserverListener)?.OnTargetLastMemberChanged(this, observer, this);
            }
            else
            {
                if (components is object[] c)
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
                if (components is object[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                        (c[i] as IBindingTargetObserverListener)?.OnTargetError(this, observer, exception, this);
                }
                else
                    (components as IBindingTargetObserverListener)?.OnTargetError(this, observer, exception, this);
            }
            else
            {
                if (components is object[] c)
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

        public void Initialize(ItemOrList<IComponent<IBinding>?, IComponent<IBinding>?[]> components, IReadOnlyMetadataContext? metadata)
        {
            if (CheckFlag(DisposedFlag))
                return;
            if (_components != null)
                ExceptionManager.ThrowObjectInitialized(this);

            var list = components.List;
            if (list == null)
            {
                if (components.Item != null && OnComponentAdding(components.Item, metadata))
                {
                    _components = components.Item;
                    OnComponentAdded(components.Item, metadata);
                }
                return;
            }

            int currentLength = 0;
            for (var i = 0; i < list.Length; i++)
            {
                if (OnComponentAdding(list[i], metadata))
                    list[currentLength++] = list[i];
            }

            if (currentLength == 0)
                return;

            if (currentLength == 1)
            {
                _components = list[0];
                OnComponentAdded(list[0]!, metadata);
                return;
            }

            if (list.Length != currentLength)
                Array.Resize(ref list, currentLength);
            _components = list;
            for (int i = 0; i < list.Length; i++)
                OnComponentAdded(list[i]!, metadata);
        }

        protected virtual object? GetSourceValue(MemberPathLastMember targetMember)
        {
            return ((IMemberPathObserver)SourceRaw!).GetLastMember(this).GetValue(this);
        }

        protected virtual bool UpdateSourceInternal(out object? newValue)
        {
            if (!CheckFlag(HasSourceLastMemberProviderFlag) || !TryGetSourceLastMember(out var pathLastMember))
            {
                if (!(SourceRaw is IMemberPathObserver sourceObserver))
                {
                    newValue = null;
                    return false;
                }
                pathLastMember = sourceObserver.GetLastMember(this);
            }

            pathLastMember.ThrowIfError();
            if (!pathLastMember.IsAvailable)
            {
                newValue = null;
                return false;
            }

            if (!CheckFlag(HasTargetValueGetterFlag) || !TryGetTargetValue(pathLastMember, out newValue))
                newValue = GetTargetValue(pathLastMember);

            if (CheckFlag(HasSourceValueInterceptorFlag))
                newValue = InterceptSourceValue(pathLastMember, newValue);

            if (newValue.IsDoNothing())
                return false;

            if (!CheckFlag(HasSourceValueSetterFlag) || !TrySetSourceValue(pathLastMember, newValue))
            {
                if (newValue.IsUnsetValue())
                    return false;
                pathLastMember.SetValueWithConvert(newValue, this);
            }
            return true;
        }

        protected virtual object? GetTargetValue(MemberPathLastMember sourceMember)
        {
            return Target.GetLastMember(this).GetValue(this);
        }

        protected virtual bool UpdateTargetInternal(out object? newValue)
        {
            if (!CheckFlag(HasTargetLastMemberProviderFlag) || !TryGetTargetLastMember(out var pathLastMember))
                pathLastMember = Target.GetLastMember(this);

            pathLastMember.ThrowIfError();
            if (!pathLastMember.IsAvailable)
            {
                newValue = null;
                return false;
            }

            if (!CheckFlag(HasSourceValueGetterFlag) || !TryGetSourceValue(pathLastMember, out newValue))
                newValue = GetSourceValue(pathLastMember);

            if (CheckFlag(HasTargetValueInterceptorFlag))
                newValue = InterceptTargetValue(pathLastMember, newValue);

            if (newValue.IsDoNothing())
                return false;

            if (!CheckFlag(HasTargetValueSetterFlag) || !TrySetTargetValue(pathLastMember, newValue))
            {
                if (newValue.IsUnsetValue())
                    return false;
                pathLastMember.SetValueWithConvert(newValue, this);
            }
            return true;
        }

        protected virtual int GetMetadataCount() => 1;

        protected virtual IEnumerator<MetadataContextValue> GetMetadataEnumerator()
        {
            return Default.SingleValueEnumerator(MetadataContextValue.Create(BindingMetadata.Binding, this));
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
            if (components is object[] c)
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
            if (components is object[] c)
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
            if (components is object[] c)
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
            if (components is object[] c)
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
            if (components is object[] c)
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
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdated(this, newValue, this);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdated(this, newValue, this);
        }

        protected object? InterceptTargetValue(MemberPathLastMember targetMember, object? value)
        {
            var components = _components;
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueInterceptorBindingComponent interceptor)
                        value = interceptor.InterceptTargetValue(this, targetMember, value, this);
                }
            }
            else if (components is ITargetValueInterceptorBindingComponent interceptor)
                value = interceptor.InterceptTargetValue(this, targetMember, value, this);

            return value;
        }

        protected object? InterceptSourceValue(MemberPathLastMember sourceMember, object? value)
        {
            var components = _components;
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueInterceptorBindingComponent interceptor)
                        value = interceptor.InterceptSourceValue(this, sourceMember, value, this);
                }
            }
            else if (components is ISourceValueInterceptorBindingComponent interceptor)
                value = interceptor.InterceptSourceValue(this, sourceMember, value, this);

            return value;
        }

        protected bool TryGetTargetValue(MemberPathLastMember sourceMember, out object? value)
        {
            var components = _components;
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueGetterBindingComponent setter && setter.TryGetTargetValue(this, sourceMember, this, out value))
                        return true;
                }
            }
            else if (components is ITargetValueGetterBindingComponent setter && setter.TryGetTargetValue(this, sourceMember, this, out value))
                return true;

            value = null;
            return false;
        }

        protected bool TrySetTargetValue(MemberPathLastMember targetMember, object? newValue)
        {
            var components = _components;
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueSetterBindingComponent setter && setter.TrySetTargetValue(this, targetMember, newValue, this))
                        return true;
                }
            }
            else if (components is ITargetValueSetterBindingComponent setter && setter.TrySetTargetValue(this, targetMember, newValue, this))
                return true;

            return false;
        }

        protected bool TryGetSourceValue(MemberPathLastMember targetMember, out object? value)
        {
            var components = _components;
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueGetterBindingComponent setter && setter.TryGetSourceValue(this, targetMember, this, out value))
                        return true;
                }
            }
            else if (components is ISourceValueGetterBindingComponent setter && setter.TryGetSourceValue(this, targetMember, this, out value))
                return true;

            value = null;
            return false;
        }

        protected bool TrySetSourceValue(MemberPathLastMember sourceMember, object? newValue)
        {
            var components = _components;
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueSetterBindingComponent setter && setter.TrySetSourceValue(this, sourceMember, newValue, this))
                        return true;
                }
            }
            else if (components is ISourceValueSetterBindingComponent setter && setter.TrySetSourceValue(this, sourceMember, newValue, this))
                return true;

            return false;
        }

        protected bool TryGetTargetLastMember(out MemberPathLastMember targetMember)
        {
            var components = _components;
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetLastMemberProviderBindingComponent provider && provider.TryGetTargetLastMember(this, this, out targetMember))
                        return true;
                }
            }
            else if (components is ITargetLastMemberProviderBindingComponent provider && provider.TryGetTargetLastMember(this, this, out targetMember))
                return true;

            targetMember = default;
            return false;
        }

        protected bool TryGetSourceLastMember(out MemberPathLastMember sourceMember)
        {
            var components = _components;
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceLastMemberProviderBindingComponent provider && provider.TryGetSourceLastMember(this, this, out sourceMember))
                        return true;
                }
            }
            else if (components is ISourceLastMemberProviderBindingComponent provider && provider.TryGetSourceLastMember(this, this, out sourceMember))
                return true;

            sourceMember = default;
            return false;
        }

        protected virtual void OnDispose()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckFlag(int flag)
        {
            return (_state & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetFlag(int flag)
        {
            _state |= flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ClearFlag(int flag)
        {
            _state &= ~flag;
        }

        private bool RemoveComponent(object component, IReadOnlyMetadataContext? metadata)
        {
            if (_components == null)
                return false;

            if (ReferenceEquals(component, _components))
            {
                if (!OnComponentRemoving(component, metadata))
                    return false;

                _components = null;
                return true;
            }

            if (!(_components is object[] items))
                return false;

            if (items.Length == 2)
            {
                if (ReferenceEquals(items[0], component))
                {
                    if (!OnComponentRemoving(component, metadata))
                        return false;

                    _components = items[1];
                    return true;
                }

                if (ReferenceEquals(items[1], component))
                {
                    if (!OnComponentRemoving(component, metadata))
                        return false;

                    _components = items[0];
                    return true;
                }
            }
            else if (MugenExtensions.Remove(ref items, component))
            {
                if (!OnComponentRemoving(component, metadata))
                    return false;

                _components = items;
                return true;
            }

            return false;
        }

        private bool OnComponentAdding(object? component, IReadOnlyMetadataContext? metadata)
        {
            if (component == null || !MugenExtensions.ComponentCollectionOnComponentAdding(this, component, metadata))
                return false;

            if (CheckFlag(HasComponentChangingListener))
            {
                var components = _components;
                if (components is object[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                    {
                        if (c[i] is IComponentCollectionChangingListener listener && !listener.OnAdding(this, component, metadata))
                            return false;
                    }
                }
                else if (components is IComponentCollectionChangingListener listener && !listener.OnAdding(this, component, metadata))
                    return false;
            }

            return true;
        }

        private void OnComponentAdded(object component, IReadOnlyMetadataContext? metadata)
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
            if (component is IComponentCollectionChangingListener)
                SetFlag(HasComponentChangingListener);
            if (component is IComponentCollectionChangedListener)
                SetFlag(HasComponentChangedListener);
            if (component is ISourceValueGetterBindingComponent)
                SetFlag(HasSourceValueGetterFlag);
            if (component is ITargetValueGetterBindingComponent)
                SetFlag(HasTargetValueGetterFlag);
            if (component is ISourceLastMemberProviderBindingComponent)
                SetFlag(HasSourceLastMemberProviderFlag);
            if (component is ITargetLastMemberProviderBindingComponent)
                SetFlag(HasTargetLastMemberProviderFlag);
            if (!CheckFlag(HasTargetObserverListener) && component is IBindingTargetObserverListener)
            {
                SetFlag(HasTargetObserverListener);
                Target.AddListener(this);
            }
            if (!CheckFlag(HasSourceObserverListener) && component is IBindingSourceObserverListener)
            {
                SetFlag(HasSourceObserverListener);
                switch (SourceRaw)
                {
                    case IMemberPathObserver source:
                        source.AddListener(this);
                        break;
                    case object[] sources:
                        {
                            for (var i = 0; i < sources.Length; i++)
                                (sources[i] as IMemberPathObserver)?.AddListener(this);
                            break;
                        }
                }
            }

            MugenExtensions.ComponentCollectionOnComponentAdded(this, component, metadata);
            if (CheckFlag(HasComponentChangedListener))
            {
                var components = _components;
                if (components is object[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                    {
                        var comp = c[i];
                        if (!ReferenceEquals(comp, component))
                            (comp as IComponentCollectionChangedListener)?.OnAdded(this, component, this);
                    }
                }
                else if (!ReferenceEquals(components, component))
                    (components as IComponentCollectionChangedListener)?.OnAdded(this, component, this);
            }
        }

        private bool OnComponentRemoving(object component, IReadOnlyMetadataContext? metadata)
        {
            if (!MugenExtensions.ComponentCollectionOnComponentRemoving(this, component, metadata))
                return false;
            if (CheckFlag(HasComponentChangingListener))
            {
                var components = _components;
                if (components is object[] c)
                {
                    for (var i = 0; i < c.Length; i++)
                    {
                        if (c[i] is IComponentCollectionChangingListener listener && !ReferenceEquals(listener, component) && !listener.OnRemoving(this, component, metadata))
                            return false;
                    }
                }
                else if (components is IComponentCollectionChangingListener listener && !ReferenceEquals(listener, component) && !listener.OnRemoving(this, component, metadata))
                    return false;
            }

            return true;
        }

        private void OnComponentRemoved(object component, bool isValidState, IReadOnlyMetadataContext? metadata)
        {
            if (isValidState)
            {
                if (component is IBindingTargetObserverListener && !HasComponent<IBindingTargetObserverListener>())
                {
                    Target.RemoveListener(this);
                    ClearFlag(HasTargetObserverListener);
                }
                if (component is IBindingSourceObserverListener && !HasComponent<IBindingSourceObserverListener>())
                {
                    switch (SourceRaw)
                    {
                        case IMemberPathObserver source:
                            source.RemoveListener(this);
                            break;
                        case object[] sources:
                            {
                                for (var i = 0; i < sources.Length; i++)
                                    (sources[i] as IMemberPathObserver)?.RemoveListener(this);
                                break;
                            }
                    }

                    ClearFlag(HasSourceObserverListener);
                }

                MugenExtensions.ComponentCollectionOnComponentRemoved(this, component, metadata);
                if (CheckFlag(HasComponentChangedListener))
                {
                    var components = _components;
                    if (components is object[] c)
                    {
                        for (var i = 0; i < c.Length; i++)
                            (c[i] as IComponentCollectionChangedListener)?.OnRemoved(this, component, this);
                    }
                    else
                        (components as IComponentCollectionChangedListener)?.OnRemoved(this, component, this);
                }
            }
            else
                MugenExtensions.ComponentCollectionOnComponentRemoved(this, component, metadata);
        }

        private bool HasComponent<TComponent>() where TComponent : class, IComponent<IBinding>
        {
            var components = _components;
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is TComponent)
                        return true;
                }

                return false;
            }

            return components is TComponent;
        }

        #endregion
    }
}