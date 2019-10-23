using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Parsing.Visitors
{
    public sealed class BindingMemberExpressionCollectorVisitor : IExpressionVisitor
    {
        #region Fields

        private readonly List<IBindingMemberExpression> _members;

        #endregion

        #region Constructors

        public BindingMemberExpressionCollectorVisitor()
        {
            _members = new List<IBindingMemberExpression>(4);
        }

        #endregion

        #region Properties

        public bool IsPostOrder => false;

        #endregion

        #region Implementation of interfaces

        IExpressionNode IExpressionVisitor.Visit(IExpressionNode node)
        {
            if (node is IBindingMemberExpression bindingMember && !_members.Contains(bindingMember))
            {
                bindingMember.SetIndex(_members.Count);
                _members.Add(bindingMember);
            }

            return node;
        }

        #endregion

        #region Methods

        public IBindingMemberExpression[] Collect(IExpressionNode expression)
        {
            expression.Accept(this);
            if (_members.Count == 0)
                return Default.EmptyArray<IBindingMemberExpression>();

            var expressions = _members.ToArray();
            _members.Clear();
            return expressions;
        }

        #endregion
    }
}