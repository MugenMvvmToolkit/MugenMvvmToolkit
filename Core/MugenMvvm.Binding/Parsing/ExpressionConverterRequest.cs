using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ExpressionConverterRequest
    {
        #region Fields

        public readonly ItemOrList<KeyValuePair<string, object>, IReadOnlyList<KeyValuePair<string, object>>> Parameters;
        public readonly Expression? Source;
        public readonly Expression Target;

        #endregion

        #region Constructors

        public ExpressionConverterRequest(Expression target, Expression? source, ItemOrList<KeyValuePair<string, object>, IReadOnlyList<KeyValuePair<string, object>>> parameters)
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