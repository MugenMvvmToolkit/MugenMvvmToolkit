using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Validation;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidator : IComponentOwner<IValidator>, IMetadataOwner<IMetadataContext>, IHasDisposeState
    {
        Task ValidateAsync(string? member = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        bool HasErrors(ItemOrIReadOnlyList<string> members = default, object? source = null, IReadOnlyMetadataContext? metadata = null);

        void GetErrors(ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> errors, object? source = null, IReadOnlyMetadataContext? metadata = null);

        void GetErrors(ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source = null, IReadOnlyMetadataContext? metadata = null);

        void SetErrors(object source, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata = null);

        void ClearErrors(ItemOrIReadOnlyList<string> members = default, object? source = null, IReadOnlyMetadataContext? metadata = null);
    }
}