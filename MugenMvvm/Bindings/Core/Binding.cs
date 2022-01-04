using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core
{
    public class Binding : IBinding, IComponentCollection, IMemberPathObserverListener, IReadOnlyMetadataContext, IComparer<object> //todo synchronized, all components synchronized
    {
        protected const int TargetUpdatingFlag = 1;//todo convert by expression implicit
        protected const int SourceUpdatingFlag = 1 << 1;

        protected const int HasTargetValueInterceptorFlag = 1 << 2;
        protected const int HasSourceValueInterceptorFlag = 1 << 3;

        protected const int HasTargetListenerFlag = 1 << 4;
        protected const int HasSourceListenerFlag = 1 << 5;

        protected const int HasTargetValueSetterFlag = 1 << 6;
        protected const int HasSourceValueSetterFlag = 1 << 7;

        protected const int HasComponentCondition = 1 << 8;
        protected const int HasComponentChangingListener = 1 << 9;
        protected const int HasComponentChangedListener = 1 << 10;
        protected const int HasTargetObserverListener = 1 << 11;
        protected const int HasSourceObserverListener = 1 << 12;

        protected const int HasItem = 1 << 29;
        protected const int InvalidFlag = 1 << 30;
        protected const int DisposedFlag = 1 << 31;

        private object? _components;
        private int _state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Binding(IMemberPathObserver target, object? sourceRaw)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            SourceRaw = sourceRaw;
            SetFlag(HasItem);
        }

        public BindingState State
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (CheckFlag(InvalidFlag))
                    return BindingState.Invalid;
                if (CheckFlag(DisposedFlag))
                    return BindingState.Disposed;
                return BindingState.Valid;
            }
        }

        public IMemberPathObserver Target { get; private set; }

        public ItemOrArray<object?> Source
        {
            get
            {
                if (SourceRaw is object?[] objects)
                    return ItemOrArray.FromList(objects);
                return ItemOrArray.FromItem<object?>(SourceRaw, CheckFlag(HasItem));
            }
        }

        public IComponentCollection Components => this;

        public bool HasComponents => _components != null;

        protected object? SourceRaw { get; private set; }

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

        int IReadOnlyMetadataContext.Count => GetMetadataCount();

        public void Initialize(ItemOrArray<object?> components, IReadOnlyMetadataContext? metadata)
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

            var currentLength = 0;
            for (var i = 0; i < list.Length; i++)
            {
                if (OnComponentAdding(list[i], metadata))
                    list[currentLength++] = list[i];
            }

            if (CheckFlag(DisposedFlag))
                return;

            if (currentLength == 0)
                return;

            if (currentLength == 1)
            {
                _components = list[0];
                OnComponentAdded(list[0]!, metadata);
            }
            else
            {
                if (list.Length != currentLength)
                    Array.Resize(ref list, currentLength);
                _components = list;
                for (var i = 0; i < list.Length; i++)
                    OnComponentAdded(list[i]!, metadata);
            }
        }

        public ItemOrArray<object> GetComponents() => ItemOrArray.FromRawValueFixedArray<object>(_components);

        public void UpdateTarget()
        {
            if (CheckFlag(TargetUpdatingFlag))
                return;
            try
            {
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
            if (CheckFlag(SourceUpdatingFlag))
                return;
            try
            {
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

        public void Dispose()
        {
            if (CheckFlag(DisposedFlag))
                return;
            SetFlag(DisposedFlag | SourceUpdatingFlag | TargetUpdatingFlag);
            OnDispose();
        }

        protected virtual object? GetTargetValue(MemberPathLastMember sourceMember) => Target.GetLastMember(this).GetValueOrThrow(this);

        protected virtual object? GetSourceValue(MemberPathLastMember targetMember)
        {
            if (SourceRaw is IMemberPathObserver memberPath)
                return memberPath.GetLastMember(this).GetValueOrThrow(this);
            if (SourceRaw is IConstantExpressionNode constantExpressionNode)
                return constantExpressionNode.Value;
            return SourceRaw;
        }

        protected virtual bool UpdateSourceInternal(out object? newValue)
        {
            if (SourceRaw is not IMemberPathObserver sourceObserver)
            {
                newValue = null;
                return false;
            }

            var pathLastMember = sourceObserver.GetLastMember(this);
            if (!pathLastMember.ThrowIfError())
            {
                newValue = null;
                return false;
            }

            newValue = GetTargetValue(pathLastMember);
            if (CheckFlag(HasSourceValueInterceptorFlag))
                newValue = BindingComponentExtensions.InterceptSourceValue(_components, this, pathLastMember, newValue, this);

            if (newValue.IsDoNothing())
                return false;

            if (!CheckFlag(HasSourceValueSetterFlag) || !BindingComponentExtensions.TrySetSourceValue(_components, this, pathLastMember, newValue, this))
            {
                if (newValue.IsUnsetValue())
                {
                    pathLastMember.TrySetDefault(this);
                    return false;
                }

                pathLastMember.TrySetValueWithConvert(newValue, this);
            }

            return true;
        }

        protected virtual bool UpdateTargetInternal(out object? newValue)
        {
            var pathLastMember = Target.GetLastMember(this);
            if (!pathLastMember.ThrowIfError())
            {
                newValue = null;
                return false;
            }

            newValue = GetSourceValue(pathLastMember);//todo review delay for expression
            if (CheckFlag(HasTargetValueInterceptorFlag))
                newValue = BindingComponentExtensions.InterceptTargetValue(_components, this, pathLastMember, newValue, this);

            if (newValue.IsDoNothing())
                return false;

            if (!CheckFlag(HasTargetValueSetterFlag) || !BindingComponentExtensions.TrySetTargetValue(_components, this, pathLastMember, newValue, this))
            {
                if (newValue.IsUnsetValue())
                {
                    pathLastMember.TrySetDefault(this);
                    return false;
                }

                pathLastMember.TrySetValueWithConvert(newValue, this);
            }

            return true;
        }

        protected virtual int GetMetadataCount() => 1;

        protected virtual ItemOrIReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>> GetMetadataValues() => BindingMetadata.Binding.ToValue(this);

        protected virtual bool TryGetMetadata(IMetadataContextKey contextKey, out object? value)
        {
            if (BindingMetadata.Binding.Equals(contextKey))
            {
                value = this;
                return true;
            }

            value = null;
            return false;
        }

        protected virtual bool ContainsMetadata(IMetadataContextKey contextKey) => BindingMetadata.Binding.Equals(contextKey);

        protected virtual void OnDispose()
        {
            InternalComponentExtensions.OnDisposing<IBinding>(_components, this, null);
            MugenService.BindingManager.OnLifecycleChanged(this, BindingLifecycleState.Disposed);
            if (CheckFlag(HasTargetObserverListener))
                Target.RemoveListener(this);
            if (CheckFlag(HasSourceObserverListener))
                BindingComponentExtensions.RemoveListener(SourceRaw, this);
            InternalComponentExtensions.OnDisposed<IBinding>(_components, this, null);
            Target.Dispose();
            BindingMugenExtensions.DisposeBindingSource(SourceRaw);
            Components.Clear();
            Target = EmptyPathObserver.Empty;
            SourceRaw = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CheckFlag(int flag) => (_state & flag) == flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetFlag(int flag) => _state |= flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ClearFlag(int flag) => _state &= ~flag;

        private bool RemoveComponent(object component, IReadOnlyMetadataContext? metadata)
        {
            if (_components == null)
                return false;

            if (component == _components)
            {
                if (!OnComponentRemoving(component, metadata))
                    return false;

                _components = null;
                return true;
            }

            if (_components is not object[] items)
                return false;

            if (items.Length == 2)
            {
                if (items[0] == component)
                {
                    if (!OnComponentRemoving(component, metadata))
                        return false;

                    _components = items[1];
                    return true;
                }

                if (items[1] == component)
                {
                    if (!OnComponentRemoving(component, metadata))
                        return false;

                    _components = items[0];
                    return true;
                }
            }
            else
            {
                var index = Array.IndexOf(items, component);
                if (index < 0)
                    return false;

                if (!OnComponentRemoving(component, metadata))
                    return false;

                MugenExtensions.RemoveAt(ref items, index);
                _components = items;
                return true;
            }

            return false;
        }

        private bool OnComponentAdding(object? component, IReadOnlyMetadataContext? metadata)
        {
            if (CheckFlag(DisposedFlag))
                return false;
            if (component == null || !ComponentComponentExtensions.CanAdd(this, component, metadata))
                return false;

            if (CheckFlag(HasComponentCondition) && !ComponentComponentExtensions.CanAdd(_components, this, component, metadata))
                return false;

            ComponentComponentExtensions.OnComponentAdding(this, component, metadata);
            if (CheckFlag(HasComponentChangingListener))
                ComponentComponentExtensions.OnComponentAdding(_components, this, component, metadata);
            return true;
        }

        private void OnComponentAdded(object component, IReadOnlyMetadataContext? metadata)
        {
            if (CheckFlag(DisposedFlag))
                return;
            if (component is ISourceValueInterceptorComponent)
                SetFlag(HasSourceValueInterceptorFlag);
            if (component is ITargetValueInterceptorComponent)
                SetFlag(HasTargetValueInterceptorFlag);
            if (component is IBindingSourceListener)
                SetFlag(HasSourceListenerFlag);
            if (component is IBindingTargetListener)
                SetFlag(HasTargetListenerFlag);
            if (component is ITargetValueSetterComponent)
                SetFlag(HasTargetValueSetterFlag);
            if (component is ISourceValueSetterComponent)
                SetFlag(HasSourceValueSetterFlag);
            if (component is IComponentCollectionChangingListener)
                SetFlag(HasComponentChangingListener);
            if (component is IConditionComponentCollectionComponent)
                SetFlag(HasComponentCondition);
            if (component is IComponentCollectionChangedListener)
                SetFlag(HasComponentChangedListener);

            if (!CheckFlag(HasSourceObserverListener) && component is IBindingSourceObserverListener)
            {
                SetFlag(HasSourceObserverListener);
                BindingComponentExtensions.AddListener(SourceRaw, this);
            }

            if (!CheckFlag(HasTargetObserverListener) && component is IBindingTargetObserverListener)
            {
                SetFlag(HasTargetObserverListener);
                Target.AddListener(this);
            }

            ComponentComponentExtensions.OnComponentAdded(this, component, metadata);
            if (CheckFlag(HasComponentChangedListener))
                ComponentComponentExtensions.OnComponentAdded(_components, this, component, metadata);
        }

        private bool OnComponentRemoving(object component, IReadOnlyMetadataContext? metadata)
        {
            if (!ComponentComponentExtensions.CanRemove(this, component, metadata))
                return false;

            if (CheckFlag(HasComponentCondition) && !ComponentComponentExtensions.CanRemove(_components, this, component, metadata))
                return false;

            ComponentComponentExtensions.OnComponentRemoving(this, component, metadata);
            if (CheckFlag(HasComponentChangingListener))
                ComponentComponentExtensions.OnComponentRemoving(_components, this, component, metadata);
            return true;
        }

        private void OnComponentRemoved(object? components, object component, IReadOnlyMetadataContext? metadata)
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
                ComponentComponentExtensions.OnComponentRemoved(components, this, component, metadata);
        }

        int IComparer<object>.Compare(object? x, object? y) => MugenExtensions.GetComponentPriority(y!).CompareTo(MugenExtensions.GetComponentPriority(x!));

        object? IComponentCollection.TryAdd<T>(T state, Func<IComponentCollection, T, IReadOnlyMetadataContext?, object?> tryGetComponent, IReadOnlyMetadataContext? metadata)
        {
            var component = tryGetComponent(this, state, metadata);
            if (component == null)
                return null;

            if (Components.TryAdd(component, metadata))
                return component;
            return null;
        }

        bool IComponentCollection.TryAdd(object component, IReadOnlyMetadataContext? metadata)
        {
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
                _components = MugenExtensions.GetComponentPriority(_components) >= MugenExtensions.GetComponentPriority(component)
                    ? new[] {_components, component}
                    : new[] {component, _components};
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

            OnComponentRemoved(_components, component, metadata);
            return true;
        }

        void IComponentCollection.Clear(IReadOnlyMetadataContext? metadata)
        {
            var components = _components;
            if (CheckFlag(DisposedFlag))
            {
                if (components is object[] array)
                {
                    foreach (var c in array)
                        ComponentComponentExtensions.OnComponentRemoving(this, c, metadata);
                    _components = null;
                    foreach (var c in array)
                        ComponentComponentExtensions.OnComponentRemoved(this, c, metadata);
                }
                else if (components != null)
                {
                    ComponentComponentExtensions.OnComponentRemoving(this, components, metadata);
                    _components = null;
                    ComponentComponentExtensions.OnComponentRemoved(this, components, metadata);
                }
            }
            else
            {
                if (components is object[] array)
                {
                    foreach (var o in array)
                        Components.Remove(o, metadata);
                }
                else if (components != null)
                    Components.Remove(components, metadata);
            }
        }

        ItemOrArray<T> IComponentCollection.Get<T>(IReadOnlyMetadataContext? metadata)
        {
            if (typeof(T) == typeof(object))
                return ItemOrArray.FromRawValue<T>(_components);

            var components = new ItemOrListEditor<T>();
            foreach (var item in ItemOrArray.FromRawValue<object>(_components))
            {
                if (item is T component)
                    components.Add(component);
            }

            return components.ToItemOrArray();
        }

        void IHasCache.Invalidate(object? component, IReadOnlyMetadataContext? metadata)
        {
            if (_components is object[] array)
                Array.Sort(array, this);
        }

        void IMemberPathObserverListener.OnPathMembersChanged(IMemberPathObserver observer)
        {
            if (Target == observer)
                BindingComponentExtensions.OnTargetPathMembersChanged(_components, this, observer, this);
            else
                BindingComponentExtensions.OnSourcePathMembersChanged(_components, this, observer, this);
        }

        void IMemberPathObserverListener.OnLastMemberChanged(IMemberPathObserver observer)
        {
            if (Target == observer)
                BindingComponentExtensions.OnTargetLastMemberChanged(_components, this, observer, this);
            else
                BindingComponentExtensions.OnSourceLastMemberChanged(_components, this, observer, this);
        }

        void IMemberPathObserverListener.OnError(IMemberPathObserver observer, Exception exception)
        {
            if (Target == observer)
                BindingComponentExtensions.OnTargetError(_components, this, observer, exception, this);
            else
                BindingComponentExtensions.OnSourceError(_components, this, observer, exception, this);
        }

        ItemOrIReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>> IReadOnlyMetadataContext.GetValues() => GetMetadataValues();

        bool IReadOnlyMetadataContext.Contains(IMetadataContextKey contextKey) => ContainsMetadata(contextKey);

        bool IReadOnlyMetadataContext.TryGetRaw(IMetadataContextKey contextKey, out object? value) => TryGetMetadata(contextKey, out value);
    }
}