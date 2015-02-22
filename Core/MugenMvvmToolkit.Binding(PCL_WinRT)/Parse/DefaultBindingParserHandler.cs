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
    /// <summary>
    ///     Represents the class that adds ability to use some macros like $self, $this, $context, $args, $arg, $GetErrors() in bindings.
    ///     This class also updates some string literals.
    /// </summary>
    public sealed class DefaultBindingParserHandler : IBindingParserHandler, IExpressionVisitor
    {
        #region Fields

        private static readonly Dictionary<string, string> ReplaceKeywords;

        internal const string GetEventArgsMethod = "GetEventArgs";
        internal const string GetErrorsMethod = "GetErrors";
        private const string GetEventArgsDynamicMethod = "$GetEventArgs()";

        private readonly List<string> _errorPathNames;
        private bool _hasGetErrors;

        #endregion

        #region Constructors

        static DefaultBindingParserHandler()
        {
            ReplaceKeywords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"&lt;", "<"},
                {"&gt;", ">"},
                {"&quot;", "\""},
                {"&amp;", "&"},
                {"$self", "{RelativeSource Self}"},
                {"$this", "{RelativeSource Self}"},
                {"$context", "{RelativeSource Self, Path=DataContext}"},
                {"$args", GetEventArgsDynamicMethod},
                {"$arg", GetEventArgsDynamicMethod}
            };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultBindingParserHandler" /> class.
        /// </summary>
        public DefaultBindingParserHandler()
        {
            _errorPathNames = new List<string>();
        }

        #endregion

        #region Implementation of interfaces

        /// <summary>
        ///     Prepares a string for the binding.
        /// </summary>
        /// <param name="bindingExpression">The specified binding expression.</param>
        /// <param name="context">The specified context.</param>
        /// <returns>An instance of <see cref="string" />.</returns>
        public void Handle(ref string bindingExpression, IDataContext context)
        {
            foreach (var replaceKeyword in ReplaceKeywords)
                bindingExpression = bindingExpression.Replace(replaceKeyword.Key, replaceKeyword.Value);
        }

        /// <summary>
        ///     Prepares a target path for the binding.
        /// </summary>
        /// <param name="targetPath">The specified target path.</param>
        /// <param name="context">The specified context.</param>
        /// <returns>An instance of <see cref="string" />.</returns>
        public void HandleTargetPath(ref string targetPath, IDataContext context)
        {
        }

        /// <summary>
        ///     Prepares an <see cref="IExpressionNode" /> for the binding.
        /// </summary>
        /// <param name="expression">The specified binding expression.</param>
        /// <param name="isPrimaryExpression">If <c>true</c> it's main binding expression; otherwise parameter expression.</param>
        /// <param name="context">The specified context.</param>
        /// <returns>An instance of delegate to update binding.</returns>
        public Action<IDataContext> Handle(ref IExpressionNode expression, bool isPrimaryExpression, IDataContext context)
        {
            if (expression == null)
                return null;
            //Updating relative sources.
            expression = expression.Accept(RelativeSourcePathMergerVisitor.Instance);
            if (!isPrimaryExpression)
                return null;
            lock (_errorPathNames)
            {
                if (!HasGetErrorsMethod(expression))
                    return null;
                var strings = _errorPathNames.Count == 0 ? null : _errorPathNames.ToArrayEx();
                return dataContext => UpdateBindingContext(dataContext, strings);
            }
        }

        /// <summary>
        ///     Dispatches the expression.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
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
                _errorPathNames.AddRange(paths);
                _hasGetErrors = true;

                //Adding binding source member if the expression does not contain members.
                if (methodCallExpressionNode.Arguments.Count == 0)
                {
                    return new MethodCallExpressionNode(methodCallExpressionNode.Target, methodCallExpressionNode.Method,
                        new IExpressionNode[]
                        {
                            new MemberExpressionNode(ResourceExpressionNode.DynamicInstance,
                                BindingServiceProvider.ResourceResolver.BindingSourceResourceName)
                        },
                        methodCallExpressionNode.TypeArgs);
                }
            }
            return node;
        }

        #endregion

        #region Methods

        private bool HasGetErrorsMethod(IExpressionNode node)
        {
            _hasGetErrors = false;
            _errorPathNames.Clear();
            node.Accept(this);
            return _hasGetErrors;
        }

        private static void UpdateBindingContext(IDataContext dataContext, string[] errorPathNames)
        {
            var behaviors = dataContext.GetOrAddBehaviors();
            behaviors.Clear();
            behaviors.Add(new OneTimeBindingMode(false));
            behaviors.Add(new NotifyDataErrorsAggregatorBehavior { ErrorPaths = errorPathNames });
        }

        #endregion
    }
}