using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation
{
    public sealed class ValidatorProvider : ComponentOwnerBase<IValidatorProvider>, IValidatorProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ValidatorProvider(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IValidator? TryGetValidator<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IValidatorProviderComponent>(metadata).TryGetValidator(request, metadata);
            if (result != null)
                GetComponents<IValidatorProviderListener>(metadata).OnValidatorCreated(this, result, request, metadata);
            return result;
        }

        #endregion
    }
}