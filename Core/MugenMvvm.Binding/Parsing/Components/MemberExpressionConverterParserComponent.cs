using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class MemberExpressionConverterParserComponent : IExpressionConverterParserComponent<Expression>, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;

        #endregion

        #region Constructors

        public MemberExpressionConverterParserComponent(IMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Member;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (!(expression is MemberExpression memberExpression))
                return null;

            var member = memberExpression.Member;
            var target = context.ConvertOptional(memberExpression.Expression) ?? ConstantExpressionNode.Get(member.DeclaringType);
            var accessModifiers = (member as PropertyInfo)?.GetAccessModifiers()
                                  ?? (member as FieldInfo)?.GetAccessModifiers()
                                  ?? (member as EventInfo)?.GetAccessModifiers()
                                  ?? MemberFlags.Public | (memberExpression.Expression == null ? MemberFlags.Static : MemberFlags.Instance);
            if (_memberProvider.DefaultIfNull().GetMember(member.DeclaringType, member.Name, MemberType.Accessor, accessModifiers, context.Metadata) is IMemberAccessorInfo memberAccessor)
                return new MemberExpressionNode(target, memberAccessor);
            return new MemberExpressionNode(target, member.Name);
        }

        #endregion
    }
}