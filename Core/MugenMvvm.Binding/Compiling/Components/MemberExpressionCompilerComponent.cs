using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public class MemberExpressionCompilerComponent : ExpressionCompilerComponent.ICompiler, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider _memberProvider;

        #endregion

        #region Constructors

        public MemberExpressionCompilerComponent(IMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

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
            var @enum = type.TryParseEnum(memberExpression.MemberName);
            if (@enum != null)
                return Expression.Constant(@enum);

            var member = _memberProvider.ServiceIfNull().GetMember(type, memberExpression.MemberName);
            if (member == null)
                return null;
        }

        #endregion

        #region Methods

        private Expression? TryCompile(Expression? target, object? member)
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