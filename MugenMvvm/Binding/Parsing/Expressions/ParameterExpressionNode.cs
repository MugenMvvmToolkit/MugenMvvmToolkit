using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class ParameterExpressionNode : ExpressionNodeBase, IParameterExpressionNode
    {
        #region Constructors

        public ParameterExpressionNode(string name)
        {
            Should.NotBeNull(name, nameof(name));
            Name = name;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType ExpressionType => ExpressionNodeType.Parameter;

        public string Name { get; }

        #endregion

        #region Methods

        protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            return this;
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}