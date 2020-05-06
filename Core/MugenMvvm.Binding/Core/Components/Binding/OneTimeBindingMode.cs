using System;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components.Binding
{
    public sealed class OneTimeBindingMode : IAttachableComponent, IBindingSourceObserverListener, IHasPriority
    {
        #region Fields

        private readonly bool _disposeBinding;

        public static readonly OneTimeBindingMode Instance = new OneTimeBindingMode(false);
        public static readonly OneTimeBindingMode DisposeBindingInstance = new OneTimeBindingMode(true);

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

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            return !Invoke((IBinding)owner, false);
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IBindingSourceObserverListener.OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            Invoke(binding, true);
        }

        void IBindingSourceObserverListener.OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            Invoke(binding, true);
        }

        void IBindingSourceObserverListener.OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
        }

        #endregion

        #region Methods

        private bool Invoke(IBinding binding, bool attached)
        {
            if (!binding.Target.IsAllMembersAvailable() || !MugenBindingExtensions.IsAllMembersAvailable(binding.Source))
                return false;

            binding.UpdateTarget();
            if (_disposeBinding)
                binding.Dispose();
            else if (attached)
                binding.RemoveComponent(this, null);
            return true;
        }

        #endregion
    }
}