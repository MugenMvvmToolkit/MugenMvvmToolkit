#region Copyright
// ****************************************************************************
// <copyright file="DefaultBindingParserHandler.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
        private const string Temp = "~~~___~~~";
        private readonly object _locker;
        private bool _hasGetErrors;

        #endregion

        #region Constructors

        static DefaultBindingParserHandler()
        {
            ReplaceKeywords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"&lt;", "<"},
                {"&gt;", ">"},
                {"&amp;", "&"},
                {"&quot;", "\""},
                {"$self", "{RelativeSource Self}"},
                {"$this", "{RelativeSource Self}"},
                {"$context", "{RelativeSource Self, Path=DataContext}"},
                {"$args", GetEventArgsDynamicMethod},
                {"$arg", GetEventArgsDynamicMethod}
            };
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultBindingParserHandler" /> class.
        /// </summary>
        public DefaultBindingParserHandler()
        {
            _locker = new object();
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

            //Replacing the symbol \' to ' and ' to "
            bindingExpression = bindingExpression.Replace(@"\'", Temp).Replace(@"'", "\"").Replace(Temp, @"'").Trim();
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
            if (!HasGetErrorsMethod(expression))
                return null;
            return UpdateBindingContext;
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
                _hasGetErrors = true;
            return node;
        }

        #endregion

        #region Methods

        private bool HasGetErrorsMethod(IExpressionNode node)
        {
            lock (_locker)
            {
                _hasGetErrors = false;
                node.Accept(this);
                return _hasGetErrors;
            }
        }

        private static void UpdateBindingContext(IDataContext dataContext)
        {
            var behaviors = dataContext.GetOrAddBehaviors();
            behaviors.Clear();
            behaviors.Add(NoneBindingMode.Instance);
            behaviors.Add(new NotifyDataErrorsAggregatorBehavior());
        }

        #endregion
    }
}