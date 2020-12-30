using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Parsing.Visitors
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

        public EnumFlags<MemberFlags> MemberFlags { get; set; }

        public EnumFlags<BindingMemberExpressionFlags> Flags { get; set; }

        public bool SuppressMethodAccessors { get; set; }

        public bool SuppressIndexAccessors { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IEqualityComparer<CacheKey>.Equals(CacheKey x, CacheKey y) => x.MemberFlagsField == y.MemberFlagsField && x.BindingMemberFlagsField == y.BindingMemberFlagsField && x.MemberType == y.MemberType
                                                                           && x.Path == y.Path && x.MethodName == y.MethodName && Equals(x.Target, y.Target);

        int IEqualityComparer<CacheKey>.GetHashCode(CacheKey key) => HashCode.Combine(key.Path, key.MethodName, (int) key.MemberFlagsField, (int) key.BindingMemberFlagsField, (int) key.MemberType, key.Target);

        IExpressionNode IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
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

        private IExpressionNode VisitMemberExpression(IMemberExpressionNode memberExpression, IReadOnlyMetadataContext? metadata) => GetOrAddBindingMember(memberExpression, null, metadata) ?? memberExpression;

        private IExpressionNode VisitHasTargetExpression(IHasTargetExpressionNode<IExpressionNode> expression, string? methodName, IReadOnlyMetadataContext? metadata)
        {
            var member = GetOrAddBindingMember(expression, null, metadata);
            if (member != null)
                return member;

            if (expression.Target == null)
            {
                _memberBuilder.Clear();
                member = GetOrAddBindingMember(expression, null, methodName);
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
                return GetOrAddBindingMember(target, null, methodName);

            if (firstExpression is IUnaryExpressionNode unaryExpression && unaryExpression.IsMacros() &&
                unaryExpression.Operand is IMemberExpressionNode memberExpression)
            {
                //$target, $self, $this
                if (memberExpression.Member == MacrosConstant.Target || memberExpression.Member == MacrosConstant.Self ||
                    memberExpression.Member == MacrosConstant.This)
                    return GetOrAddBindingMember(target, true, methodName);

                //$source
                if (memberExpression.Member == MacrosConstant.Source)
                    return GetOrAddBindingMember(target, false, methodName);

                //$context
                if (memberExpression.Member == MacrosConstant.Context)
                {
                    _memberBuilder.Insert(0, BindableMembers.For<object>().DataContext());
                    return GetOrAddBindingMember(target, true, methodName);
                }

                //type -> $string, $int, etc
                var type = _resourceResolver.DefaultIfNull().TryGetType(memberExpression.Member, memberExpression, metadata);
                var memberFlags = target.GetFlags(BindingParameterNameConstant.MemberFlags, MemberFlags);
                IExpressionNode? result;
                CacheKey key;
                if (type != null)
                {
                    if (unaryExpression.Token == UnaryTokenType.StaticExpression)
                    {
                        result = TryGetConstant("~t", memberExpression.Member, out key);
                        if (result == null)
                        {
                            var value = _observationManager.DefaultIfNull()
                                .GetMemberPath(_memberBuilder.GetPath(), metadata)
                                .GetValueFromPath(type, null, memberFlags.SetInstanceOrStaticFlags(true), 0, metadata, _memberManager);
                            result = ConstantExpressionNode.Get(value);
                            _members[key] = result;
                        }

                        return result;
                    }

                    return GetOrAddInstance(target, type, memberFlags.SetInstanceOrStaticFlags(true), methodName);
                }

                //resource -> $i18n, $color, etc
                var resource = _resourceResolver.DefaultIfNull().TryGetResource(memberExpression.Member, memberExpression, metadata);
                var resourceValue = resource.Resource;
                if (unaryExpression.Token == UnaryTokenType.DynamicExpression)
                    return GetOrAddResource(target, memberExpression.Member, memberFlags.SetInstanceOrStaticFlags(false), methodName);

                if (!resource.IsResolved)
                    ExceptionManager.ThrowCannotResolveResource(memberExpression.Member);

                result = TryGetConstant("~r", memberExpression.Member, out key);
                if (result == null)
                {
                    if (resourceValue is IDynamicResource r)
                        resourceValue = r.Value;

                    if (resourceValue == null)
                        result = ConstantExpressionNode.Null;
                    else
                    {
                        result = ConstantExpressionNode.Get(_observationManager
                            .DefaultIfNull()
                            .GetMemberPath(_memberBuilder.GetPath(), metadata)
                            .GetValueFromPath(resourceValue.GetType(), resourceValue, memberFlags.SetInstanceOrStaticFlags(false), 0, metadata, _memberManager));
                    }

                    _members[key] = result;
                }

                return result;
            }

            return null;
        }

        private IExpressionNode GetOrAddBindingMember(IExpressionNode expression, bool? isTarget, string? methodName)
        {
            BindingMemberType type = 0;
            var flags = expression.GetFlags(BindingParameterNameConstant.BindingMemberFlags, Flags);
            var memberFlags = expression.GetFlags(BindingParameterNameConstant.MemberFlags, MemberFlags);
            if (isTarget != null)
            {
                if (isTarget.Value)
                {
                    flags = flags.SetTargetFlags(true);
                    type = BindingMemberType.Target;
                }
                else
                {
                    flags = flags.SetTargetFlags(false);
                    type = BindingMemberType.Source;
                }
            }

            var key = new CacheKey(_memberBuilder.GetPath(), methodName, memberFlags.SetInstanceOrStaticFlags(false), flags, null, type);
            if (!_members.TryGetValue(key, out var node))
            {
                node = new BindingMemberExpressionNode(key.Path, _observationManager)
                {
                    ObservableMethodName = methodName,
                    Flags = key.BindingMemberFlags,
                    MemberFlags = key.MemberFlags,
                    OriginalExpression = expression
                };

                _members[key] = node;
            }

            return node;
        }

        private IExpressionNode GetOrAddInstance(IExpressionNode expression, object instance, EnumFlags<MemberFlags> flags, string? methodName)
        {
            var key = new CacheKey(_memberBuilder.GetPath(), methodName, flags, expression.GetFlags(BindingParameterNameConstant.BindingMemberFlags, Flags), instance, BindingMemberType.Instance);
            if (!_members.TryGetValue(key, out var node))
            {
                node = new BindingInstanceMemberExpressionNode(instance, key.Path, _observationManager)
                {
                    ObservableMethodName = methodName,
                    Flags = key.BindingMemberFlags,
                    MemberFlags = key.MemberFlags,
                    OriginalExpression = expression
                };

                _members[key] = node;
            }

            return node;
        }

        private IExpressionNode GetOrAddResource(IExpressionNode expression, string resourceName, EnumFlags<MemberFlags> flags, string? methodName)
        {
            var key = new CacheKey(_memberBuilder.GetPath(), methodName, flags, expression.GetFlags(BindingParameterNameConstant.BindingMemberFlags, Flags), null, BindingMemberType.Resource);
            if (!_members.TryGetValue(key, out var node))
            {
                node = new BindingResourceMemberExpressionNode(resourceName, key.Path, _observationManager, _resourceResolver)
                {
                    ObservableMethodName = methodName,
                    Flags = key.BindingMemberFlags,
                    MemberFlags = key.MemberFlags,
                    OriginalExpression = expression
                };

                _members[key] = node;
            }

            return node;
        }

        private IExpressionNode? TryGetConstant(string constantType, string constantId, out CacheKey key)
        {
            key = new CacheKey(_memberBuilder.GetPath(), constantType, default, default, constantId, BindingMemberType.Constant);
            _members.TryGetValue(key, out var node);
            return node;
        }

        private bool Condition(IExpressionNode expression)
        {
            if (expression.TryGetMetadataValue(BindingParameterNameConstant.SuppressIndexAccessors, SuppressIndexAccessors) && expression is IIndexExpressionNode)
                return false;
            if (expression.TryGetMetadataValue(BindingParameterNameConstant.SuppressMethodAccessors, SuppressMethodAccessors) && expression is IMethodCallExpressionNode)
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
            public readonly ushort MemberFlagsField;
            public readonly ushort BindingMemberFlagsField;
            public readonly BindingMemberType MemberType;
            public readonly object? Target;

            #endregion

            #region Constructors

            public CacheKey(string path, string? methodName, EnumFlags<MemberFlags> memberFlags, EnumFlags<BindingMemberExpressionFlags> bindingMemberFlags, object? target, BindingMemberType memberType)
            {
                Path = path;
                MethodName = methodName;
                MemberFlagsField = memberFlags.Value();
                BindingMemberFlagsField = bindingMemberFlags.Value();
                MemberType = memberType;
                Target = target;
            }

            #endregion

            #region Properties

            public EnumFlags<BindingMemberExpressionFlags> BindingMemberFlags
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(BindingMemberFlagsField);
            }

            public EnumFlags<MemberFlags> MemberFlags
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(MemberFlagsField);
            }

            #endregion
        }

        internal enum BindingMemberType : byte
        {
            Source = 1,
            Target = 2,
            Resource = 3,
            Instance = 4,
            Constant = 5
        }

        #endregion
    }
}