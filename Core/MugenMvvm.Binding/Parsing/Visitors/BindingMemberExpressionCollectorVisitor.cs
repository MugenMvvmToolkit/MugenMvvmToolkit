using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing.Visitors
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

        public ItemOrList<IBindingMemberExpressionNode, IBindingMemberExpressionNode[]> Collect(IExpressionNode? expression, IReadOnlyMetadataContext? metadata = null)
        {
            if (expression == null)
                return default;

            expression.Accept(this, metadata);
            if (_members.Count == 0)
                return Default.EmptyArray<IBindingMemberExpressionNode>();

            if (_members.Count == 1)
            {
                var r = new ItemOrList<IBindingMemberExpressionNode, IBindingMemberExpressionNode[]>(_members[0]);
                _members.Clear();
                return r;
            }

            var expressions = _members.ToArray();
            _members.Clear();
            return expressions;
        }

        #endregion
    }
}