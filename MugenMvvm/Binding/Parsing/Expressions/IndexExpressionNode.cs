using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class IndexExpressionNode : ExpressionNodeBase, IIndexExpressionNode
    {
        #region Constructors

        public IndexExpressionNode(IExpressionNode? target, IReadOnlyList<IExpressionNode> arguments)
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
            if (ReferenceEquals(arguments, Arguments))
                return this;
            return new IndexExpressionNode(Target, arguments);
        }

        public IIndexExpressionNode UpdateTarget(IExpressionNode? target)
        {
            return target == Target ? this : new IndexExpressionNode(target, Arguments);
        }

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
                return new IndexExpressionNode(target, newArgs);
            return this;
        }

        public override string ToString()
        {
            var join = string.Join(", ", Arguments);
            return $"{Target}[{join}]";
        }

        #endregion
    }
}