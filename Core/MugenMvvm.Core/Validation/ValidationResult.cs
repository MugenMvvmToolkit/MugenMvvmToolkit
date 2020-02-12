using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Validation
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ValidationResult
    {
        #region Fields

        public static readonly ValidationResult NoErrors = new ValidationResult(Default.ReadOnlyDictionary<string, IReadOnlyList<object>?>());

        public readonly IReadOnlyDictionary<string, IReadOnlyList<object>?>? ErrorsRaw;
        public readonly IReadOnlyMetadataContext Metadata;

        #endregion

        #region Constructors

        public ValidationResult(IReadOnlyDictionary<string, IReadOnlyList<object>?> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(errors, nameof(errors));
            ErrorsRaw = errors;
            Metadata = metadata.DefaultIfNull();
        }

        #endregion

        #region Properties

        public bool HasResult => ErrorsRaw != null;

        #endregion

        #region Methods

        public IDictionary<string, IReadOnlyList<object>?> GetErrors()
        {
            if (ErrorsRaw == null)
                return new Dictionary<string, IReadOnlyList<object>?>();
            if (ErrorsRaw is IDictionary<string, IReadOnlyList<object>?> errors && !errors.IsReadOnly)
                return errors;
            var result = new Dictionary<string, IReadOnlyList<object>?>(ErrorsRaw.Count);
            foreach (var pair in ErrorsRaw)
                result[pair.Key] = pair.Value;
            return result;
        }

        #endregion
    }
}