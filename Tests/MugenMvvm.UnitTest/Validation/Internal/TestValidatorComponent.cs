using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Validation.Internal
{
    public class TestValidatorComponent : IValidatorComponent, IDisposable, IHasPriority
    {
        #region Properties

        bool IValidatorComponent.HasErrors => HasErrors?.Invoke() ?? false;

        public Func<bool>? HasErrors { get; set; }

        public Action? Dispose { get; set; }

        public Func<string?, IReadOnlyMetadataContext?, ItemOrList<object, IReadOnlyList<object>>>? GetErrors { get; set; }

        public Func<IReadOnlyMetadataContext?, IReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>>>? GetAllErrors { get; set; }

        public Func<string?, CancellationToken, IReadOnlyMetadataContext?, Task>? ValidateAsync { get; set; }

        public Action<string?, IReadOnlyMetadataContext?>? ClearErrors { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IDisposable.Dispose()
        {
            Dispose?.Invoke();
        }

        ItemOrList<object, IReadOnlyList<object>> IValidatorComponent.TryGetErrors(string? memberName, IReadOnlyMetadataContext? metadata)
        {
            return GetErrors?.Invoke(memberName, metadata) ?? default;
        }

        IReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>> IValidatorComponent.TryGetErrors(IReadOnlyMetadataContext? metadata)
        {
            return GetAllErrors?.Invoke(metadata) ?? Default.ReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>>();
        }

        Task? IValidatorComponent.TryValidateAsync(string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return ValidateAsync?.Invoke(memberName, cancellationToken, metadata) ?? Task.CompletedTask;
        }

        void IValidatorComponent.ClearErrors(string? memberName, IReadOnlyMetadataContext? metadata)
        {
            ClearErrors?.Invoke(memberName, metadata);
        }

        #endregion
    }
}