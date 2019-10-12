using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
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
            typeof(IBindingPropertyInfo).GetMethodOrThrow(nameof(IBindingPropertyInfo.GetValue), MemberFlags.InstancePublic);
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
            if (!(expression is IMemberExpressionNode memberExpression) || memberExpression.Target == null)
                return null;

            var target = context.Build(memberExpression.Target);
            var result = TryCompile(target, memberExpression.Member);
            if (result != null)
                return result;

            var type = BindingMugenExtensions.GetTargetType(ref target);
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

            var member = _memberProvider
                .ServiceIfNull()
                .GetMember(type, memberExpression.MemberName, BindingMemberType.Property | BindingMemberType.Field, flags, context.GetMetadataOrDefault());

            if (member == null)
            {
                if (target == null)
                    BindingExceptionManager.ThrowInvalidBindingMember(type, memberExpression.MemberName);

                return Expression.Call(_thisExpression, GetValueDynamicMethod,
                    target.ConvertIfNeed(typeof(object), false),
                    Expression.Constant(memberExpression.Member, typeof(string)),
                    context.MetadataParameter);
            }

            result = TryCompile(target, member.Member);
            if (result != null)
                return result;

            if (!(member is IBindingPropertyInfo propertyInfo))
            {
                BindingExceptionManager.ThrowInvalidBindingMember(type, memberExpression.MemberName);
                return null;
            }

            var methodCall = Expression.Call(Expression.Constant(propertyInfo, typeof(IBindingPropertyInfo)),
                GetValuePropertyMethod, target.ConvertIfNeed(typeof(object), false), context.MetadataParameter);
            return Expression.Convert(methodCall, propertyInfo.Type);
        }

        #endregion

        #region Methods

        [Preserve(Conditional = true)]
        public object? GetValueDynamic(object? source, string member, IReadOnlyMetadataContext? metadata)
        {
            if (source == null)
                return null;
            var property = MugenBindingService
                .MemberProvider
                .GetMember(source.GetType(), member, BindingMemberType.Property | BindingMemberType.Field, MemberFlags & ~MemberFlags.Static, metadata) as IBindingPropertyInfo;
            if (property == null)
                BindingExceptionManager.ThrowInvalidBindingMember(source.GetType(), member);
            return property!.GetValue(source, metadata);
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