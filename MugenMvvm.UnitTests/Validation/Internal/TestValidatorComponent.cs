using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Validation.Internal
{
    public class TestValidatorComponent : IValidatorComponent, IDisposable, IHasPriority
    {
        #region Properties

        public Func<IValidator, string?, IReadOnlyMetadataContext?, bool>? HasErrors { get; set; }

        public Action? Dispose { get; set; }

        public Func<IValidator, string?, IReadOnlyMetadataContext?, ItemOrList<object, IReadOnlyList<object>>>? GetErrors { get; set; }

        public Func<IValidator, IReadOnlyMetadataContext?, IReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>>>? GetAllErrors { get; set; }

        public Func<IValidator, string?, CancellationToken, IReadOnlyMetadataContext?, Task>? ValidateAsync { get; set; }

        public Action<IValidator, string?, IReadOnlyMetadataContext?>? ClearErrors { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IDisposable.Dispose() => Dispose?.Invoke();

        bool IValidatorComponent.HasErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata) => HasErrors?.Invoke(validator, memberName, metadata) ?? false;

        ItemOrList<object, IReadOnlyList<object>> IValidatorComponent.TryGetErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata) =>
            GetErrors?.Invoke(validator, memberName, metadata) ?? default;

        IReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>> IValidatorComponent.TryGetErrors(IValidator validator, IReadOnlyMetadataContext? metadata) =>
            GetAllErrors?.Invoke(validator, metadata) ?? Default.ReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>>();

        Task? IValidatorComponent.TryValidateAsync(IValidator validator, string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            ValidateAsync?.Invoke(validator, memberName, cancellationToken, metadata) ?? Task.CompletedTask;

        void IValidatorComponent.ClearErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata) => ClearErrors?.Invoke(validator, memberName, metadata);

        #endregion
    }
}