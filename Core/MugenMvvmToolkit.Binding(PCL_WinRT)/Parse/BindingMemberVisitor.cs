#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberVisitor.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Parse
{
    public class BindingMemberVisitor : IExpressionVisitor
    {
        #region Fields

        private readonly List<string> _lamdaParameters;
        private readonly IList<KeyValuePair<string, BindingMemberExpressionNode>> _members;
        private readonly Dictionary<IExpressionNode, IExpressionNode> _staticNodes;
        private bool _isMulti;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberVisitor" /> class.
        /// </summary>
        public BindingMemberVisitor()
        {
            _members = new List<KeyValuePair<string, BindingMemberExpressionNode>>();
            _lamdaParameters = new List<string>();
            _staticNodes = new Dictionary<IExpressionNode, IExpressionNode>();
        }

        private BindingMemberVisitor(BindingMemberVisitor innerVisitor, IEnumerable<string> lambdaParameters)
            : this()
        {
            _members = innerVisitor._members;
            _staticNodes = innerVisitor._staticNodes;
            if (innerVisitor._lamdaParameters != null)
                _lamdaParameters.AddRange(innerVisitor._lamdaParameters);
            _lamdaParameters.AddRange(lambdaParameters);
            BindingExtensions.CheckDuplicateLambdaParameter(_lamdaParameters);
        }

        #endregion

        #region Properties

        public IList<KeyValuePair<string, BindingMemberExpressionNode>> Members
        {
            get { return _members; }
        }

        public bool IsMulti
        {
            get { return _isMulti; }
        }

        #endregion

        #region Implementation of IExpressionVisitor

        /// <summary>
        ///     Dispatches the expression.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        public IExpressionNode Visit(IExpressionNode node)
        {
            var lamdaNode = node as ILambdaExpressionNode;
            if (lamdaNode != null)
                return VisitLambda(lamdaNode);

            var methodCall = node as IMethodCallExpressionNode;
            if (methodCall != null)
                return VisitMethodCall(methodCall);

            var relativeSource = node as IRelativeSourceExpressionNode;
            if (relativeSource != null)
                return VisitRelativeSource(relativeSource);
            return VisitExpression(node);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears all values.
        /// </summary>
        public void Clear()
        {
            _members.Clear();
            _isMulti = false;
            _lamdaParameters.Clear();
            _staticNodes.Clear();
        }

        private IExpressionNode VisitMethodCall(IMethodCallExpressionNode methodCall)
        {
            _isMulti = true;
            if (methodCall.Target != null)
                return methodCall;
            BindingMemberExpressionNode member = GetOrAddBindingMember(string.Empty, (s, i) => new BindingMemberExpressionNode(string.Empty, s, i));
            return new MethodCallExpressionNode(member, methodCall.Method, methodCall.Arguments, methodCall.TypeArgs).Accept(this);
        }

        private IExpressionNode VisitRelativeSource(IRelativeSourceExpressionNode rs)
        {
            string memberName = rs.Type + rs.Path + rs.Level.ToString() + rs.ElementName;
            return GetOrAddBindingMember(memberName, (s, i) => new BindingMemberExpressionNode(rs, s, i));
        }

        private IExpressionNode VisitExpression(IExpressionNode node)
        {
            var nodes = new List<IExpressionNode>();
            string memberName = node.TryGetMemberName(true, true, nodes);
            if (memberName == null)
            {
                _isMulti = true;
                return node;
            }
            if (nodes[0] is ResourceExpressionNode)
                return GetResourceMember(node, memberName, nodes);

            IBindingPath path = BindingServiceProvider.BindingPathFactory(memberName);
            if (path.IsEmpty)
                return GetOrAddBindingMember(memberName, (s, i) => new BindingMemberExpressionNode(memberName, s, i));
            string firstMember = path.Parts[0];
            if (_lamdaParameters.Contains(firstMember))
                return node;
            return GetOrAddBindingMember(memberName, (s, i) => new BindingMemberExpressionNode(memberName, s, i));
        }

        private IExpressionNode VisitLambda(ILambdaExpressionNode node)
        {
            _isMulti = true;
            node.Expression.Accept(new BindingMemberVisitor(this, node.Parameters));
            return node.Clone();
        }

        private IExpressionNode GetResourceMember(IExpressionNode node, string memberName, IList<IExpressionNode> nodes)
        {
            IExpressionNode staticValue;
            if (_staticNodes.TryGetValue(node, out staticValue))
                return staticValue;

            IBindingPath path = BindingServiceProvider.BindingPathFactory(memberName);
            string firstMember = path.Parts[0];
            Type type = BindingServiceProvider.ResourceResolver.ResolveType(firstMember, DataContext.Empty, false);
            var resourceMember = (ResourceExpressionNode)nodes[0];
            if (resourceMember.Dynamic && type == null)
            {
                memberName = BindingExtensions.MergePath(path.Parts.Skip(1).ToArray());
                return GetOrAddBindingMember("$" + path.Path, (s, i) => new BindingMemberExpressionNode(firstMember, memberName, s, i));
            }

            bool dynamicMember = false;
            IExpressionNode firstMemberNode = nodes[1];
            if (!_staticNodes.TryGetValue(firstMemberNode, out staticValue))
            {
                if (type == null)
                {
                    var resourceObject = BindingServiceProvider
                        .ResourceResolver
                        .ResolveObject(firstMember, DataContext.Empty, true);
                    var dynamicObject = resourceObject.Value as IDynamicObject;
                    if (dynamicObject == null || path.Parts.Count <= 1)
                        staticValue = new ConstantExpressionNode(resourceObject.Value);
                    else
                    {
                        staticValue = new ConstantExpressionNode(dynamicObject.GetMember(path.Parts[1], Empty.Array<object>()));
                        dynamicMember = true;
                    }
                }
                else
                    staticValue = new ConstantExpressionNode(type, typeof(Type));
                _staticNodes[firstMemberNode] = staticValue;
                if (dynamicMember)
                    _staticNodes[nodes[2]] = staticValue;
            }
            if (firstMemberNode == node || (dynamicMember && node == nodes[2]))
                return staticValue;
            return node;
        }

        private BindingMemberExpressionNode GetOrAddBindingMember(string memberName,
            Func<string, int, BindingMemberExpressionNode> getNode)
        {
            KeyValuePair<string, BindingMemberExpressionNode> bindingMember =
                _members.SingleOrDefault(pair => pair.Key == memberName);
            if (bindingMember.Value == null)
            {
                bindingMember = new KeyValuePair<string, BindingMemberExpressionNode>(memberName,
                    getNode(GetParameterName(), _members.Count));
                _members.Add(bindingMember);
            }
            return bindingMember.Value;
        }

        private string GetParameterName()
        {
            return "param_" + _members.Count.ToString();
        }

        #endregion
    }
}