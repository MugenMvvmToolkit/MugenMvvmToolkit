using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class AggregatorValidatorProviderComponent : IAggregatorValidatorProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ValidationComponentPriority.AggregatorProvider;

        #endregion

        #region Implementation of interfaces

        public IAggregatorValidator? TryGetAggregatorValidator<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return new AggregatorValidator();
        }

        #endregion
    }
}