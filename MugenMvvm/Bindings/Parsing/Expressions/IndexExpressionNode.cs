using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class IndexExpressionNode : ExpressionNodeBase<IIndexExpressionNode>, IIndexExpressionNode
    {
        #region Constructors

        public IndexExpressionNode(IExpressionNode? target, IReadOnlyList<IExpressionNode> arguments, IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Should.NotBeNull(arguments, nameof(arguments));
            Target = target;
            Arguments = arguments;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Index;

        public IExpressionNode? Target { get; }

        public IReadOnlyList<IExpressionNode> Arguments { get; }

        #endregion

        #region Implementation of interfaces

        public IIndexExpressionNode UpdateArguments(IReadOnlyList<IExpressionNode> arguments)
        {
            Should.NotBeNull(arguments, nameof(arguments));
            return Equals(Arguments, arguments, null) ? this : new IndexExpressionNode(Target, arguments, Metadata);
        }

        public IIndexExpressionNode UpdateTarget(IExpressionNode? target) => Equals(target, Target) ? this : new IndexExpressionNode(target, Arguments, Metadata);

        #endregion

        #region Methods

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

        protected override bool Equals(IIndexExpressionNode other, IExpressionEqualityComparer? comparer) => Equals(Target, other.Target, comparer) && Equals(Arguments, other.Arguments, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => GetHashCode(hashCode, Target, Arguments, comparer);

        public override string ToString()
        {
            var join = string.Join(", ", Arguments);
            return $"{Target}[{join}]";
        }

        #endregion
    }
}