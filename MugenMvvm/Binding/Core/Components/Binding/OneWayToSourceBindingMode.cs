using System;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components.Binding
{
    public sealed class OneWayToSourceBindingMode : IAttachableComponent, IBindingTargetObserverListener, IHasPriority
    {
        #region Fields

        public static readonly OneWayToSourceBindingMode Instance = new OneWayToSourceBindingMode();

        #endregion

        #region Constructors

        private OneWayToSourceBindingMode()
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
            if (!MugenBindingExtensions.IsAllMembersAvailable(binding.Source))
                binding.AddComponent(OneTimeHandlerComponent.Instance, metadata);
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

        internal sealed class OneTimeHandlerComponent : IBindingSourceObserverListener
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
                if (MugenBindingExtensions.IsAllMembersAvailable(binding.Source))
                {
                    binding.RemoveComponent(this, null);
                    binding.UpdateSource();
                }
            }

            #endregion
        }

        #endregion
    }
}