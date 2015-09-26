#region Copyright

// ****************************************************************************
// <copyright file="DefaultBindingParserHandler.cs">
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
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Parse
{
    public sealed class DefaultBindingParserHandler : IBindingParserHandler, IExpressionVisitor
    {
        #region Fields

        private static readonly Dictionary<string, string> ReplaceKeywords;

        internal const string GetEventArgsMethod = "GetEventArgs";
        internal const string GetErrorsMethod = "GetErrors";

        private readonly Dictionary<Guid, string[]> _errorPathNames;

        #endregion

        #region Constructors

        static DefaultBindingParserHandler()
        {
            ReplaceKeywords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"&lt;", "<"},
                {"&gt;", ">"},
                {"&quot;", "\""},
                {"&amp;", "&"}
            };
        }

        public DefaultBindingParserHandler()
        {
            _errorPathNames = new Dictionary<Guid, string[]>();
        }

        #endregion

        #region Implementation of interfaces

        public void Handle(ref string bindingExpression, IDataContext context)
        {
            foreach (var replaceKeyword in ReplaceKeywords)
                bindingExpression = bindingExpression.Replace(replaceKeyword.Key, replaceKeyword.Value);
        }

        public void HandleTargetPath(ref string targetPath, IDataContext context)
        {
        }

        public Action<IDataContext> Handle(ref IExpressionNode expression, bool isPrimaryExpression, IDataContext context)
        {
            if (expression == null)
                return null;
            //Updating relative sources.
            expression = expression.Accept(RelativeSourcePathMergerVisitor.Instance).Accept(NullConditionalOperatorVisitor.Instance);
            if (!isPrimaryExpression)
                return null;
            lock (_errorPathNames)
            {
                if (!HasGetErrorsMethod(ref expression))
                    return null;
                var pairs = _errorPathNames.ToArrayEx();
                return dataContext => UpdateBindingContext(dataContext, pairs);
            }
        }

        IExpressionNode IExpressionVisitor.Visit(IExpressionNode node)
        {
            var methodCallExpressionNode = node as IMethodCallExpressionNode;
            if (methodCallExpressionNode == null)
                return node;
            if (methodCallExpressionNode.Method == GetErrorsMethod &&
                methodCallExpressionNode.Target is ResourceExpressionNode)
            {
                var paths = methodCallExpressionNode.Arguments
                                        .OfType<IConstantExpressionNode>()
                                        .Where(expressionNode => expressionNode.Type == typeof(string))
                                        .Select(expressionNode => expressionNode.Value as string ?? string.Empty);
                Guid id = Guid.NewGuid();
                _errorPathNames[id] = paths.ToArray();
                var idNode = new ConstantExpressionNode(id, typeof(Guid));

                var args = methodCallExpressionNode.Arguments.ToList();
                //Adding binding source member if the expression does not contain members.
                if (args.Count == 0)
                    args.Add(new MemberExpressionNode(ResourceExpressionNode.DynamicInstance,
                        BindingServiceProvider.ResourceResolver.BindingSourceResourceName));
                args.Insert(0, idNode);
                return new MethodCallExpressionNode(methodCallExpressionNode.Target, methodCallExpressionNode.Method,
                    args, methodCallExpressionNode.TypeArgs);
            }
            return node;
        }

        #endregion

        #region Methods

        private bool HasGetErrorsMethod(ref IExpressionNode node)
        {
            _errorPathNames.Clear();
            node = node.Accept(this);
            return _errorPathNames.Count != 0;
        }

        private static void UpdateBindingContext(IDataContext dataContext, KeyValuePair<Guid, string[]>[] methods)
        {
            var behaviors = dataContext.GetOrAddBehaviors();
            behaviors.Clear();
            behaviors.Add(new OneTimeBindingMode(false));
            for (int i = 0; i < methods.Length; i++)
            {
                var pair = methods[i];
                behaviors.Add(new NotifyDataErrorsAggregatorBehavior(pair.Key) { ErrorPaths = pair.Value });
            }
        }

        #endregion
    }
}
