using System.Collections.Generic;
using MugenMvvm.Collections;

namespace MugenMvvm.Bindings.Parsing
{
    public class BindingExpressionRequest
    {
        public BindingExpressionRequest(object target, object? source, ItemOrIReadOnlyList<KeyValuePair<string?, object>> parameters)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            Source = source;
            Parameters = parameters;
        }

        public ItemOrIReadOnlyList<KeyValuePair<string?, object>> Parameters { get; protected set; }

        public object? Source { get; protected set; }

        public object Target { get; protected set; }
    }
}