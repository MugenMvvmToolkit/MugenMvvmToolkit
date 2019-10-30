using System.Runtime.InteropServices;
using System.Text;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Parsing.Visitors
{
    public sealed class BindingMemberExpressionVisitor : IExpressionVisitor
    {
        #region Fields

        private readonly StringBuilder _memberBuilder;
        private readonly IMemberProvider? _memberProvider;
        private readonly MemberDictionary _members;
        private readonly IObserverProvider? _observerProvider;
        private readonly IResourceResolver? _resourceResolver;

        #endregion

        #region Constructors

        public BindingMemberExpressionVisitor(IObserverProvider? observerProvider = null, IResourceResolver? resourceResolver = null, IMemberProvider? memberProvider = null)
        {
            _observerProvider = observerProvider;
            _resourceResolver = resourceResolver;
            _memberProvider = memberProvider;
            _members = new MemberDictionary();
            _memberBuilder = new StringBuilder();
        }

        #endregion

        #region Properties

        public bool IsPostOrder => false;

        public BindingMemberFlags MemberFlags { get; set; } = BindingMemberFlags.All & ~BindingMemberFlags.StaticNonPublic;

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
            {
                _memberBuilder.Clear();
                member = GetOrAddBindingParameter(methodCall.MethodName);
            }
            else
            {
                member = GetOrAddBindingParameter(methodCall.Target, methodCall.MethodName);
                if (member == null)
                    return methodCall;
            }

            return methodCall.UpdateTarget(member).Accept(this);
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
            if (target.TryBuildBindingMember(_memberBuilder, out var firstExpression))
                return GetOrAddBindingParameter(methodName);

            if (firstExpression is UnaryExpressionNode unaryExpression && unaryExpression.IsMacros() &&
                unaryExpression.Operand is IMemberExpressionNode memberExpression)
            {
                //$target, $self, $this
                if (memberExpression.MemberName == MacrosConstants.Target || memberExpression.MemberName == MacrosConstants.Self || memberExpression.MemberName == MacrosConstants.This)
                    return GetOrAddBindingParameter(methodName, BindingMemberExpressionFlags.TargetOnly);

                //$source
                if (memberExpression.MemberName == MacrosConstants.Source)
                    return GetOrAddBindingParameter(methodName, BindingMemberExpressionFlags.SourceOnly);

                //$context
                if (memberExpression.MemberName == MacrosConstants.Context)
                    return GetOrAddBindingParameter(methodName, BindingMemberExpressionFlags.ContextOnly);

                //type -> $string, $int, etc
                var type = _resourceResolver.ServiceIfNull().TryGetType(memberExpression.MemberName);
                if (type != null)
                {
                    if (unaryExpression.Token == UnaryTokenType.StaticExpression)
                    {
                        var value = _observerProvider.ServiceIfNull().GetMemberPath(_memberBuilder.GetPath()).GetValueFromPath(type, null, MemberFlags | BindingMemberFlags.Static, memberProvider: _memberProvider);
                        return ConstantExpressionNode.Get(value);
                    }

                    return GetOrAddBindingParameter(methodName, 0, true, type);
                }

                //resource -> $i18n, $color, etc
                if (unaryExpression.Token == UnaryTokenType.StaticExpression)
                {
                    var resourceValue = _resourceResolver.ServiceIfNull().TryGetResourceValue(memberExpression.MemberName);
                    if (resourceValue == null)
                        BindingExceptionManager.ThrowCannotResolveResource(memberExpression.MemberName);
                    if (resourceValue.Value == null)
                        return ConstantExpressionNode.Null;

                    var value = _observerProvider.ServiceIfNull().GetMemberPath(_memberBuilder.GetPath()).GetValueFromPath(resourceValue.Value.GetType(), resourceValue.Value, MemberFlags & ~BindingMemberFlags.Static,
                        memberProvider: _memberProvider);
                    return ConstantExpressionNode.Get(value);
                }

                _memberBuilder.Insert(0, nameof(IResourceValue.Value));
                return GetOrAddBindingParameter(methodName, (BindingMemberExpressionFlags)(1 << 7), false, memberExpression.MemberName);
            }

            return null;
        }

        private IExpressionNode GetOrAddBindingParameter(string? methodName = null, BindingMemberExpressionFlags flags = 0, bool isStatic = false, object? target = null)
        {
            flags |= BindingMemberExpressionFlags.Observable;
            var memberFlags = isStatic ? (MemberFlags | BindingMemberFlags.Static) & ~BindingMemberFlags.Instance : (MemberFlags | BindingMemberFlags.Instance) & ~BindingMemberFlags.Static;
            var key = new CacheKey(_memberBuilder.GetPath(), methodName, memberFlags, target, flags);
            if (!_members.TryGetValue(key, out var node))
            {
                node = new BindingMemberExpressionNode(key.Path, _observerProvider, _resourceResolver)
                {
                    Target = target,
                    ObservableMethodName = methodName,
                    Flags = flags,
                    MemberFlags = memberFlags
                };

                _members[key] = node;
            }

            return node;
        }

        #endregion

        #region Nested types

        private sealed class MemberDictionary : LightDictionary<CacheKey, BindingMemberExpressionNode>
        {
            #region Constructors

            public MemberDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.Path == y.Path && x.MethodName == y.MethodName && x.MemberFlags == y.MemberFlags && x.Flags == y.Flags && Equals(x.Target, y.Target);
            }

            protected override int GetHashCode(CacheKey key)
            {
                unchecked
                {
                    var hashCode = key.Path.GetHashCode();
                    hashCode = hashCode * 397 ^ (key.MethodName != null ? key.MethodName.GetHashCode() : 0);
                    hashCode = hashCode * 397 ^ (int)key.MemberFlags;
                    hashCode = hashCode * 397 ^ (int)key.Flags;
                    hashCode = hashCode * 397 ^ (key.Target != null ? key.Target.GetHashCode() : 0);
                    return hashCode;
                }
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct CacheKey
        {
            #region Fields

            public readonly string Path;
            public readonly string? MethodName;
            public readonly BindingMemberFlags MemberFlags;
            public readonly BindingMemberExpressionFlags Flags;
            public readonly object? Target;

            #endregion

            #region Constructors

            public CacheKey(string path, string? methodName, BindingMemberFlags memberFlags, object? target, BindingMemberExpressionFlags flags)
            {
                Path = path;
                MethodName = methodName;
                MemberFlags = memberFlags;
                Flags = flags;
                Target = target;
            }

            #endregion
        }

        #endregion
    }
}