using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
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

        public IndexExpressionNode(IExpressionNode? target, IMethodInfo indexer, IReadOnlyList<IExpressionNode> arguments)
            : this(target, arguments)
        {
            Should.NotBeNull(indexer, nameof(indexer));
            Indexer = indexer;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Index;

        public IMethodInfo? Indexer { get; private set; }

        public IExpressionNode? Target { get; }

        public IReadOnlyList<IExpressionNode> Arguments { get; }

        #endregion

        #region Implementation of interfaces

        public IIndexExpressionNode UpdateArguments(IReadOnlyList<IExpressionNode> arguments)
        {
            Should.NotBeNull(arguments, nameof(arguments));
            if (ReferenceEquals(arguments, Arguments))
                return this;

            if (Indexer == null)
                return new IndexExpressionNode(Target, arguments);
            return new IndexExpressionNode(Target, Indexer, arguments);
        }

        public IIndexExpressionNode UpdateTarget(IExpressionNode? target)
        {
            if (ReferenceEquals(target, Target))
                return this;

            if (Indexer == null)
                return new IndexExpressionNode(target, Arguments);
            return new IndexExpressionNode(target, Indexer, Arguments);
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