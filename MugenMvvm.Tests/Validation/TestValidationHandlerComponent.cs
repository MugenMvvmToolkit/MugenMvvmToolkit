using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Tests.Validation
{
    public class TestValidationHandlerComponent : IValidationHandlerComponent, IHasPriority
    {
        public Func<IValidator, string?, CancellationToken, IReadOnlyMetadataContext?, Task>? TryValidateAsync { get; set; }

        public Func<IValidator, string?, IReadOnlyMetadataContext?, Task>? WaitAsync { get; set; }

        public int Priority { get; set; }

        Task IValidationHandlerComponent.TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            TryValidateAsync?.Invoke(validator, member, cancellationToken, metadata) ?? Task.CompletedTask;

        Task IValidationHandlerComponent.WaitAsync(IValidator validator, string? member, IReadOnlyMetadataContext? metadata) =>
            WaitAsync?.Invoke(validator, member, metadata) ?? Task.CompletedTask;
    }
}