using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class DisableEqualityCheckingComponent : IComponent<IDataBinding>, IAttachableComponent<IDataBinding>
    {
        #region Fields

        private readonly bool _checkTwoWay;
        private readonly bool _sourceValue;
        private readonly bool _targetValue;

        #endregion

        #region Constructors

        public DisableEqualityCheckingComponent(bool targetValue, bool sourceValue, bool checkTwoWay)
        {
            _targetValue = targetValue;
            _sourceValue = sourceValue;
            _checkTwoWay = checkTwoWay;
        }

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent<IDataBinding>.OnAttaching(IDataBinding owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is IHasEqualityCheckingSettingsDataBinding binding)
            {
                binding.DisableEqualityCheckingTarget = _targetValue;
                binding.DisableEqualityCheckingSource = _sourceValue;
            }

            return false;
        }

        void IAttachableComponent<IDataBinding>.OnAttached(IDataBinding owner, IReadOnlyMetadataContext? metadata)
        {
        }

        int IComponent.GetPriority(object source)
        {
            return int.MinValue;
        }

        #endregion
    }
}