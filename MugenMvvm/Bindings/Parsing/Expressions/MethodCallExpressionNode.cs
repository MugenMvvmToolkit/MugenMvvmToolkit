using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Expressions
{
    public sealed class MethodCallExpressionNode : ExpressionNodeBase<IMethodCallExpressionNode>, IMethodCallExpressionNode
    {
        #region Fields

        private readonly object? _arguments;
        private readonly object? _typeArgs;

        #endregion

        #region Constructors

        public MethodCallExpressionNode(IExpressionNode? target, string method,
            ItemOrIReadOnlyList<IExpressionNode> arguments, ItemOrIReadOnlyList<string> typeArgs = default, IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
        {
            Should.NotBeNull(method, nameof(method));
            Target = target;
            Method = method;
            _arguments = arguments.GetRawValue();
            _typeArgs = typeArgs.GetRawValue();
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.MethodCall;

        public string Method { get; }

        public ItemOrIReadOnlyList<string> TypeArgs => ItemOrIReadOnlyList.FromRawValue<string>(_typeArgs);

        public IExpressionNode? Target { get; }

        public ItemOrIReadOnlyList<IExpressionNode> Arguments => ItemOrIReadOnlyList.FromRawValue<IExpressionNode>(_arguments);

        #endregion

        #region Implementation of interfaces

        public IMethodCallExpressionNode UpdateArguments(ItemOrIReadOnlyList<IExpressionNode> arguments) =>
            Equals(arguments, Arguments, null) ? this : new MethodCallExpressionNode(Target, Method, arguments, TypeArgs, Metadata);

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
                typeArgs = $"<{string.Join(", ", TypeArgs.AsList())}>";
            var join = string.Join(",", Arguments.AsList());
            if (Target == null)
                return $"{Method}{typeArgs}({join})";
            return $"{Target}.{Method}{typeArgs}({join})";
        }

        private bool TypeArgsEquals(ItemOrIReadOnlyList<string> otherTypeArgs)
        {
            if (ReferenceEquals(TypeArgs.Item, otherTypeArgs.Item) && ReferenceEquals(TypeArgs.List, otherTypeArgs.List))
                return true;
            var count = TypeArgs.Count;
            if (count != otherTypeArgs.Count)
                return false;
            for (var i = 0; i < count; i++)
            {
                if (!TypeArgs[i].Equals(otherTypeArgs[i]))
                    return false;
            }

            return true;
        }

        #endregion
    }
}