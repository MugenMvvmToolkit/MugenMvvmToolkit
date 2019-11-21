using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public class ParameterExpressionNode : ExpressionNodeBase, IParameterExpressionNode
    {
        #region Constructors

        public ParameterExpressionNode(string name, int index, Type? type = null)
        {
            Should.NotBeNull(name, nameof(name));
            Index = index;
            Name = name;
            Type = type;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Parameter;

        public int Index { get; protected set; }

        public string Name { get; }

        public Type? Type { get; }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
        {
            return this;
        }

        public override string ToString()
        {
            if (NodeType == ExpressionNodeType.BindingMember)
                return "bindValue" + Index;
            return Name;
        }

        #endregion
    }
}