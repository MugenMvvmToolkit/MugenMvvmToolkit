using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;

namespace MugenMvvm.UnitTest.Validation.Internal
{
    public class TestValidator : ComponentOwnerBase<IValidator>, IValidator, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        public TestValidator(IReadOnlyMetadataContext? metadata = null, IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null) : base(
            componentCollectionProvider)
        {
            _metadata = metadata;
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadataContextProvider.LazyInitializeNonReadonly(ref _metadata, this);

        bool IValidator.HasErrors => HasErrors?.Invoke() ?? false;

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

        IReadOnlyList<object> IValidator.GetErrors(string? memberName, IReadOnlyMetadataContext? metadata)
        {
            return GetErrors?.Invoke(memberName, metadata) ?? Default.EmptyArray<object>();
        }

        IReadOnlyDictionary<string, IReadOnlyList<object>> IValidator.GetErrors(IReadOnlyMetadataContext? metadata)
        {
            return GetAllErrors?.Invoke(metadata) ?? Default.ReadOnlyDictionary<string, IReadOnlyList<object>>();
        }

        Task IValidator.ValidateAsync(string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return ValidateAsync?.Invoke(memberName, cancellationToken, metadata) ?? Task.CompletedTask;
        }

        void IValidator.ClearErrors(string? memberName, IReadOnlyMetadataContext? metadata)
        {
            ClearErrors?.Invoke(memberName, metadata);
        }

        #endregion
    }
}