using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IExpressionNode
    {
        ExpressionNodeType ExpressionType { get; }

        bool HasMetadata { get; }
        
        IDictionary<string, object?> Metadata { get; }

        IExpressionNode Accept(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata = null);
    }
}