using MugenMvvm.Binding.Interfaces.Parsing.Nodes;

namespace MugenMvvm.Binding.Parsing
{
    public readonly struct BindingParserParameter
    {
        #region Fields

        public readonly string? Name;

        public readonly IExpressionNode? Expression;

        #endregion

        #region Constructors

        public BindingParserParameter(string? name, IExpressionNode? expression)
        {
            Name = name;
            Expression = expression;
        }

        #endregion
    }
}