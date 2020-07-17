using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation
{
    public sealed class ValidationManager : ComponentOwnerBase<IValidationManager>, IValidationManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ValidationManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IValidator? TryGetValidator(object? request, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IValidatorProviderComponent>(metadata).TryGetValidator(this, request, metadata);
            if (result != null)
                GetComponents<IValidatorProviderListener>(metadata).OnValidatorCreated(this, result, request, metadata);
            return result;
        }

        #endregion
    }
}