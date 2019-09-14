using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class DisableEqualityCheckingComponent : IComponent<IDataBinding>, IAttachableComponent<IDataBinding>, IHasPriority
    {
        #region Fields

        private readonly bool _sourceValue;
        private readonly bool _targetValue;

        #endregion

        #region Constructors

        public DisableEqualityCheckingComponent(bool targetValue, bool sourceValue)
        {
            _targetValue = targetValue;
            _sourceValue = sourceValue;
        }

        #endregion

        #region Properties

        public int Priority => int.MinValue;

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent<IDataBinding>.OnAttaching(IDataBinding owner, IReadOnlyMetadataContext? metadata)
        {
            owner.SetFlag(BindingMetadata.DisableEqualityCheckingTarget, _targetValue);
            owner.SetFlag(BindingMetadata.DisableEqualityCheckingSource, _sourceValue);
            return false;
        }

        void IAttachableComponent<IDataBinding>.OnAttached(IDataBinding owner, IReadOnlyMetadataContext? metadata)
        {
        }

        #endregion
    }
}