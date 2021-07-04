﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public sealed class MemberExpressionBuilder : IExpressionBuilderComponent, IHasPriority
    {
        private static readonly MethodInfo GetValuePropertyMethod =
            typeof(IAccessorMemberInfo).GetMethodOrThrow(nameof(IAccessorMemberInfo.GetValue), BindingFlagsEx.InstancePublic);

        private static readonly MethodInfo GetValueDynamicMethod = typeof(MemberExpressionBuilder).GetMethodOrThrow(nameof(GetValueDynamic), BindingFlagsEx.InstancePublic);

        private readonly IMemberManager? _memberManager;
        private readonly Expression _thisExpression;

        public MemberExpressionBuilder(IMemberManager? memberManager = null)
        {
            _memberManager = memberManager;
            _thisExpression = Expression.Constant(this);
        }

        public EnumFlags<MemberFlags> MemberFlags { get; set; } = Enums.MemberFlags.All & ~Enums.MemberFlags.NonPublic;

        public int Priority { get; init; } = CompilingComponentPriority.Member;

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

        [Preserve(Conditional = true)]
        public object? GetValueDynamic(object? target, string member, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return null;
            var property = _memberManager
                           .DefaultIfNull()
                           .TryGetMember(target.GetType(), MemberType.Accessor, MemberFlags.SetInstanceOrStaticFlags(false), member, metadata) as IAccessorMemberInfo;
            if (property == null)
                ExceptionManager.ThrowInvalidBindingMember(target.GetType(), member);
            return property.GetValue(target, metadata);
        }

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (expression is not IMemberExpressionNode memberExpression || memberExpression.Target == null)
                return null;

            var target = context.BuildTarget(memberExpression.Target, out var type);
            EnumFlags<MemberFlags> flags;
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
                         .TryGetMember(type, MemberType.Accessor, flags, memberExpression.Member, context.GetMetadataOrDefault()) as IAccessorMemberInfo;

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
            {
                return Expression.Call(Expression.Constant(member), GetValuePropertyMethod, MugenExtensions.NullConstantExpression, context.MetadataExpression)
                                 .ConvertIfNeed(member.Type, false);
            }

            return Expression
                   .Call(Expression.Constant(member), GetValuePropertyMethod, target.ConvertIfNeed(typeof(object), false), context.MetadataExpression)
                   .ConvertIfNeed(member.Type, false);
        }
    }
}