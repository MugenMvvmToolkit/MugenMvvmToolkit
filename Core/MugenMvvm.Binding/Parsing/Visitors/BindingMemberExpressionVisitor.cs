using System;
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
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Visitors
{
    public sealed class BindingMemberExpressionVisitor : IExpressionVisitor
    {
        #region Fields

        private readonly Func<IExpressionNode, bool> _condition;

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
            _condition = Condition;
        }

        #endregion

        #region Properties

        public bool IsPostOrder => false;

        public MemberFlags MemberFlags { get; set; } = MemberFlags.All & ~MemberFlags.NonPublic;

        public BindingMemberExpressionFlags Flags { get; set; } = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethod;

        public bool IgnoreMethodMembers { get; set; }

        public bool IgnoreIndexMembers { get; set; }

        #endregion

        #region Implementation of interfaces

        IExpressionNode? IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is IMethodCallExpressionNode methodCall)
                return VisitMethodCall(methodCall, metadata);

            if (expression is IMemberExpressionNode memberExpressionNode)
                return VisitMemberExpression(memberExpressionNode);

            if (expression is IIndexExpressionNode indexExpression)
                return VisitIndexExpression(indexExpression);

            if (expression is IUnaryExpressionNode unaryExpression && unaryExpression.IsMacros())
                return GetOrAddBindingMember(expression, null) ?? expression;

            return expression;
        }

        #endregion

        #region Methods

        private bool Condition(IExpressionNode arg)
        {
            if (IgnoreIndexMembers && arg is IIndexExpressionNode)
                return false;

            if (IgnoreMethodMembers && arg is IMethodCallExpressionNode)
                return false;
            return true;
        }

        public IExpressionNode? Visit(IExpressionNode? expression, IReadOnlyMetadataContext? metadata = null)
        {
            if (expression == null)
                return null;
            _members.Clear();
            expression = expression.Accept(this, metadata);
            _members.Clear();
            return expression;
        }

        private IExpressionNode VisitMethodCall(IMethodCallExpressionNode methodCall, IReadOnlyMetadataContext? metadata)
        {
            var member = GetOrAddBindingMember(methodCall, null);
            if (member != null)
                return member;

            if (methodCall.Target == null)
            {
                _memberBuilder.Clear();
                member = GetOrAddBindingMember(BindingMemberExpressionNode.TargetType.Default, methodCall.MethodName);
            }
            else
            {
                member = GetOrAddBindingMember(methodCall.Target, methodCall.MethodName);
                if (member == null)
                    return methodCall;
            }

            return methodCall.UpdateTarget(member).Accept(this, metadata);
        }

        private IExpressionNode VisitMemberExpression(IMemberExpressionNode memberExpression)
        {
            return GetOrAddBindingMember(memberExpression, null) ?? memberExpression;
        }

        private IExpressionNode VisitIndexExpression(IIndexExpressionNode indexExpression)
        {
            return GetOrAddBindingMember(indexExpression, null) ?? indexExpression;
        }

        private IExpressionNode? GetOrAddBindingMember(IExpressionNode target, string? methodName)
        {
            if (target.TryBuildBindingMemberPath(_memberBuilder, _condition, out var firstExpression))
                return GetOrAddBindingMember(BindingMemberExpressionNode.TargetType.Default, methodName);

            if (firstExpression is UnaryExpressionNode unaryExpression && unaryExpression.IsMacros() &&
                unaryExpression.Operand is IMemberExpressionNode memberExpression)
            {
                //$target, $self, $this
                if (memberExpression.MemberName == MacrosConstants.Target || memberExpression.MemberName == MacrosConstants.Self ||
                    memberExpression.MemberName == MacrosConstants.This)
                    return GetOrAddBindingMember(BindingMemberExpressionNode.TargetType.TargetOnly, methodName);

                //$source
                if (memberExpression.MemberName == MacrosConstants.Source)
                    return GetOrAddBindingMember(BindingMemberExpressionNode.TargetType.SourceOnly, methodName);

                //$context
                if (memberExpression.MemberName == MacrosConstants.Context)
                {
                    _memberBuilder.Insert(0, BindableMembers.Object.DataContext);
                    return GetOrAddBindingMember(BindingMemberExpressionNode.TargetType.TargetOnly, methodName);
                }

                //type -> $string, $int, etc
                var type = _resourceResolver.DefaultIfNull().TryGetType(memberExpression.MemberName);
                if (type != null)
                {
                    if (unaryExpression.Token == UnaryTokenType.StaticExpression)
                    {
                        var value = _observerProvider.DefaultIfNull()
                            .GetMemberPath(_memberBuilder.GetPath())
                            .GetValueFromPath(type, null, MemberFlags | MemberFlags.Static, memberProvider: _memberProvider);
                        return ConstantExpressionNode.Get(value);
                    }

                    return GetOrAddInstance(type, MemberFlags.SetInstanceOrStaticFlags(true), methodName);
                }

                //resource -> $i18n, $color, etc
                if (unaryExpression.Token == UnaryTokenType.StaticExpression)
                {
                    var resourceValue = _resourceResolver.DefaultIfNull().TryGetResourceValue(memberExpression.MemberName);
                    if (resourceValue == null)
                        BindingExceptionManager.ThrowCannotResolveResource(memberExpression.MemberName);
                    if (resourceValue.Value == null)
                        return ConstantExpressionNode.Null;

                    var value = _observerProvider
                        .DefaultIfNull()
                        .GetMemberPath(_memberBuilder.GetPath())
                        .GetValueFromPath(resourceValue.Value.GetType(), resourceValue.Value, MemberFlags.SetInstanceOrStaticFlags(false), memberProvider: _memberProvider);
                    return ConstantExpressionNode.Get(value);
                }

                _memberBuilder.Insert(0, nameof(IResourceValue.Value));
                return GetOrAddResource(memberExpression.MemberName, methodName);
            }

            return null;
        }

        private IExpressionNode GetOrAddBindingMember(BindingMemberExpressionNode.TargetType targetType, string? methodName)
        {
            var key = new CacheKey(_memberBuilder.GetPath(), methodName, MemberFlags.SetInstanceOrStaticFlags(false), null, (BindingMemberType)targetType);
            if (!_members.TryGetValue(key, out var node))
            {
                node = new BindingMemberExpressionNode(targetType, key.Path, _observerProvider)
                {
                    ObservableMethodName = methodName,
                    Flags = Flags,
                    MemberFlags = key.MemberFlags
                };

                _members[key] = node;
            }

            return node;
        }

        private IExpressionNode GetOrAddInstance(object instance, MemberFlags flags, string? methodName)
        {
            var key = new CacheKey(_memberBuilder.GetPath(), methodName, flags, instance, BindingMemberType.Instance);
            if (!_members.TryGetValue(key, out var node))
            {
                node = new BindingInstanceMemberExpressionNode(instance, key.Path, _observerProvider)
                {
                    ObservableMethodName = methodName,
                    Flags = Flags,
                    MemberFlags = key.MemberFlags
                };

                _members[key] = node;
            }

            return node;
        }

        private IExpressionNode GetOrAddResource(string resourceName, string? methodName)
        {
            var key = new CacheKey(_memberBuilder.GetPath(), methodName, MemberFlags.SetInstanceOrStaticFlags(false), null, BindingMemberType.Resource);
            if (!_members.TryGetValue(key, out var node))
            {
                node = new BindingResourceMemberExpressionNode(resourceName, key.Path, _observerProvider, _resourceResolver)
                {
                    ObservableMethodName = methodName,
                    Flags = Flags,
                    MemberFlags = key.MemberFlags
                };

                _members[key] = node;
            }

            return node;
        }

        #endregion

        #region Nested types

        private sealed class MemberDictionary : LightDictionary<CacheKey, IBindingMemberExpressionNode>
        {
            #region Constructors

            public MemberDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(CacheKey x, CacheKey y)
            {
                return x.Path == y.Path && x.MethodName == y.MethodName && x.MemberFlags == y.MemberFlags && x.MemberType == y.MemberType && Equals(x.Target, y.Target);
            }

            protected override int GetHashCode(CacheKey key)
            {
                unchecked
                {
                    var hashCode = key.Path.GetHashCode();
                    hashCode = hashCode * 397 ^ (key.MethodName != null ? key.MethodName.GetHashCode() : 0);
                    hashCode = hashCode * 397 ^ (int)key.MemberFlags;
                    hashCode = hashCode * 397 ^ (int)key.MemberType;
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
            public readonly MemberFlags MemberFlags;
            public readonly BindingMemberType MemberType;
            public readonly object? Target;

            #endregion

            #region Constructors

            public CacheKey(string path, string? methodName, MemberFlags memberFlags, object? target, BindingMemberType memberType)
            {
                Path = path;
                MethodName = methodName;
                MemberFlags = memberFlags;
                MemberType = memberType;
                Target = target;
            }

            #endregion
        }

        private enum BindingMemberType : byte
        {
            Resource = 3,
            Instance = 4
        }

        #endregion
    }
}