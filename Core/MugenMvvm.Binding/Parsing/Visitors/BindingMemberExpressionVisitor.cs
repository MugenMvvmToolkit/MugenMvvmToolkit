using System.Text;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Collections.Internal;

namespace MugenMvvm.Binding.Parsing.Visitors
{
    public class BindingMemberExpressionVisitor : IExpressionVisitor //todo relativesource visitor, resource visitor
    {
        #region Fields

        private readonly StringBuilder _memberNameBuilder;
        private readonly StringOrdinalLightDictionary<IExpressionNode> _members;

        #endregion

        #region Constructors

        public BindingMemberExpressionVisitor()
        {
            _members = new StringOrdinalLightDictionary<IExpressionNode>(3);
            _memberNameBuilder = new StringBuilder();
        }

        #endregion

        #region Properties

        public bool IsPostOrder => false;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? Visit(IExpressionNode node)
        {
            if (node is IMethodCallExpressionNode methodCall)
                return VisitMethodCall(methodCall);

            if (node is IMemberExpressionNode memberExpressionNode)
                return VisitMemberExpression(memberExpressionNode);

            if (node is IIndexExpressionNode indexExpression)
                return VisitIndexExpression(indexExpression);

            return node;
        }

        #endregion

        #region Methods

        public void Clear()
        {
            _members.Clear();
        }

        private IExpressionNode VisitMethodCall(IMethodCallExpressionNode methodCall)
        {
            IExpressionNode? member;
            if (methodCall.Target == null)
                member = GetOrAddBindingParameter(string.Empty, methodCall.MethodName);
            else
            {
                member = GetOrAddBindingParameter(methodCall.Target, methodCall.MethodName);
                if (member == null)
                    return methodCall;
            }

            if (methodCall.Method == null)
                return new MethodCallExpressionNode(member, methodCall.MethodName, methodCall.Arguments, methodCall.TypeArgs).Accept(this);
            return new MethodCallExpressionNode(member, methodCall.Method, methodCall.Arguments, methodCall.TypeArgs).Accept(this);
        }

        private IExpressionNode VisitMemberExpression(IMemberExpressionNode memberExpression)
        {
            return GetOrAddBindingParameter(memberExpression, null) ?? memberExpression;
        }

        private IExpressionNode VisitIndexExpression(IIndexExpressionNode indexExpression)
        {
            return GetOrAddBindingParameter(indexExpression, null) ?? indexExpression;
        }

        private IExpressionNode? GetOrAddBindingParameter(IExpressionNode target, string? methodName)
        {
            if (target.TryBuildBindingMember(_memberNameBuilder, out _))
                return GetOrAddBindingParameter(_memberNameBuilder.ToString(), methodName);
            return null;
        }

        private IExpressionNode GetOrAddBindingParameter(string path, string? methodName)
        {
            if (!_members.TryGetValue(path, out var node))
            {
                //                node = new BindingMemberExpression(path, _members.Count);//todo fix
                _members[path] = node;
            }

            return node;
        }

        #endregion
    }
}