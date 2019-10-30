﻿using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingHolderStateDispatcherComponent : AttachableComponentBase<IBindingManager>, IBindingStateDispatcherComponent,
        IComponentCollectionChangedListener<IComponent<IBindingManager>>
    {
        #region Fields

        private IBindingHolderComponent[] _holders;

        #endregion

        #region Constructors

        public BindingHolderStateDispatcherComponent()
        {
            _holders = Default.EmptyArray<IBindingHolderComponent>();
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyMetadataContext? OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycle, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycle == BindingLifecycleState.Initialized)
            {
                for (var i = 0; i < _holders.Length; i++)
                {
                    if (_holders[i].TryRegister(binding, metadata))
                        break;
                }
            }
            else if (lifecycle == BindingLifecycleState.Disposed)
            {
                for (var i = 0; i < _holders.Length; i++)
                {
                    if (_holders[i].TryUnregister(binding, metadata))
                        break;
                }
            }

            return null;
        }

        void IComponentCollectionChangedListener<IComponent<IBindingManager>>.OnAdded(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _holders, collection, component);
        }

        void IComponentCollectionChangedListener<IComponent<IBindingManager>>.OnRemoved(IComponentCollection<IComponent<IBindingManager>> collection,
            IComponent<IBindingManager> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _holders, component);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            _holders = owner.Components.GetItems().OfType<IBindingHolderComponent>().ToArray();
            owner.Components.Components.Add(this);
        }

        protected override void OnDetachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.Components.Remove(this);
            _holders = Default.EmptyArray<IBindingHolderComponent>();
        }

        #endregion
    }
}