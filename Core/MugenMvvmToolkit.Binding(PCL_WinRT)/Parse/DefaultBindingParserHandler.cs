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
using System.Globalization;
using System.Linq;
using System.Text;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Parse
{
    public sealed class DefaultBindingParserHandler : IBindingParserHandler, IExpressionVisitor
    {
        #region Fields

        public static readonly Dictionary<string, string> ReplaceKeywords;
        private static readonly Tokenizer InterpolatedStringTokenizer;
        private static readonly HashSet<string> QuoteSymbols;

        internal const string GetEventArgsMethod = "GetEventArgs";
        internal const string GetErrorsMethod = "GetErrors";
        internal const string GetBindingMethod = "GetBinding";

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
            QuoteSymbols = new HashSet<string> { "\"", "'" };
            InterpolatedStringTokenizer = new Tokenizer(false);
            InterpolatedStringTokenizer.IgnoreChars.Add('\'');
            InterpolatedStringTokenizer.IgnoreChars.Add('"');
        }

        public DefaultBindingParserHandler()
        {
            _errorPathNames = new Dictionary<Guid, string[]>();
        }

        #endregion

        #region Properties

        public bool IsPostOrder
        {
            get { return false; }
        }

        #endregion

        #region Implementation of interfaces

        public void Handle(ref string bindingExpression, IDataContext context)
        {
            foreach (var replaceKeyword in ReplaceKeywords)
                bindingExpression = bindingExpression.Replace(replaceKeyword.Key, replaceKeyword.Value);

            if (!bindingExpression.Contains("$\"{") && !bindingExpression.Contains("$'{"))
                return;
            Dictionary<string, string> dict = null;
            InterpolatedStringTokenizer.SetSource(bindingExpression);
            while (InterpolatedStringTokenizer.Token != TokenType.Eof)
            {
                int start;
                int end;
                var exp = ParseInterpolatedString(InterpolatedStringTokenizer, out start, out end);
                if (exp != null)
                {
                    if (dict == null)
                        dict = new Dictionary<string, string>();
                    dict[bindingExpression.Substring(start, end - start)] = exp;
                }
            }
            if (dict != null)
            {
                foreach (var s in dict)
                {
                    if (s.Value != null)
                        bindingExpression = bindingExpression.Replace(s.Key, s.Value);
                }
            }
        }

        public void HandleTargetPath(ref string targetPath, IDataContext context)
        {
        }

        public Action<IDataContext> Handle(ref IExpressionNode expression, bool isPrimaryExpression, IDataContext context)
        {
            if (expression == null)
                return null;
            expression = expression.Accept(MacrosExpressionVisitor.Instance).Accept(NullConditionalOperatorVisitor.Instance);
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
                return new MethodCallExpressionNode(methodCallExpressionNode.Target, methodCallExpressionNode.Method, args, methodCallExpressionNode.TypeArgs);
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

        //https://msdn.microsoft.com/en-us/library/dn961160.aspx
        private static string ParseInterpolatedString(Tokenizer tokenizer, out int start, out int end, bool openQuote = false)
        {
            start = -1;
            end = -1;
            var resultBuilder = new StringBuilder();
            var items = new List<string>();
            while (tokenizer.Token != TokenType.Eof)
            {
                if (tokenizer.Token == TokenType.Dollar)
                {
                    start = tokenizer.Position - 1;
                    tokenizer.NextToken(false);
                    if (QuoteSymbols.Contains(tokenizer.Value))
                    {
                        if (openQuote)
                            return null;
                        openQuote = true;
                        tokenizer.NextToken(false);
                    }
                }
                if (QuoteSymbols.Contains(tokenizer.Value))
                {
                    if (openQuote)
                    {
                        end = tokenizer.Position;
                        tokenizer.NextToken(false);
                        return string.Format("$string.Format(\"{0}\", {1})", resultBuilder, string.Join(",", items));
                    }
                }

                if (!openQuote)
                {
                    tokenizer.NextToken(false);
                    continue;
                }

                if (tokenizer.Token == TokenType.OpenBrace)
                {
                    resultBuilder.Append(tokenizer.Value);
                    tokenizer.NextToken(false);
                    //Ignoring two brace 
                    if (tokenizer.Token == TokenType.OpenBrace)
                    {
                        resultBuilder.Append(tokenizer.Value);
                        tokenizer.NextToken(false);
                        continue;
                    }
                    var item = new StringBuilder();
                    bool hasItem = false;
                    bool hasFieldWidth = false;
                    bool hasFormat = false;
                    bool startFormat = false;
                    int openParenCount = 0;
                    while (true)
                    {
                        if (tokenizer.Token == TokenType.Eof)
                            return null;
                        if (tokenizer.Token == TokenType.Dollar)
                        {
                            tokenizer.NextToken(false);
                            if (QuoteSymbols.Contains(tokenizer.Value))
                            {
                                if (hasItem)
                                    return null;
                                tokenizer.NextToken(false);
                                int s, e;
                                var nestedItem = ParseInterpolatedString(tokenizer, out s, out e, true);
                                if (nestedItem == null)
                                    return null;
                                item.Append(nestedItem);
                            }
                            else
                            {
                                if (hasItem)
                                    resultBuilder.Append(TokenType.Dollar.Value);
                                else
                                    item.Append(TokenType.Dollar.Value);
                            }
                        }
                        if (tokenizer.Token == TokenType.OpenParen)
                            ++openParenCount;
                        else if (tokenizer.Token == TokenType.CloseParen)
                            --openParenCount;
                        else if (openParenCount == 0)
                        {
                            if (tokenizer.Token == TokenType.CloseBrace)
                            {
                                AddInterpolatedItem(ref hasItem, resultBuilder, item, items);
                                resultBuilder.Append(tokenizer.Value);
                                tokenizer.NextToken(false);
                                break;
                            }

                            if (tokenizer.Token == TokenType.Comma)
                            {
                                AddInterpolatedItem(ref hasItem, resultBuilder, item, items);
                                resultBuilder.Append(tokenizer.Value);
                                while (tokenizer.NextToken(false) == TokenType.Whitespace)
                                    resultBuilder.Append(tokenizer.Value);
                                if (tokenizer.Token == TokenType.IntegerLiteral)
                                {
                                    if (hasFieldWidth)
                                        return null;
                                    hasFieldWidth = true;
                                    if (startFormat)
                                        hasFormat = true;
                                }
                                else if (!startFormat)
                                    return null;
                            }
                            else if (tokenizer.Token == TokenType.Colon)
                            {
                                if (hasFormat)
                                    return null;
                                AddInterpolatedItem(ref hasItem, resultBuilder, item, items);
                                startFormat = true;
                            }
                        }


                        if (hasItem)
                            resultBuilder.Append(tokenizer.Value);
                        else
                            item.Append(tokenizer.Value);
                        tokenizer.NextToken(false);
                    }
                }
                else if (tokenizer.Token == TokenType.CloseBrace)
                {
                    tokenizer.NextToken(false);
                    if (tokenizer.Token != TokenType.CloseBrace)
                        return null;
                    tokenizer.NextToken(false);
                    resultBuilder.Append("}}");
                }
                else
                {
                    resultBuilder.Append(tokenizer.Value);
                    tokenizer.NextToken(false);
                }
            }
            return null;
        }

        private static void AddInterpolatedItem(ref bool hasItem, StringBuilder builder, StringBuilder itemBuilder, List<string> items)
        {
            if (hasItem)
                return;
            hasItem = true;
            var exp = itemBuilder.ToString();
            int index = items.IndexOf(exp);
            if (index < 0)
            {
                index = items.Count;
                items.Add(exp);
            }
            builder.Append(index.ToString(CultureInfo.InvariantCulture));
        }

        #endregion
    }
}
