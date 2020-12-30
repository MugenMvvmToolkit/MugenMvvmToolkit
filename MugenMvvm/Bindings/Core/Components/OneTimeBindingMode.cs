using System;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class OneTimeBindingMode : IAttachableComponent, IBindingSourceObserverListener, IHasPriority
    {
        #region Fields

        private readonly bool _disposeBinding;

        public static readonly OneTimeBindingMode Instance = new(true);
        public static readonly OneTimeBindingMode NonDisposeInstance = new(false);

        #endregion

        #region Constructors

        private OneTimeBindingMode(bool disposeBinding)
        {
            _disposeBinding = disposeBinding;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.Mode;

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => !Invoke((IBinding) owner, false);

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IBindingSourceObserverListener.OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => Invoke(binding, true);

        void IBindingSourceObserverListener.OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => Invoke(binding, true);

        void IBindingSourceObserverListener.OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
        }

        #endregion

        #region Methods

        private bool Invoke(IBinding binding, bool attached)
        {
            if (!binding.Target.IsAllMembersAvailable() || !BindingMugenExtensions.IsAllMembersAvailable(binding.Source))
                return false;

            binding.UpdateTarget();
            if (_disposeBinding)
                binding.Dispose();
            else if (attached)
                binding.RemoveComponent(this);
            return true;
        }

        #endregion
    }
}