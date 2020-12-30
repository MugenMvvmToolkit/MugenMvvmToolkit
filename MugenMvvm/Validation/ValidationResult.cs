using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Validation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ValidationResult
    {
        #region Fields

        public static readonly ValidationResult NoErrors = new(null, Default.ReadOnlyDictionary<string, object?>());

        public readonly object? RawErrors;
        public readonly string? SingleMemberName;
        public readonly IReadOnlyMetadataContext Metadata;

        #endregion

        #region Constructors

        private ValidationResult(string? member, object? errors, IReadOnlyMetadataContext? metadata = null)
        {
            RawErrors = errors;
            SingleMemberName = member;
            Metadata = metadata.DefaultIfNull();
        }

        #endregion

        #region Properties

        public bool HasResult => RawErrors != null;

        public ItemOrList<object, IReadOnlyList<object>> SingleMemberErrors
        {
            get
            {
                if (SingleMemberName == null)
                    return default;
                return ItemOrList.FromRawValue<object, IReadOnlyList<object>>(RawErrors);
            }
        }

        public IReadOnlyDictionary<string, object?>? Errors => RawErrors as IReadOnlyDictionary<string, object?>;

        #endregion

        #region Methods

        public static ValidationResult Get(IReadOnlyDictionary<string, object?> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(errors, nameof(errors));
            return new ValidationResult(null, errors, metadata);
        }

        public static ValidationResult Get(string member, ItemOrList<object, IReadOnlyList<object>> errors, IReadOnlyMetadataContext? metadata = null)
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

        #endregion
    }
}