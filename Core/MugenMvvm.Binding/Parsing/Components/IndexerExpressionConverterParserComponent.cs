using System.Linq.Expressions;
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
    public sealed class IndexerExpressionConverterParserComponent : IExpressionConverterParserComponent<Expression>, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;

        #endregion

        #region Constructors

        public IndexerExpressionConverterParserComponent(IMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Indexer;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (!(expression is IndexExpression index))
                return null;

            var target = context.ConvertOptional(index.Object) ?? ConstantExpressionNode.Get(index.Indexer.DeclaringType);
            var args = context.Convert(index.Arguments);
            var method = index.Indexer.GetGetMethod(true);
            if (method == null)
                return new IndexExpressionNode(target, args);
            if (_memberProvider.DefaultIfNull().GetMember(method.DeclaringType, method.Name, MemberType.Method, method.GetAccessModifiers(), context.Metadata) is IMethodInfo memberInfo)
                return new IndexExpressionNode(target, memberInfo, args);
            return new IndexExpressionNode(target, args);
        }

        #endregion
    }
}