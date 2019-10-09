using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public class MemberExpressionCompilerComponent : ExpressionCompilerComponent.ICompiler, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;
        private static readonly MethodInfo GetValueMethod = typeof(IBindingPropertyInfo).GetMethodOrThrow(nameof(IBindingPropertyInfo.GetValue), MemberFlags.InstancePublic);

        #endregion

        #region Constructors

        public MemberExpressionCompilerComponent(IMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public MemberFlags MemberFlags { get; set; } = MemberFlags.All & ~MemberFlags.NonPublic;

        #endregion

        #region Implementation of interfaces

        public Expression? TryCompile(ExpressionCompilerComponent.IContext context, IExpressionNode expression)
        {
            if (!(expression is IMemberExpressionNode memberExpression) || memberExpression.Target == null)
                return null;

            var target = context.Compile(memberExpression.Target);
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
                .GetMember(type, memberExpression.MemberName, BindingMemberType.Property | BindingMemberType.Field, flags);

            if (member == null)
            {
                BindingExceptionManager.ThrowInvalidBindingMember(type, memberExpression.MemberName);
                return null;
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
                GetValueMethod, target.ConvertIfNeed(typeof(object), false), context.MetadataExpression);
            return Expression.Convert(methodCall, propertyInfo.Type);
        }

        #endregion

        #region Methods

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