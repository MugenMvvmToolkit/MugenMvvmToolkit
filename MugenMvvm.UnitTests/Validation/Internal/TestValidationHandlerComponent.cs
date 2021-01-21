using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Validation.Internal
{
    public class TestValidationHandlerComponent : IValidationHandlerComponent, IHasPriority
    {
        private readonly IValidator? _validator;

        public TestValidationHandlerComponent(IValidator? validator)
        {
            _validator = validator;
        }

        public Func<string?, CancellationToken, IReadOnlyMetadataContext?, Task>? TryValidateAsync { get; set; }

        public int Priority { get; set; }

        Task IValidationHandlerComponent.TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            _validator?.ShouldEqual(validator);
            return TryValidateAsync?.Invoke(member, cancellationToken, metadata) ?? Default.CompletedTask;
        }
    }
}