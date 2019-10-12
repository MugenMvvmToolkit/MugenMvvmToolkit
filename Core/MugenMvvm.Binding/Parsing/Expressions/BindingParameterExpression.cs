using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class BindingParameterExpression : ParameterExpression
    {
        #region Constructors

        public BindingParameterExpression(string path, int index)
            : base(path, index)
        {
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.BindingParameter;

        #endregion

        #region Methods

        public override string ToString()
        {
            return "bindingValue" + Index.ToString();
        }

        #endregion
    }
}