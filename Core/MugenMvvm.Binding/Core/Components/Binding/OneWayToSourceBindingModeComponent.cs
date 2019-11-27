using System;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components.Binding
{
    public sealed class OneWayToSourceBindingModeComponent : IAttachableComponent, IBindingTargetObserverListener, IHasPriority
    {
        #region Fields

        public static readonly OneWayToSourceBindingModeComponent Instance = new OneWayToSourceBindingModeComponent();

        #endregion

        #region Constructors

        private OneWayToSourceBindingModeComponent()
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
            binding.UpdateSource();
            if (!binding.Source.IsAllMembersAvailable())
                binding.Components.Add(OneTimeHandlerComponent.Instance);
        }

        void IBindingTargetObserverListener.OnTargetPathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            binding.UpdateSource();
        }

        void IBindingTargetObserverListener.OnTargetLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            binding.UpdateSource();
        }

        void IBindingTargetObserverListener.OnTargetError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
        }

        #endregion

        #region Nested types

        private sealed class OneTimeHandlerComponent : IBindingSourceObserverListener
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

            public void OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
            {
                Invoke(binding);
            }

            public void OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
            {
                Invoke(binding);
            }

            public void OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
            {
            }

            #endregion

            #region Methods

            private void Invoke(IBinding binding)
            {
                if (binding.Source.IsAllMembersAvailable())
                {
                    binding.Components.Remove(this);
                    binding.UpdateSource();
                }
            }

            #endregion
        }

        #endregion
    }
}