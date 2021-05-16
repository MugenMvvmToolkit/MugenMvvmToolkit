﻿using System;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    public class TestExpressionEqualityComparer : IExpressionEqualityComparer
    {
        public new Func<IExpressionNode, IExpressionNode, bool?>? Equals { get; set; }

        public new Func<IExpressionNode, int?>? GetHashCode { get; set; }

        bool? IExpressionEqualityComparer.Equals(IExpressionNode x, IExpressionNode y) => Equals?.Invoke(x, y);

        int? IExpressionEqualityComparer.GetHashCode(IExpressionNode expression) => GetHashCode?.Invoke(expression);
    }
}