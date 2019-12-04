using System;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components.Binding
{
    public sealed class OneWayBindingModeComponent : IAttachableComponent, IBindingSourceObserverListener, IHasPriority
    {
        #region Fields

        public static readonly OneWayBindingModeComponent Instance = new OneWayBindingModeComponent();

        #endregion

        #region Constructors

        private OneWayBindingModeComponent()
        {
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.Mode;

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            var binding = (IBinding)owner;
            binding.UpdateTarget();
            if (!binding.Target.IsAllMembersAvailable())
                binding.AddComponent(OneTimeHandlerComponent.Instance, metadata);
        }

        void IBindingSourceObserverListener.OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            binding.UpdateTarget();
        }

        void IBindingSourceObserverListener.OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            binding.UpdateTarget();
        }

        void IBindingSourceObserverListener.OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
        }

        #endregion

        #region Nested types

        private sealed class OneTimeHandlerComponent : IBindingTargetObserverListener
        {
            #region Fields

            public static readonly OneTimeHandlerComponent Instance = new OneTimeHandlerComponent();

            #endregion

            #region Constructors

            private OneTimeHandlerComponent()
            {
            }

            #endregion

            #region Implementation of interfaces

            void IBindingTargetObserverListener.OnTargetPathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
            {
                Invoke(binding);
            }

            void IBindingTargetObserverListener.OnTargetLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
            {
                Invoke(binding);
            }

            void IBindingTargetObserverListener.OnTargetError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
            {
            }

            #endregion

            #region Methods

            private void Invoke(IBinding binding)
            {
                if (binding.Target.IsAllMembersAvailable())
                {
                    binding.RemoveComponent(this, null);
                    binding.UpdateTarget();
                }
            }

            #endregion
        }

        #endregion
    }
}