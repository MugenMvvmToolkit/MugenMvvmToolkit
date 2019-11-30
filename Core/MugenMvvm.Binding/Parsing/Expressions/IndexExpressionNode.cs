using System.Collections.Generic;
using System.Linq;
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
            if (ReferenceEquals(target, Target))
                return this;
            return new IndexExpressionNode(target, Arguments);
        }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            var changed = false;
            IExpressionNode? target = null;
            if (Target != null)
                target = VisitWithCheck(visitor, Target, false, ref changed, metadata);
            var itemsChanged = false;
            IExpressionNode[]? newArgs = null;
            for (var i = 0; i < Arguments.Count; i++)
            {
                var node = VisitWithCheck(visitor, Arguments[i], true, ref itemsChanged, metadata);
                if (itemsChanged)
                    newArgs = Arguments.ToArray();
                if (newArgs != null)
                    newArgs[i] = node;
            }

            if (changed || itemsChanged)
                return new IndexExpressionNode(target!, newArgs ?? Arguments);
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