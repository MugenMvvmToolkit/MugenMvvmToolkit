using System.Collections.Generic;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Parsing.Visitors
{
    public sealed class BindingMemberExpressionCollectorVisitor : IExpressionVisitor
    {
        #region Fields

        private readonly List<IBindingMemberExpressionNode> _members;

        #endregion

        #region Constructors

        public BindingMemberExpressionCollectorVisitor()
        {
            _members = new List<IBindingMemberExpressionNode>(4);
        }

        #endregion

        #region Properties

        bool IExpressionVisitor.IsPostOrder => false;

        #endregion

        #region Implementation of interfaces

        IExpressionNode IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is IBindingMemberExpressionNode bindingMember && !_members.Contains(bindingMember))
            {
                bindingMember.Index = _members.Count;
                _members.Add(bindingMember);
            }

            return expression;
        }

        #endregion

        #region Methods

        public ItemOrList<IBindingMemberExpressionNode, IReadOnlyList<IBindingMemberExpressionNode>> Collect(IExpressionNode? expression, IReadOnlyMetadataContext? metadata = null)
        {
            if (expression == null)
                return default;

            expression.Accept(this, metadata);
            return _members.ToItemOrList(true);
        }

        #endregion
    }
}