using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct BindingExpressionRequest
    {
        #region Fields

        public readonly ItemOrList<KeyValuePair<string?, object>, IReadOnlyList<KeyValuePair<string?, object>>> Parameters;
        public readonly object? Source;
        public readonly object Target;

        #endregion

        #region Constructors

        public BindingExpressionRequest(object target, object? source, ItemOrList<KeyValuePair<string?, object>, IReadOnlyList<KeyValuePair<string?, object>>> parameters)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            Source = source;
            Parameters = parameters;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Target == null;

        #endregion
    }
}