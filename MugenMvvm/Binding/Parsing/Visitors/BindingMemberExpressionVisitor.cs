using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Visitors
{
    public sealed class BindingMemberExpressionVisitor : IExpressionVisitor, IEqualityComparer<BindingMemberExpressionVisitor.CacheKey>
    {
        #region Fields

        private readonly Func<IExpressionNode, bool> _condition;
        private readonly StringBuilder _memberBuilder;
        private readonly IMemberManager? _memberManager;
        private readonly Dictionary<CacheKey, IExpressionNode> _members;
        private readonly IObservationManager? _observationManager;
        private readonly IResourceResolver? _resourceResolver;

        #endregion

        #region Constructors

        public BindingMemberExpressionVisitor(IObservationManager? observationManager = null, IResourceResolver? resourceResolver = null, IMemberManager? memberManager = null)
        {
            _observationManager = observationManager;
            _resourceResolver = resourceResolver;
            _memberManager = memberManager;
            _members = new Dictionary<CacheKey, IExpressionNode>(this);
            _memberBuilder = new StringBuilder();
            _condition = Condition;
        }

        #endregion

        #region Properties

        bool IExpressionVisitor.IsPostOrder => false;

        public MemberFlags MemberFlags { get; set; }

        public BindingMemberExpressionFlags Flags { get; set; }

        public bool IgnoreMethodMembers { get; set; }

        public bool IgnoreIndexMembers { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IEqualityComparer<CacheKey>.Equals(CacheKey x, CacheKey y)
        {
            return x.MemberFlags == y.MemberFlags && x.MemberType == y.MemberType && x.Path == y.Path && x.MethodName == y.MethodName && Equals(x.Target, y.Target);
        }

        int IEqualityComparer<CacheKey>.GetHashCode(CacheKey key)
        {
            return HashCode.Combine(key.Path, key.MethodName, (int) key.MemberFlags, (int) key.MemberType, key.Target);
        }

        IExpressionNode? IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is IMethodCallExpressionNode methodCall)
                return VisitHasTargetExpression(methodCall, methodCall.Method, metadata);

            if (expression is IMemberExpressionNode memberExpressionNode)
                return VisitMemberExpression(memberExpressionNode, metadata);

            if (expression is IIndexExpressionNode indexExpression)
                return VisitHasTargetExpression(indexExpression, null, metadata);

            if (expression is IUnaryExpressionNode unaryExpression && unaryExpression.IsMacros())
                return GetOrAddBindingMember(expression, null, metadata) ?? expression;

            return expression;
        }

        #endregion

        #region Methods

        [return: NotNullIfNotNull("expression")]
        public IExpressionNode? Visit(IExpressionNode? expression, bool isTargetExpression, IReadOnlyMetadataContext? metadata = null)
        {
            if (expression == null)
                return null;
            Flags = Flags.SetTargetFlags(isTargetExpression);
            _members.Clear();
            expression = expression.Accept(this, metadata);
            _members.Clear();
            return expression;
        }

        private IExpressionNode VisitMemberExpression(IMemberExpressionNode memberExpression, IReadOnlyMetadataContext? metadata)
        {
            return GetOrAddBindingMember(memberExpression, null, metadata) ?? memberExpression;
        }

        private IExpressionNode VisitHasTargetExpression(IHasTargetExpressionNode<IExpressionNode> expression, string? methodName, IReadOnlyMetadataContext? metadata)
        {
            var member = GetOrAddBindingMember(expression, null, metadata);
            if (member != null)
                return member;

            if (expression.Target == null)
            {
                _memberBuilder.Clear();
                member = GetOrAddBindingMember(null, methodName);
            }
            else
            {
                member = GetOrAddBindingMember(expression.Target, methodName, metadata);
                if (member == null)
                    return expression;
            }

            return expression.UpdateTarget(member);
        }

        private IExpressionNode? GetOrAddBindingMember(IExpressionNode target, string? methodName, IReadOnlyMetadataContext? metadata)
        {
            if (target.TryBuildBindingMemberPath(_memberBuilder, _condition, out var firstExpression))
                return GetOrAddBindingMember(null, methodName);

            if (firstExpression is IUnaryExpressionNode unaryExpression && unaryExpression.IsMacros() &&
                unaryExpression.Operand is IMemberExpressionNode memberExpression)
            {
                //$target, $self, $this
                if (memberExpression.Member == MacrosConstant.Target || memberExpression.Member == MacrosConstant.Self ||
                    memberExpression.Member == MacrosConstant.This)
                    return GetOrAddBindingMember(true, methodName);

                //$source
                if (memberExpression.Member == MacrosConstant.Source)
                    return GetOrAddBindingMember(false, methodName);

                //$context
                if (memberExpression.Member == MacrosConstant.Context)
                {
                    _memberBuilder.Insert(0, BindableMembers.For<object>().DataContext());
                    return GetOrAddBindingMember(true, methodName);
                }

                //type -> $string, $int, etc
                var type = _resourceResolver.DefaultIfNull().TryGetType(memberExpression.Member, memberExpression, metadata);
                if (type != null)
                {
                    if (unaryExpression.Token == UnaryTokenType.StaticExpression)
                    {
                        var result = TryGetConstant("~t", memberExpression.Member, out var key);
                        if (result == null)
                        {
                            var value = _observationManager.DefaultIfNull()
                                .GetMemberPath(_memberBuilder.GetPath(), metadata)
                                .GetValueFromPath(type, null, MemberFlags.SetInstanceOrStaticFlags(true), 0, metadata, _memberManager);
                            result = ConstantExpressionNode.Get(value);
                            _members[key] = result;
                        }

                        return result;
                    }

                    return GetOrAddInstance(type, MemberFlags.SetInstanceOrStaticFlags(true), methodName);
                }

                //resource -> $i18n, $color, etc
                var resourceValue = _resourceResolver.DefaultIfNull().TryGetResourceValue(memberExpression.Member, memberExpression, metadata);
                if (unaryExpression.Token == UnaryTokenType.StaticExpression || resourceValue != null && resourceValue.IsStatic)
                {
                    var result = TryGetConstant("~r", memberExpression.Member, out var key);
                    if (result == null)
                    {
                        if (resourceValue == null)
                            BindingExceptionManager.ThrowCannotResolveResource(memberExpression.Member);
                        if (resourceValue.Value == null)
                            result = ConstantExpressionNode.Null;
                        else
                        {
                            var value = _observationManager
                                .DefaultIfNull()
                                .GetMemberPath(_memberBuilder.GetPath(), metadata)
                                .GetValueFromPath(resourceValue.Value.GetType(), resourceValue.Value, MemberFlags.SetInstanceOrStaticFlags(false), 0, metadata, _memberManager);
                            result = ConstantExpressionNode.Get(value);
                        }

                        _members[key] = result;
                    }

                    return result;
                }

                _memberBuilder.Insert(0, nameof(IResourceValue.Value));
                return GetOrAddResource(memberExpression.Member, methodName);
            }

            return null;
        }

        private IExpressionNode GetOrAddBindingMember(bool? isTarget, string? methodName)
        {
            byte type = 0;
            var flags = Flags;
            if (isTarget != null)
            {
                if (isTarget.Value)
                {
                    flags = flags.SetTargetFlags(true);
                    type = 2;
                }
                else
                {
                    flags = flags.SetTargetFlags(false);
                    type = 1;
                }
            }

            var key = new CacheKey(_memberBuilder.GetPath(), methodName, MemberFlags.SetInstanceOrStaticFlags(false), null, (BindingMemberType) type);
            if (!_members.TryGetValue(key, out var node))
            {
                node = new BindingMemberExpressionNode(key.Path, _observationManager)
                {
                    ObservableMethodName = methodName,
                    Flags = flags,
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
                node = new BindingInstanceMemberExpressionNode(instance, key.Path, _observationManager)
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
                node = new BindingResourceMemberExpressionNode(resourceName, key.Path, _observationManager, _resourceResolver)
                {
                    ObservableMethodName = methodName,
                    Flags = Flags,
                    MemberFlags = key.MemberFlags
                };

                _members[key] = node;
            }

            return node;
        }

        private IExpressionNode? TryGetConstant(string constantType, string constantId, out CacheKey key)
        {
            key = new CacheKey(_memberBuilder.GetPath(), constantType, MemberFlags, constantId, BindingMemberType.Constant);
            _members.TryGetValue(key, out var node);
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

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct CacheKey
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

        internal enum BindingMemberType : byte
        {
            Resource = 3,
            Instance = 4,
            Constant = 5
        }

        #endregion
    }
}