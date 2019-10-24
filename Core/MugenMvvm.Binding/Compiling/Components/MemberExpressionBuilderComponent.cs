using System;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class MemberExpressionBuilderComponent : ExpressionCompilerComponent.IExpressionBuilder, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;
        private readonly Expression _thisExpression;

        private static readonly MethodInfo GetValuePropertyMethod =
            typeof(IBindingMemberAccessorInfo).GetMethodOrThrow(nameof(IBindingMemberAccessorInfo.GetValue), MemberFlags.InstancePublic);

        private static readonly MethodInfo GetValueDynamicMethod = typeof(MemberExpressionBuilderComponent).GetMethodOrThrow(nameof(GetValueDynamic), MemberFlags.InstancePublic);

        #endregion

        #region Constructors

        public MemberExpressionBuilderComponent(IMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
            _thisExpression = Expression.Constant(this, typeof(MemberExpressionBuilderComponent));
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public MemberFlags MemberFlags { get; set; } = MemberFlags.All & ~MemberFlags.NonPublic;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(ExpressionCompilerComponent.IContext context, IExpressionNode expression)
        {
            if (expression is IMemberExpressionNode memberExpression)
                return memberExpression.Build(this, context, (component, ctx, m, target) => component.Build(ctx, m, target));
            return null;

        }

        #endregion

        #region Methods

        private Expression Build(ExpressionCompilerComponent.IContext context, IMemberExpressionNode memberExpression, Expression target)
        {
            var type = BindingMugenExtensions.GetTargetType(ref target);
            var member = memberExpression.Member;
            if (member == null)
            {
                MemberFlags flags;
                if (target == null)
                {
                    var @enum = type.TryParseEnum(memberExpression.MemberName);
                    if (@enum != null)
                        return Expression.Constant(@enum);

                    flags = MemberFlags & ~MemberFlags.Instance;
                }
                else
                    flags = MemberFlags & ~MemberFlags.Static;

                member = _memberProvider
                    .ServiceIfNull()
                    .GetMember(type, memberExpression.MemberName,
                        BindingMemberType.Property | BindingMemberType.Field, flags, context.GetMetadataOrDefault()) as IBindingMemberAccessorInfo;
            }

            if (member == null)
            {
                if (target == null)
                    BindingExceptionManager.ThrowInvalidBindingMember(type, memberExpression.MemberName);

                return Expression.Call(_thisExpression, GetValueDynamicMethod,
                    target.ConvertIfNeed(typeof(object), false),
                    Expression.Constant(memberExpression.MemberName, typeof(string)),
                    context.MetadataParameter);
            }

            var result = TryCompile(target, member.Member);
            if (result != null)
                return result;

            var methodCall = Expression.Call(Expression.Constant(member, typeof(IBindingMemberAccessorInfo)),
                GetValuePropertyMethod, target.ConvertIfNeed(typeof(object), false), context.MetadataParameter);
            return Expression.Convert(methodCall, member.Type);
        }

        [Preserve(Conditional = true)]
        public object? GetValueDynamic(object? target, string member, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return null;
            var property = MugenBindingService
                    .MemberProvider
                    .GetMember(target.GetType(), member, BindingMemberType.Property | BindingMemberType.Field, MemberFlags & ~MemberFlags.Static, metadata) as
                IBindingMemberAccessorInfo;
            if (property == null)
                BindingExceptionManager.ThrowInvalidBindingMember(target.GetType(), member);
            return property!.GetValue(target, metadata);
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