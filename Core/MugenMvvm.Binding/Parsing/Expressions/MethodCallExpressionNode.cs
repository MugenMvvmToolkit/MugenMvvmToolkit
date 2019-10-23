using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class MethodCallExpressionNode : ExpressionNodeBase, IMethodCallExpressionNode
    {
        #region Constructors

        public MethodCallExpressionNode(IExpressionNode? target, IBindingMethodInfo method,
            IReadOnlyList<IExpressionNode> arguments, IReadOnlyList<string>? typeArgs = null)
            : this(target, method?.Name!, arguments, typeArgs)
        {
            Method = method;
        }

        public MethodCallExpressionNode(IExpressionNode? target, string method,
            IReadOnlyList<IExpressionNode> arguments, IReadOnlyList<string>? typeArgs = null)
        {
            Should.NotBeNull(method, nameof(method));
            Should.NotBeNull(arguments, nameof(arguments));
            Target = target;
            MethodName = method;
            Arguments = arguments;
            TypeArgs = typeArgs ?? Default.EmptyArray<string>();
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.MethodCall;

        public IBindingMethodInfo? Method { get; private set; }

        public string MethodName { get; }

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

            if (Method == null)
                return new MethodCallExpressionNode(Target, MethodName, arguments, TypeArgs);
            return new MethodCallExpressionNode(Target, Method, arguments, TypeArgs);
        }

        public IMethodCallExpressionNode UpdateTarget(IExpressionNode? target)
        {
            if (ReferenceEquals(target, Target))
                return this;

            if (Method == null)
                return new MethodCallExpressionNode(target, MethodName, Arguments, TypeArgs);
            return new MethodCallExpressionNode(target, Method, Arguments, TypeArgs);
        }

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
                return new MethodCallExpressionNode(target, MethodName, newArgs ?? Arguments, TypeArgs) {Method = Method};
            return this;
        }

        public override string ToString()
        {
            string? typeArgs = null;
            if (TypeArgs.Count != 0)
                typeArgs = $"<{string.Join(", ", TypeArgs)}>";
            var join = string.Join(",", Arguments);
            if (Target == null)
                return $"{MethodName}{typeArgs}({join})";
            return $"{Target}.{MethodName}{typeArgs}({join})";
        }

        #endregion
    }
}