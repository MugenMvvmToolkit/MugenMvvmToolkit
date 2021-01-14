using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class ParameterExpressionNode : ExpressionNodeBase<IParameterExpressionNode>, IParameterExpressionNode
    {
        public ParameterExpressionNode(string name, IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Should.NotBeNull(name, nameof(name));
            Name = name;
        }

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Parameter;

        public string Name { get; }

        public override string ToString() => Name;

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata) => this;

        protected override IParameterExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new ParameterExpressionNode(Name, metadata);

        protected override bool Equals(IParameterExpressionNode other, IExpressionEqualityComparer? comparer) => Name.Equals(other.Name);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => HashCode.Combine(hashCode, Name);
    }
}