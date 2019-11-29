using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Compiling
{
    public interface IExpressionBuilderContext : IMetadataOwner<IMetadataContext>
    {
        Expression MetadataParameter { get; }

        IParameterInfo? TryGetLambdaParameter();

        void SetLambdaParameter(IParameterInfo parameter);

        void ClearLambdaParameter(IParameterInfo parameter);

        Expression? TryGetExpression(IExpressionNode expression);

        void SetExpression(IExpressionNode expression, Expression value);

        void ClearExpression(IExpressionNode expression);

        Expression Build(IExpressionNode expression);
    }
}