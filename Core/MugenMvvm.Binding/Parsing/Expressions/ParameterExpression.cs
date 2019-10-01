using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class ParameterExpression : ExpressionNodeBase, IParameterExpression
    {
        #region Constructors

        public ParameterExpression(string name, int index, Type? type = null)
        {
            Should.NotBeNull(name, nameof(name));
            Index = index;
            Name = name;
            Type = type;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Parameter;

        public int Index { get; }

        public string Name { get; }

        public Type? Type { get; }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
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