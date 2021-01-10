using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Parsing
{
    public class BindingExpressionRequest
    {
        #region Constructors

        public BindingExpressionRequest(object target, object? source, ItemOrIReadOnlyList<KeyValuePair<string?, object>> parameters)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            Source = source;
            Parameters = parameters;
        }

        #endregion

        #region Properties

        public ItemOrIReadOnlyList<KeyValuePair<string?, object>> Parameters { get; protected set; }

        public object? Source { get; protected set; }

        public object Target { get; protected set; }

        #endregion
    }
}