using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Compiling
{
    public interface ILinqExpressionBuilderContext : IMetadataOwner<IMetadataContext>
    {
        Expression MetadataParameter { get; }

        IBindingParameterInfo? TryGetLambdaParameter();

        void SetLambdaParameter(IBindingParameterInfo parameter);

        void ClearLambdaParameter(IBindingParameterInfo parameter);

        Expression? TryGetExpression(IExpressionNode expression);

        void SetExpression(IExpressionNode expression, Expression value);

        void ClearExpression(IExpressionNode expression);

        Expression Build(IExpressionNode expression);
    }
}