using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class MethodCallExpressionNode : ExpressionNodeBase<IMethodCallExpressionNode>, IMethodCallExpressionNode
    {
        #region Constructors

        public MethodCallExpressionNode(IExpressionNode? target, string method,
            IReadOnlyList<IExpressionNode> arguments, IReadOnlyList<string>? typeArgs = null, IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Should.NotBeNull(method, nameof(method));
            Should.NotBeNull(arguments, nameof(arguments));
            Target = target;
            Method = method;
            Arguments = arguments;
            TypeArgs = typeArgs ?? Default.Array<string>();
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
            return Equals(arguments, Arguments, null) ? this : new MethodCallExpressionNode(Target, Method, arguments, TypeArgs, Metadata);
        }

        public IMethodCallExpressionNode UpdateTarget(IExpressionNode? target) => Equals(target, Target) ? this : new MethodCallExpressionNode(target, Method, Arguments, TypeArgs, Metadata);

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
                return new MethodCallExpressionNode(target, Method, newArgs, TypeArgs, Metadata);
            return this;
        }

        protected override IMethodCallExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => new MethodCallExpressionNode(Target, Method, Arguments, TypeArgs, metadata);

        protected override bool Equals(IMethodCallExpressionNode other, IExpressionEqualityComparer? comparer) =>
            Method.Equals(other.Method) && TypeArgsEquals(other.TypeArgs) && Equals(Target, other.Target, comparer) && Equals(Arguments, other.Arguments, comparer);

        protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer)
        {
            if (Target == null)
                return HashCode.Combine(hashCode, Method, Arguments.Count, TypeArgs.Count);
            return HashCode.Combine(hashCode, Method, Arguments.Count, TypeArgs.Count, Target.GetHashCode(comparer));
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

        private bool TypeArgsEquals(IReadOnlyList<string> otherTypeArgs)
        {
            if (ReferenceEquals(TypeArgs, otherTypeArgs))
                return true;
            if (TypeArgs.Count != otherTypeArgs.Count)
                return false;
            for (int i = 0; i < TypeArgs.Count; i++)
            {
                if (!TypeArgs[i].Equals(otherTypeArgs[i]))
                    return false;
            }

            return true;
        }

        #endregion
    }
}