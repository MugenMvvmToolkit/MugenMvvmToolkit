using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestValueExpression : IValueExpression
    {
        public Func<IReadOnlyMetadataContext?, object?>? Invoke { get; set; }

        object? IValueExpression.Invoke(IReadOnlyMetadataContext? metadata) => Invoke?.Invoke(metadata);
    }
}