using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

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

        public IndexExpressionNode(PropertyInfo indexer, IExpressionNode? target, IReadOnlyList<IExpressionNode> arguments)
            : this(target, arguments)
        {
            Should.NotBeNull(indexer, nameof(indexer));
            Indexer = indexer;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Index;

        public PropertyInfo? Indexer { get; private set; }

        public IExpressionNode? Target { get; }

        public IReadOnlyList<IExpressionNode> Arguments { get; }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
        {
            var changed = false;
            IExpressionNode? target = null;
            if (Target != null)
                target = VisitWithCheck(visitor, Target, false, ref changed);
            var itemsChanged = false;
            IExpressionNode[]? newArgs = null;
            for (var i = 0; i < Arguments.Count; i++)
            {
                var node = VisitWithCheck(visitor, Arguments[i], true, ref itemsChanged);
                if (itemsChanged)
                    newArgs = Arguments.ToArray();
                if (newArgs != null)
                    newArgs[i] = node;
            }

            if (changed || itemsChanged)
                return new IndexExpressionNode(target!, newArgs ?? Arguments) { Indexer = Indexer };
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