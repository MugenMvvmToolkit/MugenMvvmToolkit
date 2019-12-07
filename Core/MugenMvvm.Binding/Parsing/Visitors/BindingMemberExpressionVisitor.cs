﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
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

        public MemberFlags MemberFlags { get; set; }

        public BindingMemberExpressionFlags Flags { get; set; }

        public bool IgnoreMethodMembers { get; set; }

        public bool IgnoreIndexMembers { get; set; }

        #endregion

        #region Implementation of interfaces

        IExpressionNode? IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is IMethodCallExpressionNode methodCall)
                return VisitMethodCall(methodCall, metadata);

            if (expression is IMemberExpressionNode memberExpressionNode)
                return VisitMemberExpression(memberExpressionNode, metadata);

            if (expression is IIndexExpressionNode indexExpression)
                return VisitIndexExpression(indexExpression, metadata);

            if (expression is IUnaryExpressionNode unaryExpression && unaryExpression.IsMacros())
                return GetOrAddBindingMember(expression, null, metadata) ?? expression;

            return expression;
        }

        #endregion

        #region Methods

        [return: NotNullIfNotNull("expression")]
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
            var member = GetOrAddBindingMember(methodCall, null, metadata);
            if (member != null)
                return member;

            if (methodCall.Target == null)
            {
                _memberBuilder.Clear();
                member = GetOrAddBindingMember(BindingMemberExpressionNode.TargetType.Default, methodCall.Method);
            }
            else
            {
                member = GetOrAddBindingMember(methodCall.Target, methodCall.Method, metadata);
                if (member == null)
                    return methodCall;
            }

            return methodCall.UpdateTarget(member).Accept(this, metadata);
        }

        private IExpressionNode VisitMemberExpression(IMemberExpressionNode memberExpression, IReadOnlyMetadataContext? metadata)
        {
            return GetOrAddBindingMember(memberExpression, null, metadata) ?? memberExpression;
        }

        private IExpressionNode VisitIndexExpression(IIndexExpressionNode indexExpression, IReadOnlyMetadataContext? metadata)
        {
            return GetOrAddBindingMember(indexExpression, null, metadata) ?? indexExpression;
        }

        private IExpressionNode? GetOrAddBindingMember(IExpressionNode target, string? methodName, IReadOnlyMetadataContext? metadata)
        {
            if (target.TryBuildBindingMemberPath(_memberBuilder, _condition, out var firstExpression))
                return GetOrAddBindingMember(BindingMemberExpressionNode.TargetType.Default, methodName);

            if (firstExpression is UnaryExpressionNode unaryExpression && unaryExpression.IsMacros() &&
                unaryExpression.Operand is IMemberExpressionNode memberExpression)
            {
                //$target, $self, $this
                if (memberExpression.Member == MacrosConstant.Target || memberExpression.Member == MacrosConstant.Self ||
                    memberExpression.Member == MacrosConstant.This)
                    return GetOrAddBindingMember(BindingMemberExpressionNode.TargetType.TargetOnly, methodName);

                //$source
                if (memberExpression.Member == MacrosConstant.Source)
                    return GetOrAddBindingMember(BindingMemberExpressionNode.TargetType.SourceOnly, methodName);

                //$context
                if (memberExpression.Member == MacrosConstant.Context)
                {
                    _memberBuilder.Insert(0, BindableMembers.Object.DataContext);
                    return GetOrAddBindingMember(BindingMemberExpressionNode.TargetType.TargetOnly, methodName);
                }

                //type -> $string, $int, etc
                var type = _resourceResolver.DefaultIfNull().TryGetType(memberExpression.Member);
                if (type != null)
                {
                    if (unaryExpression.Token == UnaryTokenType.StaticExpression)
                    {
                        var value = _observerProvider.DefaultIfNull()
                            .GetMemberPath(_memberBuilder.GetPath(), metadata)
                            .GetValueFromPath(type, null, MemberFlags | MemberFlags.Static, 0, metadata, _memberProvider);
                        return ConstantExpressionNode.Get(value);
                    }

                    return GetOrAddInstance(type, MemberFlags.SetInstanceOrStaticFlags(true), methodName);
                }

                //resource -> $i18n, $color, etc
                if (unaryExpression.Token == UnaryTokenType.StaticExpression)
                {
                    var resourceValue = _resourceResolver.DefaultIfNull().TryGetResourceValue(memberExpression.Member, metadata);
                    if (resourceValue == null)
                        BindingExceptionManager.ThrowCannotResolveResource(memberExpression.Member);
                    if (resourceValue.Value == null)
                        return ConstantExpressionNode.Null;
                
                    var value = _observerProvider
                        .DefaultIfNull()
                        .GetMemberPath(_memberBuilder.GetPath(), metadata)
                        .GetValueFromPath(resourceValue.Value.GetType(), resourceValue.Value, MemberFlags.SetInstanceOrStaticFlags(false), 0, metadata, _memberProvider);
                    return ConstantExpressionNode.Get(value);
                }

                _memberBuilder.Insert(0, nameof(IResourceValue.Value));
                return GetOrAddResource(memberExpression.Member, methodName);
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

        private bool Condition(IExpressionNode arg)
        {
            if (IgnoreIndexMembers && arg is IIndexExpressionNode)
                return false;

            if (IgnoreMethodMembers && arg is IMethodCallExpressionNode)
                return false;
            return true;
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
                return x.MemberFlags == y.MemberFlags && x.MemberType == y.MemberType && x.Path == y.Path && x.MethodName == y.MethodName && Equals(x.Target, y.Target);
            }

            protected override int GetHashCode(CacheKey key)
            {
                return HashCode.Combine(key.Path, key.MethodName, (int)key.MemberFlags, (int)key.MemberType, key.Target);
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