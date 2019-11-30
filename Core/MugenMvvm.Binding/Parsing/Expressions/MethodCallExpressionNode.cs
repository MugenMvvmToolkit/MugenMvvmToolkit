using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class MethodCallExpressionNode : ExpressionNodeBase, IMethodCallExpressionNode
    {
        #region Constructors

        public MethodCallExpressionNode(IExpressionNode? target, string method,
            IReadOnlyList<IExpressionNode> arguments, IReadOnlyList<string>? typeArgs = null)
        {
            Should.NotBeNull(method, nameof(method));
            Should.NotBeNull(arguments, nameof(arguments));
            Target = target;
            Method = method;
            Arguments = arguments;
            TypeArgs = typeArgs ?? Default.EmptyArray<string>();
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.MethodCall;

        public string Method { get; }

        public IReadOnlyList<string> TypeArgs { get; }

        public IExpressionNode? Target { get; }

        public IReadOnlyList<IExpressionNode> Arguments { get; }

        #endregion

        #region Implementation of interfaces

        public IMethodCallExpressionNode UpdateArguments(IReadOnlyList<IExpressionNode> arguments)
        {
            Should.NotBeNull(arguments, nameof(arguments));
            if (ReferenceEquals(arguments, Arguments))
                return this;
            return new MethodCallExpressionNode(Target, Method, arguments, TypeArgs);
        }

        public IMethodCallExpressionNode UpdateTarget(IExpressionNode? target)
        {
            if (ReferenceEquals(target, Target))
                return this;
            return new MethodCallExpressionNode(target, Method, Arguments, TypeArgs);
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
                return new MethodCallExpressionNode(target, Method, newArgs ?? Arguments, TypeArgs);
            return this;
        }

        public override string ToString()
        {
            string? typeArgs = null;
            if (TypeArgs.Count != 0)
                typeArgs = $"<{string.Join(", ", TypeArgs)}>";
            var join = string.Join(",", Arguments);
            if (Target == null)
                return $"{Method}{typeArgs}({join})";
            return $"{Target}.{Method}{typeArgs}({join})";
        }

        #endregion
    }
}