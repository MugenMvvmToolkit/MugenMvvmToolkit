using System.Text;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Parsing.Visitors
{
    public sealed class BindingMemberExpressionVisitor : IExpressionVisitor //todo relativesource visitor, resource visitor
    {
        #region Fields

        private readonly StringBuilder _memberNameBuilder;
        private readonly MemberDictionary _members;
        private readonly IObserverProvider? _observerProvider;

        #endregion

        #region Constructors

        public BindingMemberExpressionVisitor(IObserverProvider? observerProvider = null)
        {
            _observerProvider = observerProvider;
            _members = new MemberDictionary();
            _memberNameBuilder = new StringBuilder();
        }

        #endregion

        #region Properties

        public bool IsPostOrder => false;

        public MemberFlags MemberFlags { get; set; } = MemberFlags.All & ~MemberFlags.StaticNonPublic;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? Visit(IExpressionNode node)
        {
            if (node is IMethodCallExpressionNode methodCall)
                return VisitMethodCall(methodCall);

            if (node is IMemberExpressionNode memberExpressionNode)
                return VisitMemberExpression(memberExpressionNode);

            if (node is IIndexExpressionNode indexExpression)
                return VisitIndexExpression(indexExpression);

            if (node is IUnaryExpressionNode unaryExpression && unaryExpression.IsMacros())
                return GetOrAddBindingParameter(node, null) ?? node;

            return node;
        }

        #endregion

        #region Methods

        public void Clear()
        {
            _members.Clear();
        }

        private IExpressionNode VisitMethodCall(IMethodCallExpressionNode methodCall)
        {
            var member = GetOrAddBindingParameter(methodCall, null);
            if (member != null)
                return member;

            if (methodCall.Target == null)
                member = GetOrAddBindingParameter(string.Empty, 0, methodCall.MethodName);
            else
            {
                member = GetOrAddBindingParameter(methodCall.Target, methodCall.MethodName);
                if (member == null)
                    return methodCall;
            }

            if (methodCall.Method == null)
                return new MethodCallExpressionNode(member, methodCall.MethodName, methodCall.Arguments, methodCall.TypeArgs).Accept(this);
            return new MethodCallExpressionNode(member, methodCall.Method, methodCall.Arguments, methodCall.TypeArgs).Accept(this);
        }

        private IExpressionNode VisitMemberExpression(IMemberExpressionNode memberExpression)
        {
            return GetOrAddBindingParameter(memberExpression, null) ?? memberExpression;
        }

        private IExpressionNode VisitIndexExpression(IIndexExpressionNode indexExpression)
        {
            return GetOrAddBindingParameter(indexExpression, null) ?? indexExpression;
        }

        private IExpressionNode? GetOrAddBindingParameter(IExpressionNode target, string? methodName)
        {
            if (target.TryBuildBindingMember(_memberNameBuilder, out var firstExpression))
                return GetOrAddBindingParameter(_memberNameBuilder.ToString(), 0, methodName);

            if (firstExpression is UnaryExpressionNode unaryExpression && unaryExpression.IsMacros() &&
                unaryExpression.Operand is IMemberExpressionNode memberExpression)
            {
                //$target, $self, $this
                if (memberExpression.MemberName == "target" || memberExpression.MemberName == "self" || memberExpression.MemberName == "this")
                    return GetOrAddBindingParameter(_memberNameBuilder.ToString(), BindingMemberExpressionFlags.TargetOnly, methodName);

                //$source
                if (memberExpression.MemberName == "source")
                    return GetOrAddBindingParameter(_memberNameBuilder.ToString(), BindingMemberExpressionFlags.SourceOnly, methodName);

                //$context
                if (memberExpression.MemberName == "context")
                    return GetOrAddBindingParameter(_memberNameBuilder.ToString(), BindingMemberExpressionFlags.ContextOnly, methodName);
            }

            return null;
        }

        private IExpressionNode GetOrAddBindingParameter(string path, BindingMemberExpressionFlags flags, string? methodName)
        {
            var key = new CacheKey(path, flags);
            if (!_members.TryGetValue(key, out var node))
            {
                node = new BindingMemberExpression(path, MemberFlags & ~MemberFlags.Static, methodName, _observerProvider);
                node.Flags |= flags;
                _members[key] = node;
            }

            return node;
        }

        #endregion

        #region Nested types

        private sealed class MemberDictionary : LightDictionary<CacheKey, IBindingMemberExpression>
        {
            #region Constructors

            public MemberDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.Path == y.Path && x.Flags == y.Flags;
            }

            protected override int GetHashCode(CacheKey key)
            {
                unchecked
                {
                    return key.Path.GetHashCode() * 397 ^ ((short)key.Flags).GetHashCode();
                }
            }

            #endregion
        }

        private readonly struct CacheKey
        {
            #region Fields

            public readonly string Path;
            public readonly BindingMemberExpressionFlags Flags;

            #endregion

            #region Constructors

            public CacheKey(string path, BindingMemberExpressionFlags flags)
            {
                Path = path;
                Flags = flags;
            }

            #endregion
        }

        #endregion
    }
}