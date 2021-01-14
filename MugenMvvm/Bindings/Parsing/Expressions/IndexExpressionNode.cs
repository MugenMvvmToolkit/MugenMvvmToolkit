using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class IndexExpressionNode : ExpressionNodeBase<IIndexExpressionNode>, IIndexExpressionNode
    {
        private readonly object? _arguments;

        public IndexExpressionNode(IExpressionNode? target, ItemOrIReadOnlyList<IExpressionNode> arguments, IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Target = target;
            _arguments = arguments.GetRawValue();
        }

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Index;

        public ItemOrIReadOnlyList<IExpressionNode> Arguments => ItemOrIReadOnlyList.FromRawValue<IExpressionNode>(_arguments);

        public IExpressionNode? Target { get; }

        public override string ToString()
        {
            var join = string.Join(", ", Arguments.AsList());
            return $"{Target}[{join}]";
        }

        public IIndexExpressionNode UpdateArguments(ItemOrIReadOnlyList<IExpressionNode> arguments) =>
            Equals(Arguments, arguments, null) ? this : new IndexExpressionNode(Target, arguments, Metadata);

        public IIndexExpressionNode UpdateTarget(IExpressionNode? target) => Equals(target, Target) ? this : new IndexExpressionNode(target, Arguments, Metadata);

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            var changed = false;
            IExpressionNode? target = null;
            if (Target != null)
                target = VisitWithCheck(visitor, Target, false, ref changed, metadata);
            var newArgs = VisitWithCheck(visitor, Arguments, ref changed, metadata);
            if (changed)
                return new IndexExpressionNode(target, newArgs, Metadata);
            return this;
        }

        protected override IIndexExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new IndexExpressionNode(Target, Arguments, metadata);

        protected override bool Equals(IIndexExpressionNode other, IExpressionEqualityComparer? comparer) =>
            Equals(Target, other.Target, comparer) && Equals(Arguments, other.Arguments, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => GetHashCode(hashCode, Target, Arguments, comparer);
    }
}