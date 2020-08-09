using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
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