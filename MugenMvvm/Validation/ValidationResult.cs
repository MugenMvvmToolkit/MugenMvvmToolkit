using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Validation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ValidationResult
    {
        public static readonly ValidationResult NoErrors = new(null, Default.ReadOnlyDictionary<string, object?>());

        public readonly object? RawErrors;
        public readonly string? SingleMemberName;
        public readonly IReadOnlyMetadataContext Metadata;

        private ValidationResult(string? member, object? errors, IReadOnlyMetadataContext? metadata = null)
        {
            RawErrors = errors;
            SingleMemberName = member;
            Metadata = metadata.DefaultIfNull();
        }

        public bool HasResult => RawErrors != null;

        public ItemOrIReadOnlyList<object> SingleMemberErrors
        {
            get
            {
                if (SingleMemberName == null)
                    return default;
                return ItemOrIReadOnlyList.FromRawValue<object>(RawErrors);
            }
        }

        public IReadOnlyDictionary<string, object?>? Errors => RawErrors as IReadOnlyDictionary<string, object?>;

        public static ValidationResult Get(IReadOnlyDictionary<string, object?> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(errors, nameof(errors));
            return new ValidationResult(null, errors, metadata);
        }

        public static ValidationResult Get(string member, ItemOrIReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(member, nameof(member));
            return new ValidationResult(member, errors.GetRawValue(), metadata);
        }

        public IDictionary<string, object?> GetErrors()
        {
            if (Errors == null)
                return new Dictionary<string, object?>();
            if (Errors is IDictionary<string, object?> errors && !errors.IsReadOnly)
                return errors;
            var result = new Dictionary<string, object?>(Errors.Count);
            foreach (var pair in Errors)
                result[pair.Key] = pair.Value;
            return result;
        }
    }
}