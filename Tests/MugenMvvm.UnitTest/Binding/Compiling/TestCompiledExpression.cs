using System;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Compiling
{
    public class TestCompiledExpression : ICompiledExpression
    {
        #region Properties

        public Func<ItemOrList<ExpressionValue, ExpressionValue[]>, IReadOnlyMetadataContext?, object?>? Invoke { get; set; }

        #endregion

        #region Implementation of interfaces

        object? ICompiledExpression.Invoke(ItemOrList<ExpressionValue, ExpressionValue[]> values, IReadOnlyMetadataContext? metadata)
        {
            return Invoke?.Invoke(values, metadata);
        }

        #endregion
    }
}