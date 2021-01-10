using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IExpressionNode : IEquatable<IExpressionNode>//todo itemorlist
    {
        ExpressionNodeType ExpressionType { get; }

        IReadOnlyDictionary<string, object?> Metadata { get; }

        IExpressionNode Accept(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata = null);

        IExpressionNode UpdateMetadata(IReadOnlyDictionary<string, object?>? metadata);

        bool Equals(IExpressionNode? other, IExpressionEqualityComparer? comparer);

        int GetHashCode(IExpressionEqualityComparer? comparer);
    }
}