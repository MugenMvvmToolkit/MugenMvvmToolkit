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
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Core
{
    public abstract class DataBindingBase : ArrayComponentCollectionBase<IComponent<IDataBinding>>, IHasEqualityCheckingSettingsDataBinding, IBindingPathObserverListener,
        IComponentOwnerAddedCallback<IComponent<IDataBinding>>, IComponentOwnerRemovedCallback<IComponent<IDataBinding>>, IReadOnlyMetadataContext
    {
        #region Fields

        private byte _sourceObserverCount;
        private short _state;
        private byte _targetObserverCount;

        private const short AttachedFlag = 1;
        private const short DisposedFlag = 1 << 1;

        private const short DisableEqualityCheckingTargetFlag = 1 << 2;
        private const short DisableEqualityCheckingSourceFlag = 1 << 3;

        private const short HasTargetValueInterceptorFlag = 1 << 4;
        private const short HasSourceValueInterceptorFlag = 1 << 5;

        private const short HasTargetListenerFlag = 1 << 6;
        private const short HasSourceListenerFlag = 1 << 7;

        private const short HasTargetValueSetterFlag = 1 << 8;
        private const short HasSourceValueSetterFlag = 1 << 9;

        private const short TargetUpdatingFlag = 1 << 10;
        private const short SourceUpdatingFlag = 1 << 11;

        #endregion

        #region Constructors

        protected DataBindingBase(IBindingPathObserver target)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
        }

        #endregion

        #region Properties

        public bool HasComponents => HasItems;

        public IComponentCollection<IComponent<IDataBinding>> Components => this;

        public DataBindingState State
        {
            get
            {
                if (CheckFlag(DisposedFlag))
                    return DataBindingState.Disposed;
                return CheckFlag(AttachedFlag) ? DataBindingState.Attached : DataBindingState.Detached;
            }
        }

        public IBindingPathObserver Target { get; }

        public abstract IBindingPathObserver[] Sources { get; }

        public sealed override object Owner => this;

        public bool DisableEqualityCheckingTarget
        {
            get => CheckFlag(DisableEqualityCheckingTargetFlag);
            set
            {
                if (value)
                    SetFlag(DisableEqualityCheckingTargetFlag);
                else
                    ClearFlag(DisableEqualityCheckingTargetFlag);
            }
        }

        public bool DisableEqualityCheckingSource
        {
            get => CheckFlag(DisableEqualityCheckingSourceFlag);
            set
            {
                if (value)
                    SetFlag(DisableEqualityCheckingSourceFlag);
                else
                    ClearFlag(DisableEqualityCheckingSourceFlag);
            }
        }

        protected virtual IReadOnlyMetadataContext Metadata => this;

        protected sealed override bool IsOrdered => true;

        protected sealed override bool IsSynchronized => false;

        int IReadOnlyCollection<MetadataContextValue>.Count => 1;

        #endregion

        #region Implementation of interfaces

        void IBindingPathObserverListener.OnPathMembersChanged(IBindingPathObserver observer)
        {
            var isTarget = ReferenceEquals(Target, observer);
            var components = GetItems();
            if (isTarget)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IDataBindingTargetObserverListener)?.OnTargetPathMembersChanged(this, observer, Metadata);
            }
            else
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IDataBindingSourceObserverListener)?.OnSourcePathMembersChanged(this, observer, Metadata);
            }
        }

        void IBindingPathObserverListener.OnLastMemberChanged(IBindingPathObserver observer)
        {
            var isTarget = ReferenceEquals(Target, observer);
            var components = GetItems();
            if (isTarget)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IDataBindingTargetObserverListener)?.OnTargetLastMemberChanged(this, observer, Metadata);
            }
            else
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IDataBindingSourceObserverListener)?.OnSourceLastMemberChanged(this, observer, Metadata);
            }
        }

        void IBindingPathObserverListener.OnError(IBindingPathObserver observer, Exception exception)
        {
            var isTarget = ReferenceEquals(Target, observer);
            var components = GetItems();
            if (isTarget)
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IDataBindingTargetObserverListener)?.OnTargetError(this, observer, exception, Metadata);
            }
            else
            {
                for (var i = 0; i < components.Length; i++)
                    (components[i] as IDataBindingSourceObserverListener)?.OnSourceError(this, observer, exception, Metadata);
            }
        }

        void IComponentOwnerAddedCallback<IComponent<IDataBinding>>.OnComponentAdded(IComponentCollection<IComponent<IDataBinding>> collection, IComponent<IDataBinding> component, IReadOnlyMetadataContext? metadata)
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
            if (component is IDataBindingSourceObserverListener && ++_sourceObserverCount == 1)
            {
                if (this is ISingleSourceDataBinding singleSource)
                    singleSource.Source.AddListener(this);
                else
                {
                    var observers = Sources;
                    for (var i = 0; i < observers.Length; i++)
                        observers[i].AddListener(this);
                }
            }

            OnComponentAdded(component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IDataBinding>>.OnComponentRemoved(IComponentCollection<IComponent<IDataBinding>> collection, IComponent<IDataBinding> component, IReadOnlyMetadataContext? metadata)
        {
            if (CheckFlag(DisposedFlag))
                return;
            if (component is IDataBindingTargetObserverListener && --_targetObserverCount == 0)
                Target.RemoveListener(this);
            if (component is IDataBindingSourceObserverListener && --_sourceObserverCount == 0)
            {
                if (this is ISingleSourceDataBinding singleSource)
                    singleSource.Source.RemoveListener(this);
                else
                {
                    var observers = Sources;
                    for (var i = 0; i < observers.Length; i++)
                        observers[i].RemoveListener(this);
                }
            }

            OnComponentRemoved(component, metadata);
        }

        public void Dispose()
        {
            if (CheckFlag(DisposedFlag))
                return;
            SetFlag(DisposedFlag);
            OnDispose();
            if (_targetObserverCount != 0)
                Target.RemoveListener(this);
            Target.Dispose();
            if (this is ISingleSourceDataBinding singleSource)
            {
                if (_sourceObserverCount != 0)
                    singleSource.Source.RemoveListener(this);
                singleSource.Source.Dispose();
            }
            else
            {
                var sources = Sources;
                for (var i = 0; i < sources.Length; i++)
                {
                    var observer = sources[i];
                    if (_sourceObserverCount != 0)
                        observer.RemoveListener(this);
                    observer.Dispose();
                }
            }

            Clear();
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

        public void SetComponents(IComponent<IDataBinding>[] components)
        {
            if (Items.Length != 0)
                ExceptionManager.ThrowObjectInitialized(this);
            var listener = CallbackInvokerComponentCollectionComponent.GetComponentCollectionListener<IComponent<IDataBinding>>();
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (listener.OnAdding(this, component, Metadata))
                    listener.OnAdded(this, component, Metadata);
            }

            Items = components;
        }

        protected abstract object? GetSourceValue(IBindingMemberInfo lastMember);

        protected abstract bool UpdateSourceInternal(out object? newValue);

        protected virtual object? GetTargetValue(IBindingMemberInfo lastMember)
        {
            return Target.GetLastMember(Metadata).GetLastMemberValue(metadata: Metadata);
        }

        protected bool UpdateTargetInternal(out object? newValue)
        {
            return SetSourceValue(Target, out newValue);
        }

        protected virtual void OnTargetUpdateFailed(Exception error)
        {
            var components = GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IDataBindingTargetListener)?.OnTargetUpdateFailed(this, error, Metadata);
        }

        protected virtual void OnTargetUpdateCanceled()
        {
            var components = GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IDataBindingTargetListener)?.OnTargetUpdateCanceled(this, Metadata);
        }

        protected virtual void OnTargetUpdated(object? newValue)
        {
            var components = GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IDataBindingTargetListener)?.OnTargetUpdated(this, newValue, Metadata);
        }

        protected virtual void OnSourceUpdateFailed(Exception error)
        {
            var components = GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IDataBindingSourceListener)?.OnSourceUpdateFailed(this, error, Metadata);
        }

        protected virtual void OnSourceUpdateCanceled()
        {
            var components = GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IDataBindingSourceListener)?.OnSourceUpdateCanceled(this, Metadata);
        }

        protected virtual void OnSourceUpdated(object? newValue)
        {
            var components = GetItems();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IDataBindingSourceListener)?.OnSourceUpdated(this, newValue, Metadata);
        }

        protected virtual object? InterceptTargetValue(in BindingPathLastMember targetMembers, object? value)
        {
            var components = GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is ITargetValueInterceptorDataBindingComponent interceptor)
                    value = interceptor.InterceptTargetValue(targetMembers, value, Metadata);
            }

            return value;
        }

        protected virtual object? InterceptSourceValue(in BindingPathLastMember sourceMembers, object? value)
        {
            var components = GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is ISourceValueInterceptorDataBindingComponent interceptor)
                    value = interceptor.InterceptSourceValue(sourceMembers, value, Metadata);
            }

            return value;
        }

        protected virtual bool TrySetTargetValue(in BindingPathLastMember targetMembers, object? newValue)
        {
            var components = GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is ITargetValueSetterDataBindingComponent setter && setter.TrySetTargetValue(targetMembers, newValue, Metadata))
                    return true;
            }

            return false;
        }

        protected virtual bool TrySetSourceValue(in BindingPathLastMember sourceMembers, object? newValue)
        {
            var components = GetItems();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] is ISourceValueSetterDataBindingComponent setter && setter.TrySetSourceValue(sourceMembers, newValue, Metadata))
                    return true;
            }

            return false;
        }

        protected virtual void OnDispose()
        {
        }

        protected virtual void OnComponentAdded(IComponent<IDataBinding> component, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual void OnComponentRemoved(IComponent<IDataBinding> component, IReadOnlyMetadataContext? metadata)
        {
        }

        protected bool SetTargetValue(IBindingPathObserver targetObserver, out object? newValue)
        {
            var pathLastMember = targetObserver.GetLastMember(Metadata);
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

            newValue = Service<IGlobalBindingValueConverter>.Instance.Convert(newValue, pathLastMember.LastMember.Type, pathLastMember.LastMember, Metadata);
            if (!DisableEqualityCheckingTarget && pathLastMember.LastMember.CanRead)
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

            newValue = Service<IGlobalBindingValueConverter>.Instance.Convert(newValue, pathLastMember.LastMember.Type, pathLastMember.LastMember, Metadata);
            if (!DisableEqualityCheckingSource && pathLastMember.LastMember.CanRead)
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

        #endregion
    }
}