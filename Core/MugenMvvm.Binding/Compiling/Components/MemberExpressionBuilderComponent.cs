using System;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class MemberExpressionBuilderComponent : IExpressionBuilderComponent, IHasPriority
    {
        #region Fields

        private readonly IMemberManager? _memberManager;
        private readonly Expression _thisExpression;

        private static readonly MethodInfo GetValuePropertyMethod =
            typeof(IMemberAccessorInfo).GetMethodOrThrow(nameof(IMemberAccessorInfo.GetValue), BindingFlagsEx.InstancePublic);

        private static readonly MethodInfo GetValueDynamicMethod = typeof(MemberExpressionBuilderComponent).GetMethodOrThrow(nameof(GetValueDynamic), BindingFlagsEx.InstancePublic);

        #endregion

        #region Constructors

        public MemberExpressionBuilderComponent(IMemberManager? memberManager = null)
        {
            _memberManager = memberManager;
            _thisExpression = Expression.Constant(this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = CompilingComponentPriority.Member;

        public MemberFlags MemberFlags { get; set; } = MemberFlags.All & ~MemberFlags.NonPublic;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (!(expression is IMemberExpressionNode memberExpression) || memberExpression.Target == null)
                return null;

            Expression? target = context.Build(memberExpression.Target);
            var type = MugenBindingExtensions.GetTargetType(ref target);
            MemberFlags flags;
            if (target == null)
            {
                if (type.IsEnum)
                    return Expression.Constant(Enum.Parse(type, memberExpression.Member));
                flags = MemberFlags.SetInstanceOrStaticFlags(true);
            }
            else
                flags = MemberFlags.SetInstanceOrStaticFlags(false);

            var member = _memberManager
                .DefaultIfNull()
                .GetMember(type, memberExpression.Member, MemberType.Accessor, flags, context.GetMetadataOrDefault()) as IMemberAccessorInfo;

            if (member == null)
            {
                if (target == null)
                {
                    context.TryGetErrors()?.Add(BindingMessageConstant.InvalidBindingMemberFormat2.Format(memberExpression.Member, type));
                    return null;
                }

                return Expression.Call(_thisExpression, GetValueDynamicMethod,
                    target.ConvertIfNeed(typeof(object), false),
                    Expression.Constant(memberExpression.Member),
                    context.MetadataExpression);
            }

            var result = TryCompile(target, member.UnderlyingMember);
            if (result != null)
                return result;

            if (target == null)
                return Expression.Call(Expression.Constant(member), GetValuePropertyMethod, MugenExtensions.NullConstantExpression, context.MetadataExpression).ConvertIfNeed(member.Type, false);
            return Expression
                .Call(Expression.Constant(member), GetValuePropertyMethod, target.ConvertIfNeed(typeof(object), false), context.MetadataExpression)
                .ConvertIfNeed(member.Type, false);
        }

        #endregion

        #region Methods

        [Preserve(Conditional = true)]
        public object? GetValueDynamic(object? target, string member, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return null;
            var property = _memberManager
                .DefaultIfNull()
                .GetMember(target.GetType(), member, MemberType.Accessor, MemberFlags.SetInstanceOrStaticFlags(false), metadata) as IMemberAccessorInfo;
            if (property == null)
                BindingExceptionManager.ThrowInvalidBindingMember(target.GetType(), member);
            return property.GetValue(target, metadata);
        }

        private static Expression? TryCompile(Expression? target, object? member)
        {
            if (member == null)
                return null;
            if (member is PropertyInfo property)
                return Expression.Property(target, property);
            if (member is FieldInfo field)
                return Expression.Field(target, field);
            return null;
        }

        #endregion
    }
}