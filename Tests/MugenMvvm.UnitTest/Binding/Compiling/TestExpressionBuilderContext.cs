using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.UnitTest.Binding.Compiling
{
    public class TestExpressionBuilderContext : MetadataOwnerBase, IExpressionBuilderContext
    {
        #region Constructors

        public TestExpressionBuilderContext(IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(metadata, metadataContextProvider)
        {
        }

        #endregion

        #region Properties

        public Expression MetadataExpression { get; set; }

        public Func<IExpressionNode, Expression?>? TryGetExpression { get; set; }

        public Action<IExpressionNode, Expression?>? SetExpression { get; set; }

        public Action<IExpressionNode>? ClearExpression { get; set; }

        public Func<IExpressionNode, Expression>? Build { get; set; }

        #endregion

        #region Implementation of interfaces

        Expression? IExpressionBuilderContext.TryGetExpression(IExpressionNode expression)
        {
            return TryGetExpression?.Invoke(expression);
        }

        void IExpressionBuilderContext.SetExpression(IExpressionNode expression, Expression value)
        {
            SetExpression?.Invoke(expression, value);
        }

        void IExpressionBuilderContext.ClearExpression(IExpressionNode expression)
        {
            ClearExpression?.Invoke(expression);
        }

        Expression IExpressionBuilderContext.Build(IExpressionNode expression)
        {
            return Build?.Invoke(expression) ?? throw new NotSupportedException();
        }

        #endregion
    }
}