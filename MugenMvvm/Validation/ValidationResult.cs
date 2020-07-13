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

        public static readonly ValidationResult NoErrors = new ValidationResult(null, Default.ReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>>());

        private readonly object? _errors;
        public readonly IReadOnlyMetadataContext Metadata;

        #endregion

        #region Constructors

        private ValidationResult(string? member, object? errors, IReadOnlyMetadataContext? metadata = null)
        {
            SingleMemberName = member;
            _errors = errors;
            Metadata = metadata.DefaultIfNull();
        }

        #endregion

        #region Properties

        public bool HasResult => _errors != null;

        public string? SingleMemberName { get; }

        public ItemOrList<object, IReadOnlyList<object>> SingleMemberErrors
        {
            get
            {
                if (SingleMemberName == null)
                    return default;
                return ItemOrList.FromRawValue<object, IReadOnlyList<object>>(_errors, true);
            }
        }

        public IReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>>? Errors => _errors as IReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>>;

        #endregion

        #region Methods

        public static ValidationResult FromErrors(IReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(errors, nameof(errors));
            return new ValidationResult(null, errors, metadata);
        }

        public static ValidationResult FromMemberErrors(string member, ItemOrList<object, object[]> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(member, nameof(member));
            return new ValidationResult(member, errors.GetRawValue(), metadata);
        }

        public IDictionary<string, ItemOrList<object, IReadOnlyList<object>>> GetErrorsNonReadOnly()
        {
            if (Errors == null)
                return new Dictionary<string, ItemOrList<object, IReadOnlyList<object>>>();
            if (Errors is IDictionary<string, ItemOrList<object, IReadOnlyList<object>>> errors && !errors.IsReadOnly)
                return errors;
            var result = new Dictionary<string, ItemOrList<object, IReadOnlyList<object>>>(Errors.Count);
            foreach (var pair in Errors)
                result[pair.Key] = pair.Value;
            return result;
        }

        #endregion
    }
}