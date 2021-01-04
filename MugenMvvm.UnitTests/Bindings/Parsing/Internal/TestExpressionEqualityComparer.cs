using System;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    public class TestExpressionEqualityComparer : IExpressionEqualityComparer
    {
        #region Properties

        public new Func<IExpressionNode, IExpressionNode, bool?>? Equals { get; set; }

        public new Func<IExpressionNode, int?>? GetHashCode { get; set; }

        #endregion

        #region Implementation of interfaces

        bool? IExpressionEqualityComparer.Equals(IExpressionNode x, IExpressionNode y) => Equals?.Invoke(x, y);

        int? IExpressionEqualityComparer.GetHashCode(IExpressionNode expression) => GetHashCode?.Invoke(expression);

        #endregion
    }
}