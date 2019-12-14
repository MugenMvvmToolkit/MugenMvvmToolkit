using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
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
            if (CheckFlag(HasSourceObserverListener))
                BindingComponentExtensions.RemoveListener(SourceRaw, this);
            if (SourceRaw is IDisposable disposable)
                disposable.Dispose();
            else if (SourceRaw is object[] sources)
            {
                for (int i = 0; i < sources.Length; i++)
                    (sources[i] as IDisposable)?.Dispose();
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
                        BindingComponentExtensions.OnTargetUpdated(_components, this, newValue, this);
                    else
                        BindingComponentExtensions.OnTargetUpdateCanceled(_components, this, this);
                }
            }
            catch (Exception e)
            {
                if (CheckFlag(HasTargetListenerFlag))
                    BindingComponentExtensions.OnTargetUpdateFailed(_components, this, e, this);
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
                        BindingComponentExtensions.OnSourceUpdated(_components, this, newValue, this);
                    else
                        BindingComponentExtensions.OnSourceUpdateCanceled(_components, this, this);
                }
            }
            catch (Exception e)
            {
                if (CheckFlag(HasSourceListenerFlag))
                    BindingComponentExtensions.OnSourceUpdateFailed(_components, this, e, this);
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
            if (ReferenceEquals(Target, observer))
                BindingComponentExtensions.OnTargetPathMembersChanged(_components, this, observer, this);
            else
                BindingComponentExtensions.OnSourcePathMembersChanged(_components, this, observer, this);
        }

        void IMemberPathObserverListener.OnLastMemberChanged(IMemberPathObserver observer)
        {
            if (ReferenceEquals(Target, observer))
                BindingComponentExtensions.OnTargetLastMemberChanged(_components, this, observer, this);
            else
                BindingComponentExtensions.OnSourceLastMemberChanged(_components, this, observer, this);
        }

        void IMemberPathObserverListener.OnError(IMemberPathObserver observer, Exception exception)
        {
            if (ReferenceEquals(Target, observer))
                BindingComponentExtensions.OnTargetError(_components, this, observer, exception, this);
            else
                BindingComponentExtensions.OnSourceError(_components, this, observer, exception, this);
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
            if (!CheckFlag(HasSourceLastMemberProviderFlag) || !BindingComponentExtensions.TryGetSourceLastMember(_components, this, this, out var pathLastMember))
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

            if (!CheckFlag(HasTargetValueGetterFlag) || !BindingComponentExtensions.TryGetTargetValue(_components, this, pathLastMember, this, out newValue))
                newValue = GetTargetValue(pathLastMember);

            if (CheckFlag(HasSourceValueInterceptorFlag))
                newValue = BindingComponentExtensions.InterceptSourceValue(_components, this, pathLastMember, newValue, this);

            if (newValue.IsDoNothing())
                return false;

            if (!CheckFlag(HasSourceValueSetterFlag) || !BindingComponentExtensions.TrySetSourceValue(_components, this, pathLastMember, newValue, this))
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
            if (!CheckFlag(HasTargetLastMemberProviderFlag) || !BindingComponentExtensions.TryGetTargetLastMember(_components, this, this, out var pathLastMember))
                pathLastMember = Target.GetLastMember(this);

            pathLastMember.ThrowIfError();
            if (!pathLastMember.IsAvailable)
            {
                newValue = null;
                return false;
            }

            if (!CheckFlag(HasSourceValueGetterFlag) || !BindingComponentExtensions.TryGetSourceValue(_components, this, pathLastMember, this, out newValue))
                newValue = GetSourceValue(pathLastMember);

            if (CheckFlag(HasTargetValueInterceptorFlag))
                newValue = BindingComponentExtensions.InterceptTargetValue(_components, this, pathLastMember, newValue, this);

            if (newValue.IsDoNothing())
                return false;

            if (!CheckFlag(HasTargetValueSetterFlag) || !BindingComponentExtensions.TrySetTargetValue(_components, this, pathLastMember, newValue, this))
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
            if (component == null || !ComponentComponentExtensions.OnComponentAdding(this, component, metadata))
                return false;

            return !CheckFlag(HasComponentChangingListener) || ComponentComponentExtensions.OnComponentAdding(_components, this, component, metadata);
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
                BindingComponentExtensions.AddListener(SourceRaw, this);
            }

            ComponentComponentExtensions.OnComponentAdded(this, component, metadata);
            if (CheckFlag(HasComponentChangedListener))
                ComponentComponentExtensions.OnComponentAdded(_components, this, component, metadata);
        }

        private bool OnComponentRemoving(object component, IReadOnlyMetadataContext? metadata)
        {
            if (!ComponentComponentExtensions.OnComponentRemoving(this, component, metadata))
                return false;
            return !CheckFlag(HasComponentChangingListener) || ComponentComponentExtensions.OnComponentRemoving(_components, this, component, metadata);
        }

        private void OnComponentRemoved(object component, bool isValidState, IReadOnlyMetadataContext? metadata)
        {
            if (isValidState)
            {
                if (component is IBindingTargetObserverListener && !ComponentComponentExtensions.HasComponent<IBindingTargetObserverListener>(_components))
                {
                    Target.RemoveListener(this);
                    ClearFlag(HasTargetObserverListener);
                }
                if (component is IBindingSourceObserverListener && !ComponentComponentExtensions.HasComponent<IBindingSourceObserverListener>(_components))
                {
                    BindingComponentExtensions.RemoveListener(SourceRaw, this);
                    ClearFlag(HasSourceObserverListener);
                }

                ComponentComponentExtensions.OnComponentRemoved(this, component, metadata);
                if (CheckFlag(HasComponentChangedListener))
                    ComponentComponentExtensions.OnComponentRemoved(_components, this, component, this);
            }
            else
                ComponentComponentExtensions.OnComponentRemoved(this, component, metadata);
        }

        #endregion
    }
}