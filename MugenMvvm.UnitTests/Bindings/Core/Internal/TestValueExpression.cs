using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestValueExpression : IValueExpression
    {
        #region Properties

        public Func<IReadOnlyMetadataContext?, object?>? Invoke { get; set; }

        #endregion

        #region Implementation of interfaces

        object? IValueExpression.Invoke(IReadOnlyMetadataContext? metadata) => Invoke?.Invoke(metadata);

        #endregion
    }
}