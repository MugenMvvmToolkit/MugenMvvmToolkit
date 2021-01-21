using MugenMvvm.Attributes;
using MugenMvvm.Collections;
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
        [Preserve(Conditional = true)]
        public ValidationManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        public IValidator? TryGetValidator(ItemOrIReadOnlyList<object> targets = default, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IValidatorProviderComponent>(metadata).TryGetValidator(this, targets, metadata);
            if (result != null)
                GetComponents<IValidationManagerListener>(metadata).OnValidatorCreated(this, result, targets, metadata);
            return result;
        }
    }
}