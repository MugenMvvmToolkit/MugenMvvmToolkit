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

        public Func<string?, IReadOnlyMetadataContext?, IReadOnlyList<object>>? GetErrors { get; set; }

        public Func<IReadOnlyMetadataContext?, IReadOnlyDictionary<string, IReadOnlyList<object>>>? GetAllErrors { get; set; }

        public Func<string?, CancellationToken, IReadOnlyMetadataContext?, Task>? ValidateAsync { get; set; }

        public Action<string?, IReadOnlyMetadataContext?>? ClearErrors { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IDisposable.Dispose()
        {
            Dispose?.Invoke();
        }

        IReadOnlyList<object> IValidatorComponent.GetErrors(string? memberName, IReadOnlyMetadataContext? metadata)
        {
            return GetErrors?.Invoke(memberName, metadata) ?? Default.Array<object>();
        }

        IReadOnlyDictionary<string, IReadOnlyList<object>> IValidatorComponent.GetErrors(IReadOnlyMetadataContext? metadata)
        {
            return GetAllErrors?.Invoke(metadata) ?? Default.ReadOnlyDictionary<string, IReadOnlyList<object>>();
        }

        Task IValidatorComponent.ValidateAsync(string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
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