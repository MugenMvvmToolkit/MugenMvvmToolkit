#region Copyright

// ****************************************************************************
// <copyright file="BindingParser.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Converters;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Binding.Sources;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Parse
{
    /// <summary>
    ///     Represents the data binding parser.
    /// </summary>
    public class BindingParser : IBindingParser
    {
        #region Fields

        protected const string PathName = "Path";
        protected const string LevelName = "Level";
        protected const string TrueLiteral = "true";
        protected const string FalseLiteral = "false";
        protected const string NullLiteral = "null";

        private static readonly Comparison<KeyValuePair<KeyValuePair<string, int>, Action<IDataContext>[]>> MemberComparison;
        private static readonly Func<IDataContext, IBindingSource>[] EmptyBindingSourceDelegates;
        private static readonly HashSet<string> LiteralConstants;
        private static readonly HashSet<TokenType> LiteralTokens;
        private static readonly HashSet<TokenType> ResourceTokens;
        private static readonly HashSet<TokenType> DelimeterTokens;
        private static readonly Action<IDataContext> EmptyPathSourceDelegate;

        private readonly Dictionary<string, TokenType> _binaryOperationAliases;
        private readonly Dictionary<TokenType, int> _binaryOperationTokens;
        private readonly Dictionary<string, Func<BindingParser, IList<Action<IDataContext>>>> _bindingParameterToAction;

        private readonly Dictionary<string, KeyValuePair<KeyValuePair<string, int>, Action<IDataContext>[]>[]> _cache;
        private readonly ExpressionCounterVisitor _counterVisitor;
        private readonly Tokenizer _defaultTokenizer;
        private readonly ICollection<string> _elementSourceAliases;
        private readonly BindingMemberVisitor _memberVisitor;
        private readonly ICollection<string> _relativeSourceAliases;
        private readonly IDictionary<string, TokenType> _unaryOperationAliases;
        private readonly ICollection<TokenType> _unaryOperationTokens;
        private readonly List<IBindingParserHandler> _handlers;
        private readonly DataContext _defaultContext;

        private IDataContext _context;
        private string _expression;
        private bool _parsingTarget;
        private ITokenizer _tokenizer;

        private List<TokenType> _tokens;
        private List<IExpressionNode> _nodes;

        #endregion

        #region Constructors

        static BindingParser()
        {
            MemberComparison = OrderByMemberPriority;
            DelimeterTokens = new HashSet<TokenType>
            {
                TokenType.Comma,
                TokenType.Eof,
                TokenType.Semicolon
            };
            ResourceTokens = new HashSet<TokenType>
            {
                TokenType.Dollar,
                TokenType.DoubleDollar
            };
            LiteralConstants = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                TrueLiteral,
                FalseLiteral,
                NullLiteral
            };
            LiteralTokens = new HashSet<TokenType>
            {
                TokenType.IntegerLiteral,
                TokenType.RealLiteral,
                TokenType.StringLiteral
            };
            EmptyBindingSourceDelegates = new Func<IDataContext, IBindingSource>[]
            {
                BindEmptyPathSource
            };
            EmptyPathSourceDelegate = context => context.Add(BindingBuilderConstants.Sources, EmptyBindingSourceDelegates);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingParser" /> class.
        /// </summary>
        public BindingParser()
        {
            _cache = new Dictionary<string, KeyValuePair<KeyValuePair<string, int>, Action<IDataContext>[]>[]>(StringComparer.Ordinal);
            _defaultContext = new DataContext();
            _handlers = new List<IBindingParserHandler> { new DefaultBindingParserHandler() };
            _defaultTokenizer = new Tokenizer(true);
            _memberVisitor = new BindingMemberVisitor();
            _counterVisitor = new ExpressionCounterVisitor();
            _binaryOperationTokens = new Dictionary<TokenType, int>
            {
                {TokenType.QuestionDot, 101},
                {TokenType.Asterisk, 100},
                {TokenType.Slash, 100},
                {TokenType.Percent, 100},
                {TokenType.Plus, 99},
                {TokenType.Minus, 99},
                {TokenType.ExclamationEqual, 98},
                {TokenType.GreaterThan, 98},
                {TokenType.LessThan, 98},
                {TokenType.GreaterThanEqual, 98},
                {TokenType.LessThanEqual, 98},
                {TokenType.Equal, 97},
                {TokenType.DoubleEqual, 97},
                {TokenType.Amphersand, 96},
                {TokenType.Bar, 95},
                {TokenType.DoubleAmphersand, 94},
                {TokenType.DoubleBar, 93},
            };

            _unaryOperationTokens = new HashSet<TokenType>
            {
                TokenType.Minus,
                TokenType.Exclamation,
                TokenType.Tilde
            };

            _bindingParameterToAction = new Dictionary<string, Func<BindingParser, IList<Action<IDataContext>>>>(StringComparer.OrdinalIgnoreCase)
            {
                {"Mode", parser => parser.GetBindingModeSetter()},
                {"M", parser => parser.GetBindingModeSetter()},
                {"ValidatesOnNotifyDataErrors", parser => parser.GetBehaviorSetter(ValidatesOnNotifyDataErrorsBehavior.Prototype)},
                {"ValidatesOnErrors", parser => parser.GetBehaviorSetter(ValidatesOnNotifyDataErrorsBehavior.Prototype)},
                {"ValidatesOnExceptions", parser => parser.GetBehaviorSetter(ValidatesOnExceptionsBehavior.Instance)},
                {"Validate", parser => parser.GetBehaviorSetter(ValidatesOnNotifyDataErrorsBehavior.Prototype, ValidatesOnExceptionsBehavior.Instance)},
                {"DefaultValueOnException", parser => parser.GetDefaultValueOnExceptionSetter()},
                {"SetDefaultValue", parser => parser.GetDefaultValueOnExceptionSetter()},
                {"Delay", parser => parser.GetDelaySetter(false)},
                {"SourceDelay", parser => parser.GetDelaySetter(false)},
                {"TargetDelay", parser => parser.GetDelaySetter(true)},
                {"Converter", parser => parser.GetConverterSetter()},
                {"Conv", parser => parser.GetConverterSetter()},
                {"ConverterParameter", parser => parser.GetConverterParameterSetter()},
                {"ConverterCulture", parser => parser.GetConverterCultureSetter()},
                {"Fallback", parser => parser.GetFallbackSetter()},
                {"TargetNullValue", parser => parser.GetTargetNullValueSetter()},
                {AttachedMemberConstants.CommandParameter, parser => parser.GetCommandParameterSetter()},
                {"Behavior", parser => parser.GetCustomBehaviorSetter()},
                {"ToggleEnabledState", parser => parser.GetToggleEnabledState()},
                {"ToggleEnabled", parser => parser.GetToggleEnabledState()}
            };

            _binaryOperationAliases = new Dictionary<string, TokenType>(StringComparer.OrdinalIgnoreCase)
            {
                {"and", TokenType.DoubleAmphersand},
                {"or", TokenType.DoubleBar},
                {"mod", TokenType.Percent}
            };

            _unaryOperationAliases = new Dictionary<string, TokenType>(StringComparer.OrdinalIgnoreCase)
            {
                {"not", TokenType.Exclamation}
            };

            _relativeSourceAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                RelativeSourceExpressionNode.RelativeSourceType,
                "Relative",
                "Rel"
            };

            _elementSourceAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                RelativeSourceExpressionNode.ElementSourceType,
                "Element",
                "El"
            };
        }

        #endregion

        #region Implementation of IBindingParser

        /// <summary>
        ///     Gets the collection of <see cref="IBindingParserHandler" />.
        /// </summary>
        public IList<IBindingParserHandler> Handlers
        {
            get { return _handlers; }
        }

        /// <summary>
        ///     Parses a string to the set of instances of <see cref="IDataContext" /> that allows to create a series of instances
        ///     of <see cref="IDataBinding" />.
        /// </summary>
        /// <param name="bindingExpression">The specified binding expression.</param>
        /// <param name="context">The specified context.</param>
        /// <param name="target">The specified binding target.</param>
        /// <param name="sources">The specified sources, if any.</param>
        /// <returns>A set of instances of <see cref="IDataContext" />.</returns>
        public IList<IDataContext> Parse(string bindingExpression, IDataContext context, object target,
             IList<object> sources)
        {
            Should.NotBeNullOrWhitespace(bindingExpression, "bindingExpression");
            KeyValuePair<KeyValuePair<string, int>, Action<IDataContext>[]>[] bindingValues;
            lock (_cache)
            {
                if (!_cache.TryGetValue(bindingExpression, out bindingValues))
                {
                    try
                    {
                        if (ReferenceEquals(context, DataContext.Empty) || context == null)
                            context = _defaultContext;
                        context.AddOrUpdate(BindingBuilderConstants.Target, target);
                        _context = context;
                        _expression = Handle(bindingExpression, context);
                        _tokenizer = CreateTokenizer(Expression);
                        var value = ParseInternal()
                            .Select((pair, i) => new KeyValuePair<KeyValuePair<string, int>, Action<IDataContext>[]>(new KeyValuePair<string, int>(pair.Key, i), pair.Value))
                            .ToList();
                        value.Sort(MemberComparison);
                        bindingValues = value.ToArray();
                        if (!context.Contains(BindingBuilderConstants.NoCache))
                            _cache[bindingExpression] = bindingValues;
                    }
                    finally
                    {
                        if (ReferenceEquals(_defaultContext, context))
                        {
                            _defaultContext.Clear();
                            context = DataContext.Empty;
                        }
                        _tokenizer = null;
                        _expression = null;
                        _context = null;
                    }
                }
            }
            var result = new IDataContext[bindingValues.Length];
            if (sources != null && sources.Count > 0)
            {
                for (int i = 0; i < bindingValues.Length; i++)
                {
                    var pair = bindingValues[i];
                    var dataContext = new DataContext(context);
                    dataContext.AddOrUpdate(BindingBuilderConstants.Target, target);
                    if (pair.Key.Value < sources.Count)
                    {
                        object src = sources[pair.Key.Value];
                        if (src != null)
                            dataContext.Add(BindingBuilderConstants.Source, src);
                    }
                    var actions = pair.Value;
                    for (int j = 0; j < actions.Length; j++)
                        actions[j].Invoke(dataContext);
                    result[i] = dataContext;
                }
            }
            else
            {
                for (int i = 0; i < bindingValues.Length; i++)
                {
                    var actions = bindingValues[i].Value;
                    var dataContext = new DataContext(context);
                    dataContext.AddOrUpdate(BindingBuilderConstants.Target, target);
                    for (int j = 0; j < actions.Length; j++)
                        actions[j].Invoke(dataContext);
                    result[i] = dataContext;
                }
            }
            return result;
        }

        #endregion

        #region Properties

        public ICollection<string> ElementSourceAliases
        {
            get { return _elementSourceAliases; }
        }

        public ICollection<string> RelativeSourceAliases
        {
            get { return _relativeSourceAliases; }
        }

        public IDictionary<string, TokenType> UnaryOperationAliases
        {
            get { return _unaryOperationAliases; }
        }

        public IDictionary<string, TokenType> BinaryOperationAliases
        {
            get { return _binaryOperationAliases; }
        }

        /// <summary>
        ///     Gets the current string expression.
        /// </summary>
        protected string Expression
        {
            get { return _expression; }
        }

        /// <summary>
        ///     Gets the current <see cref="ITokenizer" />.
        /// </summary>
        protected ITokenizer Tokenizer
        {
            get { return _tokenizer; }
        }

        /// <summary>
        ///     Gets the external context.
        /// </summary>
        protected IDataContext Context
        {
            get { return _context; }
        }

        protected Dictionary<string, Func<BindingParser, IList<Action<IDataContext>>>> BindingParameterToAction
        {
            get { return _bindingParameterToAction; }
        }

        protected ICollection<TokenType> UnaryOperationTokens
        {
            get { return _unaryOperationTokens; }
        }

        protected IDictionary<TokenType, int> BinaryOperationTokens
        {
            get { return _binaryOperationTokens; }
        }

        private bool PrevMemberIsWhitespace
        {
            get { return char.IsWhiteSpace(Expression, Tokenizer.Position - 2); }
        }

        #endregion

        #region Methods

        #region Parsing

        /// <summary>
        ///     Parses a string to the <see cref="MugenMvvmToolkit.Models.DataContext" /> that allows to create an instance of
        ///     <see cref="IDataBinding" />.
        /// </summary>
        protected virtual List<KeyValuePair<string, Action<IDataContext>[]>> ParseInternal()
        {
            NextToken(true);
            var list = new List<KeyValuePair<string, Action<IDataContext>[]>>(1);
            while (true)
            {
                list.Add(Parse());
                if (Tokenizer.Token == TokenType.Semicolon)
                {
                    NextToken(true);
                    //Skip empty bindings.
                    while (Tokenizer.Token == TokenType.Semicolon)
                        NextToken(true);
                }
                if (Tokenizer.Token == TokenType.Eof)
                    break;
            }
            return list;
        }

        private KeyValuePair<string, Action<IDataContext>[]> Parse()
        {
            var actions = new List<Action<IDataContext>>();
            ValidateToken(TokenType.Identifier);
            _parsingTarget = true;
            IExpressionNode target = ParsePrimary();
            _parsingTarget = false;
            string targetPath = HandleTargetPath(target.TryGetMemberName(true, false), Context);
            if (string.IsNullOrEmpty(targetPath))
                throw BindingExceptionManager.InvalidExpressionParser(target.ToString(), Tokenizer, Expression);

            actions.Add(context => context.Add(BindingBuilderConstants.TargetPath, BindingServiceProvider.BindingPathFactory(targetPath)));
            //Empty source path.
            IExpressionNode source = IsAnyOf(DelimeterTokens)
                ? null
                : ParseExpression();

            while (true)
            {
                if (Tokenizer.Token == TokenType.Eof || Tokenizer.Token == TokenType.Semicolon)
                    break;
                ValidateToken(TokenType.Comma);
                ValidateToken(NextToken(true), TokenType.Identifier);
                string left = Tokenizer.Value;
                NextToken(true);
                var setters = GetBindingBehaviorSetter(left);
                if (setters != null)
                {
                    for (int i = 0; i < setters.Count; i++)
                        actions.AddIfNotNull(setters[i]);
                }
            }
            source = Handle(source, true, Context, actions);
            actions.Add(GetBindingSourceSetter(source));
            return new KeyValuePair<string, Action<IDataContext>[]>(targetPath, actions.ToArray());
        }

        protected virtual IExpressionNode ParseExpression()
        {
            return ParseBinaryExpression();
        }

        protected virtual IExpressionNode ParseBinaryExpression(IExpressionNode expr = null)
        {
            if (expr == null)
                expr = ParseUnary();
            if (IsBinaryToken())
            {
                TokenType token = Tokenizer.Token;
                if (!BinaryOperationTokens.ContainsKey(token))
                    token = BinaryOperationAliases[Tokenizer.Value];
                NextToken(true);
                IExpressionNode right = ParseUnary();
                expr = ParseBinaryExpression(expr, right, token, false);
            }
            if (Tokenizer.Token == TokenType.Question)
            {
                NextToken(true);
                IExpressionNode expr1 = ParseExpression();
                ValidateToken(TokenType.Colon);
                NextToken(true);
                IExpressionNode expr2 = ParseExpression();
                expr = new ConditionExpressionNode(expr, expr1, expr2);
            }
            if (Tokenizer.Token == TokenType.DoubleQuestion)
            {
                NextToken(true);
                IExpressionNode expr1 = ParseExpression();
                expr = new BinaryExpressionNode(expr, expr1, TokenType.DoubleQuestion);
            }
            return expr;
        }

        ///NOTE: It is possible to increase the operating speed by using another algorithm, but the current version does not involve large expressions.        
        private IExpressionNode ParseBinaryExpression(IExpressionNode left, IExpressionNode right, TokenType token, bool isInternalCall)
        {
            bool returnEmptyNode = false;
            List<IExpressionNode> nodes = null;
            List<TokenType> tokens = null;
            if (isInternalCall)
            {
                nodes = _nodes;
                tokens = _tokens;
                returnEmptyNode = nodes != null;
            }

            if (!IsBinaryToken() && !returnEmptyNode)
                return new BinaryExpressionNode(left, right, token);
            if (nodes == null)
                nodes = new List<IExpressionNode>();
            if (tokens == null)
                tokens = new List<TokenType>();
            nodes.Add(left);
            nodes.Add(right);
            tokens.Add(token);

            var oldTokens = _tokens;
            var oldNodes = _nodes;
            _nodes = nodes;
            _tokens = tokens;
            try
            {
                do
                {
                    token = Tokenizer.Token;
                    if (!BinaryOperationTokens.ContainsKey(token))
                        token = BinaryOperationAliases[Tokenizer.Value];
                    NextToken(true);
                    tokens.Add(token);
                    nodes.AddIfNotNull(ParseUnary());
                } while (IsBinaryToken());
            }
            finally
            {
                _tokens = oldTokens;
                _nodes = oldNodes;
            }
            if (returnEmptyNode)
                return null;

            int index = GetMaxPriorityTokenIndex(tokens);
            while (index != -1)
            {
                token = tokens[index];
                tokens.RemoveAt(index);
                nodes[index] = new BinaryExpressionNode(nodes[index], nodes[index + 1], token);
                nodes.RemoveAt(index + 1);
                index = GetMaxPriorityTokenIndex(tokens);
            }
            return nodes[0];
        }

        protected virtual IExpressionNode ParseUnary()
        {
            if (IsAnyOf(UnaryOperationTokens) || IsAnyOf(UnaryOperationAliases.Keys))
            {
                TokenType token = Tokenizer.Token;
                if (!UnaryOperationTokens.Contains(token))
                    token = UnaryOperationAliases[Tokenizer.Value];
                NextToken(true);
                return new UnaryExressionNode(ParsePrimary(), token);
            }
            return ParsePrimary();
        }

        protected virtual IExpressionNode ParsePrimary(IExpressionNode primaryStart = null)
        {
            IExpressionNode node = primaryStart ?? ParsePrimaryStart();
            while (true)
            {
                if (Tokenizer.Token == TokenType.Dot)
                {
                    NextToken(true);
                    node = ParseMemberAccess(node);
                }
                else if (Tokenizer.Token == TokenType.OpenBracket)
                {
                    if (_parsingTarget)
                    {
                        if (PrevMemberIsWhitespace)
                            return node;
                    }
                    node = ParseIndexer(node);
                }
                else
                    break;
            }
            return node;
        }

        protected virtual IExpressionNode ParseIndexer(IExpressionNode node)
        {
            NextToken(true);
            IList<IExpressionNode> args = ParseArguments();
            ValidateToken(TokenType.CloseBracket);
            NextToken(true);
            return new IndexExpressionNode(node, args);
        }

        protected virtual IExpressionNode ParseMethodCallExpression(IExpressionNode node, string method,
            IList<string> typeArgs)
        {
            IList<IExpressionNode> nodes = ParseArgumentList();
            return new MethodCallExpressionNode(node, method, nodes, typeArgs);
        }

        protected virtual IExpressionNode ParseBraceExpression()
        {
            ValidateToken(TokenType.OpenBrace);
            NextToken(true);
            string sourceName = Tokenizer.Value;
            if (sourceName != RelativeSourceExpressionNode.RelativeSourceType && sourceName != RelativeSourceExpressionNode.ElementSourceType &&
                !RelativeSourceAliases.Contains(sourceName) && !ElementSourceAliases.Contains(sourceName))
            {
                IExpressionNode node = ParsePrimary();
                string memberName = node.TryGetMemberName(true, false);
                if (string.IsNullOrEmpty(memberName))
                    throw BindingExceptionManager.UnknownIdentifierParser(sourceName, Tokenizer, Expression,
                        RelativeSourceExpressionNode.RelativeSourceType, RelativeSourceExpressionNode.ElementSourceType);
                ValidateToken(TokenType.CloseBrace);
                NextToken(true);
                return RelativeSourceExpressionNode.CreateBindingContextSource(memberName);
            }
            bool isRelativeSource = sourceName == RelativeSourceExpressionNode.RelativeSourceType || RelativeSourceAliases.Contains(sourceName);
            int position = Tokenizer.Position;
            NextToken(true);
            ValidateToken(TokenType.Identifier);
            string typeName = ParsePrimary().TryGetMemberName(false, false);
            if (string.IsNullOrEmpty(typeName))
                throw BindingExceptionManager.InvalidMemberName(Expression, position);
            string path = string.Empty;
            uint level = 1;
            if (typeName == RelativeSourceExpressionNode.SelfType || !isRelativeSource)
            {
                if (Tokenizer.Token == TokenType.Comma)
                {
                    NextToken(true);
                    path = ParsePath();
                }
            }
            else
            {
                if (Tokenizer.Token == TokenType.Comma)
                {
                    NextToken(true);
                    if (Tokenizer.Value == PathName)
                    {
                        path = ParsePath();
                        if (Tokenizer.Token == TokenType.Comma)
                        {
                            NextToken(true);
                            level = ParseLevel();
                        }
                    }
                    else
                    {
                        level = ParseLevel();
                        if (Tokenizer.Token == TokenType.Comma)
                        {
                            NextToken(true);
                            path = ParsePath();
                        }
                    }
                }
            }

            ValidateToken(TokenType.CloseBrace);
            NextToken(true);
            RelativeSourceExpressionNode expression;
            //Self
            if (typeName == RelativeSourceExpressionNode.SelfType)
                expression = RelativeSourceExpressionNode.CreateSelfSource(path);
            else if (isRelativeSource)
                expression = RelativeSourceExpressionNode.CreateRelativeSource(typeName, level, path);
            else
                expression = RelativeSourceExpressionNode.CreateElementSource(typeName, path);
            return ParsePrimary(expression);
        }

        protected virtual IExpressionNode ParseConstantExpression()
        {
            object value = null;
            if (Tokenizer.Token == TokenType.IntegerLiteral)
                value = ParseIntegerLiteral();
            else if (Tokenizer.Token == TokenType.RealLiteral)
                value = ParseRealLiteral();
            else if (Tokenizer.Token == TokenType.StringLiteral)
                value = ParseStringLiteral();
            else if (Tokenizer.Value.Equals(TrueLiteral, StringComparison.OrdinalIgnoreCase))
            {
                value = Empty.TrueObject;
                NextToken(true);
            }
            else if (Tokenizer.Value.Equals(FalseLiteral, StringComparison.OrdinalIgnoreCase))
            {
                value = Empty.FalseObject;
                NextToken(true);
            }
            else
                NextToken(true);
            var expr = new ConstantExpressionNode(value);
            return expr;
        }

        protected virtual IExpressionNode ParseMemberAccess(IExpressionNode node)
        {
            ValidateToken(TokenType.Identifier);
            string value = Tokenizer.Value;
            NextToken(true);

            //Try parse type args.
            var typeArgs = new List<string>();
            if (Tokenizer.Token == TokenType.LessThan)
            {
                NextToken(true);
                IExpressionNode type = ParseUnary();
                string typeName = type.TryGetMemberName(false, false);
                if (typeName != null)
                {
                    typeArgs.Add(typeName);
                    if (Tokenizer.Token == TokenType.Comma)
                    {
                        NextToken(true);
                        typeArgs.AddRange(ParseTypeArgs());
                        ValidateToken(TokenType.GreaterThan);
                    }
                    if (Tokenizer.Token == TokenType.GreaterThan)
                    {
                        NextToken(true);
                        return ParseMethodCallExpression(node, value, typeArgs);
                    }
                }
                return ParseBinaryExpression(new MemberExpressionNode(node, value), type, TokenType.LessThan, true);
            }

            if (Tokenizer.Token == TokenType.OpenParen)
            {
                if (!_parsingTarget || !PrevMemberIsWhitespace)
                    return ParseMethodCallExpression(node, value, null);
            }
            if (Tokenizer.Token == TokenType.EmptyParen)
            {
                NextToken(true);
                return new MethodCallExpressionNode(node, value, null, null);
            }
            return new MemberExpressionNode(node, value);
        }

        protected virtual IExpressionNode ParseParenExpression()
        {
            ValidateToken(TokenType.OpenParen);
            NextToken(true);
            IExpressionNode e = ParseExpression();
            ValidateToken(TokenType.CloseParen);
            NextToken(true);
            return e;
        }

        protected IExpressionNode ParsePrimaryStart()
        {
            bool isDynamicMember = IsAnyOf(ResourceTokens);
            if (isDynamicMember)
            {
                ResourceExpressionNode node = Tokenizer.Token == TokenType.DoubleDollar
                    ? ResourceExpressionNode.StaticInstance
                    : ResourceExpressionNode.DynamicInstance;
                NextToken(true);
                if (Tokenizer.Token == TokenType.Identifier)
                    return ParseMemberAccess(node);
            }
            else
            {
                if (IsAnyOf(LiteralTokens) || IsAnyOf(LiteralConstants))
                    return ParseConstantExpression();
                if (Tokenizer.Token == TokenType.Identifier)
                    return ParseMemberAccess(null);
                if (Tokenizer.Token == TokenType.OpenParen)
                    return ParseParenExpression();
                if (Tokenizer.Token == TokenType.OpenBracket)
                    return ParseIndexer(null);
                if (Tokenizer.Token == TokenType.OpenBrace)
                    return ParseBraceExpression();
            }
            throw BindingExceptionManager.UnknownIdentifierParser(Tokenizer.Value, Tokenizer, Expression);
        }

        protected IList<IExpressionNode> ParseArgumentList()
        {
            if (Tokenizer.Token == TokenType.EmptyParen)
            {
                NextToken(true);
                return Empty.Array<IExpressionNode>();
            }
            ValidateToken(TokenType.OpenParen);
            NextToken(true);
            IList<IExpressionNode> args = Tokenizer.Token != TokenType.CloseParen
                ? ParseArguments()
                : Empty.Array<IExpressionNode>();
            ValidateToken(TokenType.CloseParen);
            NextToken(true);
            return args;
        }

        protected IList<IExpressionNode> ParseArguments()
        {
            var argList = new List<IExpressionNode>();
            while (true)
            {
                argList.Add(ParseLambda());
                if (Tokenizer.Token != TokenType.Comma)
                    break;
                NextToken(true);
            }
            return argList;
        }

        protected IExpressionNode ParseLambda()
        {
            if (Tokenizer.Token == TokenType.EmptyParen)
            {
                ValidateToken(NextToken(true), TokenType.Lambda);
                NextToken(true);
                var item = new LambdaExpressionNode(ParseExpression(), null);
                return item;
            }

            if (Tokenizer.Token == TokenType.OpenParen)
            {
                NextToken(true);
                IExpressionNode node = ParseExpression();
                var memberExp = node as IMemberExpressionNode;
                if (memberExp != null && memberExp.Target == null)
                {
                    var nodes = new List<IExpressionNode> { memberExp };
                    if (Tokenizer.Token == TokenType.Comma)
                    {
                        NextToken(true);
                        nodes.AddRange(ParseArguments());
                        ValidateToken(TokenType.CloseParen);
                    }
                    if (Tokenizer.Token == TokenType.CloseParen)
                    {
                        NextToken(true);
                        if (nodes.Count > 1)
                            ValidateToken(TokenType.Lambda);
                        if (Tokenizer.Token == TokenType.Lambda)
                        {
                            NextToken(true);
                            for (int index = 0; index < nodes.Count; index++)
                                ValidateLambdaParameter(nodes[index]);

                            return new LambdaExpressionNode(ParseExpression(),
                                nodes.Cast<IMemberExpressionNode>().Select(expressionNode => expressionNode.Member));
                        }
                        return ParseBinaryExpression(node);
                    }
                }
                ValidateToken(TokenType.CloseParen);
                NextToken(true);
                return ParseBinaryExpression(node);
            }

            if (Tokenizer.Token == TokenType.Identifier)
            {
                IExpressionNode node = ParseExpression();
                var memberExp = node as IMemberExpressionNode;
                if (memberExp != null && memberExp.Target == null && Tokenizer.Token == TokenType.Lambda)
                {
                    NextToken(true);
                    return new LambdaExpressionNode(ParseExpression(), new[] { memberExp.Member });
                }
                return node;
            }
            return ParseExpression();
        }

        protected object ParseStringLiteral()
        {
            ValidateToken(TokenType.StringLiteral);
            char quote = Tokenizer.Value[0];
            string s = Tokenizer.Value
                                .Substring(1, Tokenizer.Value.Length - 2)
                                .Replace(@"\'", "'")
                                .Replace(@"\""", "\"");
            if (quote == '\'' && s.Length == 1)
            {
                NextToken(true);
                return s[0];
            }
            NextToken(true);
            return s;
        }

        protected object ParseIntegerLiteral()
        {
            ValidateToken(TokenType.IntegerLiteral);
            string text = Tokenizer.Value;
            object value = null;
            char last = text[text.Length - 1];
            switch (last)
            {
                case 'U':
                case 'u':
                    uint u;
                    if (uint.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out u))
                        value = u;
                    break;

                case 'L':
                case 'l':
                    char prevL = text[text.Length - 2];
                    if (prevL == 'U' || prevL == 'u')
                    {
                        ulong ul;
                        if (ulong.TryParse(text.Substring(0, text.Length - 2), NumberStyles.Any,
                            CultureInfo.InvariantCulture, out ul))
                            value = ul;
                    }
                    else
                    {
                        long l;
                        if (long.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Any,
                            CultureInfo.InvariantCulture, out l))
                            value = l;
                    }
                    break;
                default:
                    long result;
                    if (long.TryParse(text, out result))
                    {
                        if (result <= Int32.MaxValue)
                            value = (int)result;
                        else
                            value = result;
                    }
                    break;
            }
            if (value == null)
                throw BindingExceptionManager.InvalidIntegerLiteral(text, Tokenizer);
            NextToken(true);
            return value;
        }

        protected object ParseRealLiteral()
        {
            ValidateToken(TokenType.RealLiteral);
            string text = Tokenizer.Value;
            object value = null;
            char last = text[text.Length - 1];
            switch (last)
            {
                case 'f':
                case 'F':
                    float f;
                    if (float.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out f))
                        value = f;
                    break;
                case 'd':
                case 'D':
                    double d;
                    if (double.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out d))
                        value = d;
                    break;
                case 'm':
                case 'M':
                    decimal dec;
                    if (decimal.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out dec))
                        value = dec;
                    break;
                default:
                    double result;
                    if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                        value = result;
                    break;
            }
            if (value == null)
                throw BindingExceptionManager.InvalidRealLiteral(text, Tokenizer);
            NextToken(true);
            return value;
        }

        private uint ParseLevel()
        {
            if (Tokenizer.Value != LevelName)
                throw BindingExceptionManager.UnknownIdentifierParser(Tokenizer.Value, Tokenizer, Expression,
                    PathName);
            NextToken(true);
            ValidateToken(TokenType.Equal);
            NextToken(true);
            ValidateToken(TokenType.IntegerLiteral);
            uint i = uint.Parse(Tokenizer.Value);
            NextToken(true);
            return i;
        }

        private string ParsePath()
        {
            if (Tokenizer.Value != PathName)
                throw BindingExceptionManager.UnknownIdentifierParser(Tokenizer.Value, Tokenizer, Expression,
                    PathName);
            NextToken(true);
            ValidateToken(TokenType.Equal);
            NextToken(true);
            int pos = Tokenizer.Position;
            string memberName = ParsePrimary().TryGetMemberName(true, false);
            if (string.IsNullOrEmpty(memberName))
                throw BindingExceptionManager.InvalidMemberName(Expression, pos);
            return memberName;
        }

        private void ValidateLambdaParameter(IExpressionNode node)
        {
            var expressionNode = node as IMemberExpressionNode;
            if (expressionNode == null || expressionNode.Target != null)
                throw BindingExceptionManager.InvalidExpressionParser(node.ToString(), Tokenizer, Expression);
        }

        private IList<string> ParseTypeArgs()
        {
            var typeArgs = new List<string>();
            while (true)
            {
                int position = Tokenizer.Position;
                string memberName = ParsePrimary().TryGetMemberName(false, false);
                if (string.IsNullOrEmpty(memberName))
                    throw BindingExceptionManager.InvalidMemberName(Expression, position);
                typeArgs.Add(memberName);
                if (Tokenizer.Token != TokenType.Comma)
                    break;
                NextToken(true);
            }
            return typeArgs;
        }

        protected bool ReadBoolValue()
        {
            ValidateToken(NextToken(true), TokenType.Identifier);
            bool b = bool.Parse(Tokenizer.Value);
            NextToken(true);
            return b;
        }

        #endregion

        #region Initialization

        protected virtual Action<IDataContext> GetBindingSourceSetter([CanBeNull] IExpressionNode expression)
        {
            if (expression == null)
                return EmptyPathSourceDelegate;
            try
            {
                expression = expression.Accept(_memberVisitor);
                KeyValuePair<string, BindingMemberExpressionNode>[] members = _memberVisitor.Members.ToArrayEx();
                bool isEmpty = members.Length == 0;
                IExpressionInvoker invoker = null;
                IBindingValueConverter converter = null;
                string converterName = null;
                if (_memberVisitor.IsMulti)
                {
                    //NOTE: Optimizing expression to use only converter instead of compile expression.
                    if (!isEmpty)
                    {
                        int count = _counterVisitor.GetCount(expression);
                        switch (count)
                        {
                            case 2:
                                var unaryExressionNode = expression as IUnaryExressionNode;
                                if (unaryExressionNode != null && unaryExressionNode.Token == TokenType.Exclamation)
                                    converter = InverseBooleanValueConverter.Instance;
                                break;
                            case 3:
                                var methodCallExpressionNode = expression as IMethodCallExpressionNode;
                                if (methodCallExpressionNode != null &&
                                    methodCallExpressionNode.Target is ResourceExpressionNode)
                                    converterName = methodCallExpressionNode.Method;
                                break;
                        }
                    }
                    invoker = CreateExpressionInvoker(expression, members, isEmpty);
                }
                else if (isEmpty)
                    invoker = CreateExpressionInvoker(expression, members, true);

                Func<IDataContext, IBindingSource>[] bindingSource;
                if (isEmpty)
                    bindingSource = EmptyBindingSourceDelegates;
                else
                {
                    bindingSource = new Func<IDataContext, IBindingSource>[members.Length];
                    for (int i = 0; i < members.Length; i++)
                        bindingSource[i] = GetBindingSourceDelegate(members[i].Value);
                }


                return context =>
                {
                    if (converter != null || converterName != null)
                    {
                        if (!context.Contains(BindingBuilderConstants.Converter))
                        {
                            if (converter == null)
                                converter = BindingServiceProvider
                                                           .ResourceResolver
                                                           .ResolveConverter(converterName, context, false);
                            if (converter != null)
                                context.Add(BindingBuilderConstants.Converter, d => converter);
                            else if (invoker != null)
                                context.Add(BindingBuilderConstants.MultiExpression, invoker.Invoke);
                        }
                        else if (invoker != null)
                            context.Add(BindingBuilderConstants.MultiExpression, invoker.Invoke);
                    }
                    else if (invoker != null)
                        context.Add(BindingBuilderConstants.MultiExpression, invoker.Invoke);
                    context.Add(BindingBuilderConstants.Sources, bindingSource);

                    //using OneTimeBindingMode if the expression is a constant expression.
                    if (isEmpty)
                    {
                        var behaviors = context.GetOrAddBehaviors();
                        for (int i = 0; i < behaviors.Count; i++)
                        {
                            if (behaviors[i].Id == BindingModeBase.IdBindingMode)
                                return;
                        }
                        behaviors.Add(new OneTimeBindingMode(true));
                    }
                };
            }
            finally
            {
                _memberVisitor.Clear();
            }
        }

        [CanBeNull]
        protected virtual IList<Action<IDataContext>> GetBindingBehaviorSetter(string left)
        {
            Func<BindingParser, IList<Action<IDataContext>>> value;
            if (BindingParameterToAction.TryGetValue(left, out value))
                return value(this);
            NextToken(true);
            var args = new[] { GetConstantValue(ParsePrimary()) };
            return new Action<IDataContext>[]
            {
                context =>
                {
                    var behavior = BindingServiceProvider
                        .ResourceResolver
                        .ResolveBehavior(left, context, args, true);
                    context.GetOrAddBehaviors().Add(behavior);
                }
            };
        }

        private IList<Action<IDataContext>> GetCustomBehaviorSetter()
        {
            NextToken(true);
            var actions = new List<Action<IDataContext>>();
            var expressionNode = Handle(ParseMemberAccess(null), false, Context, actions);
            var memberName = expressionNode.TryGetMemberName(false, false);
            IList<object> args;
            if (memberName == null)
            {
                var method = expressionNode as IMethodCallExpressionNode;
                if (method == null)
                    throw BindingExceptionManager.InvalidExpressionParser(expressionNode.ToString(), Tokenizer, Expression);
                memberName = method.Method;
                args = new object[method.Arguments.Count];
                for (int index = 0; index < method.Arguments.Count; index++)
                    args[index] = GetConstantValue(method.Arguments[index]);
            }
            else
                args = Empty.Array<object>();
            return new Action<IDataContext>[]
            {
                context =>
                {
                    var behavior = BindingServiceProvider
                        .ResourceResolver
                        .ResolveBehavior(memberName, context, args, true);
                    context.GetOrAddBehaviors().Add(behavior);
                }
            };
        }

        private IList<Action<IDataContext>> GetToggleEnabledState()
        {
            var value = ReadBoolValue();
            return new Action<IDataContext>[]
            {
                context => context.Add(BindingBuilderConstants.ToggleEnabledState, value)
            };
        }

        private IList<Action<IDataContext>> GetBindingModeSetter()
        {
            ValidateToken(NextToken(true), TokenType.Identifier);
            string mode = Tokenizer.Value;
            IBindingBehavior behavior;
            if (!BindingServiceProvider.BindingModeToBehavior.TryGetValue(mode, out behavior))
                throw BindingExceptionManager.UnknownIdentifierParser(mode, Tokenizer, Expression, BindingServiceProvider.BindingModeToBehavior.Keys.ToArrayEx());
            NextToken(true);
            if (behavior == null)
                return null;
            return new Action<IDataContext>[]
            {
                context => context.GetOrAddBehaviors().Add(behavior.Clone())
            };
        }

        private IList<Action<IDataContext>> GetBehaviorSetter(IBindingBehavior first, IBindingBehavior second = null)
        {
            if (ReadBoolValue())
                return new Action<IDataContext>[]
                {
                    context =>
                    {
                        IList<IBindingBehavior> behaviors = context.GetOrAddBehaviors();
                        if (first != null)
                            behaviors.Add(first.Clone());
                        if (second != null)
                            behaviors.Add(second.Clone());
                    }
                };
            return null;
        }

        private IList<Action<IDataContext>> GetDefaultValueOnExceptionSetter()
        {
            NextToken(true);
            var value = GetConstantValue(ParsePrimary());
            return new Action<IDataContext>[]
            {
                context => context.GetOrAddBehaviors().Add(new DefaultValueOnExceptionBehavior(value))
            };
        }

        private IList<Action<IDataContext>> GetDelaySetter(bool isTarget)
        {
            ValidateToken(NextToken(true), TokenType.IntegerLiteral);
            uint delay = uint.Parse(Tokenizer.Value);
            Action<IDataContext> result = context => context.GetOrAddBehaviors().Add(new DelayBindingBehavior(delay, isTarget));
            NextToken(true);
            return new[] { result };
        }

        private IList<Action<IDataContext>> GetBindingValueSetter(Action<IDataContext, object> setSimpleValue,
            Action<IDataContext, Func<IDataContext, object>> setComplexValue, bool useBindingForMember)
        {
            ValidateToken(TokenType.Equal);
            NextToken(true);
            var actions = new List<Action<IDataContext>>();
            IExpressionNode node = Handle(ParseExpression(), false, Context, actions);
            if (node != null)
                actions.Add(GetBindingValueSetterMain(node, setSimpleValue, setComplexValue, useBindingForMember));
            return actions;
        }

        [NotNull]
        private Action<IDataContext> GetBindingValueSetterMain(IExpressionNode node, Action<IDataContext, object> setSimpleValue,
            Action<IDataContext, Func<IDataContext, object>> setComplexValue, bool useBindingForMember)
        {
            var constantNode = node as IConstantExpressionNode;
            if (constantNode != null)
            {
                object value = constantNode.Value;
                return context => setSimpleValue(context, value);
            }

            try
            {
                node = node.Accept(_memberVisitor);
                if (_memberVisitor.IsMulti)
                {
                    var members = _memberVisitor.Members.ToArrayEx();
                    var bindingSource = members.Length == 0
                        ? Empty.Array<Func<IDataContext, IBindingSource>>()
                        : new Func<IDataContext, IBindingSource>[members.Length];
                    for (int i = 0; i < members.Length; i++)
                        bindingSource[i] = GetBindingSourceDelegate(members[i].Value);
                    var invoker = CreateExpressionInvoker(node, members, members.Length == 0);
                    return context =>
                    {
                        var sources = bindingSource.Length == 0
                            ? Empty.Array<IBindingSource>()
                            : new IBindingSource[bindingSource.Length];
                        for (int i = 0; i < bindingSource.Length; i++)
                            sources[i] = bindingSource[i].Invoke(context);
                        setComplexValue(context, dataContext =>
                        {
                            object[] args;
                            if (sources.Length == 0)
                                args = Empty.Array<object>();
                            else
                            {
                                args = new object[sources.Length];
                                for (int i = 0; i < sources.Length; i++)
                                    args[i] = sources[i].GetCurrentValue();
                            }
                            try
                            {
                                return invoker.Invoke(dataContext, args);
                            }
                            catch (Exception e)
                            {
                                Tracer.Error(e.Message);
                                return null;
                            }
                        });
                    };
                }
                else
                {
                    if (_memberVisitor.Members.Count == 0)
                    {
                        var value = ((IConstantExpressionNode)node).Value;
                        return context => setSimpleValue(context, value);
                    }
                    var memberExp = _memberVisitor.Members[0].Value;
                    if (!useBindingForMember && !memberExp.IsRelativeSource && !memberExp.IsDynamic)
                    {
                        var path = memberExp.Path;
                        return context => setSimpleValue(context, path);
                    }

                    var srcFunc = GetBindingSourceDelegate(memberExp);
                    return context =>
                    {
                        var src = srcFunc(context);
                        setComplexValue(context, dataContext => src.GetCurrentValue());
                    };
                }
            }
            finally
            {
                _memberVisitor.Clear();
            }
        }

        private IList<Action<IDataContext>> GetConverterSetter()
        {
            return GetBindingValueSetter((context, o) =>
            {
                var converter = o as IBindingValueConverter ?? BindingServiceProvider.ResourceResolver.ResolveConverter((string)o, context, true);
                context.Add(BindingBuilderConstants.Converter, d => converter);
            }, (context, func) => context.Add(BindingBuilderConstants.Converter, d => (IBindingValueConverter)func(d)), false);
        }

        private IList<Action<IDataContext>> GetConverterParameterSetter()
        {
            return
                GetBindingValueSetter((context, o) => context.Add(BindingBuilderConstants.ConverterParameter, d => o),
                    (context, func) => context.Add(BindingBuilderConstants.ConverterParameter, func), true);
        }

        private IList<Action<IDataContext>> GetFallbackSetter()
        {
            return GetBindingValueSetter((context, o) => context.Add(BindingBuilderConstants.Fallback, d => o),
                (context, func) => context.Add(BindingBuilderConstants.Fallback, func), false);
        }

        private IList<Action<IDataContext>> GetTargetNullValueSetter()
        {
            return GetBindingValueSetter((context, o) => context.Add(BindingBuilderConstants.TargetNullValue, o),
                (context, func) => context.Add(BindingBuilderConstants.TargetNullValue, func(DataContext.Empty)), false);
        }

        private IList<Action<IDataContext>> GetCommandParameterSetter()
        {
            return GetBindingValueSetter((context, o) => context.Add(BindingBuilderConstants.CommandParameter, d => o),
                (context, func) => context.Add(BindingBuilderConstants.CommandParameter, func), true);
        }

        private IList<Action<IDataContext>> GetConverterCultureSetter()
        {
            return GetBindingValueSetter((context, o) =>
            {
                CultureInfo culture = o as CultureInfo ?? new CultureInfo(((string)o).Replace("\"", string.Empty));
                context.Add(BindingBuilderConstants.ConverterCulture, d => culture);
            }, (context, func) => context.Add(BindingBuilderConstants.ConverterCulture, d => (CultureInfo)func(d)), false);
        }

        private object GetConstantValue(IExpressionNode argument)
        {
            var node = argument as IConstantExpressionNode;
            if (node != null)
                return node.Value;
            var name = argument.TryGetMemberName(false, false);
            if (name == null)
                throw BindingExceptionManager.InvalidExpressionParser(argument.ToString(), Tokenizer, Expression);
            return name;
        }

        #endregion

        #region Bind members

        protected static Func<IDataContext, IBindingSource> GetBindingSourceDelegate(BindingMemberExpressionNode node)
        {
            IBindingPath path = BindingServiceProvider.BindingPathFactory(node.Path);
            if (node.IsDynamic)
            {
                string resourceName = node.ResourceName;
                return context =>
                {
                    var resourceObject = BindingServiceProvider.ResourceResolver.ResolveObject(resourceName, context, true);
                    return new BindingSource(BindingServiceProvider.ObserverProvider.Observe(resourceObject, path, false));
                };
            }
            if (node.IsRelativeSource)
            {
                IRelativeSourceExpressionNode r = node.RelativeSourceExpression;
                return context => BindingExtensions.CreateBindingSource(r, context.GetData(BindingBuilderConstants.Target, true), null);
            }
            return context => BindSource(context, path);
        }

        private static IBindingSource BindEmptyPathSource(IDataContext context)
        {
            return BindSource(context, BindingPath.Empty);
        }

        private static IBindingSource BindSource(IDataContext context, IBindingPath path)
        {
            object src = context.GetData(BindingBuilderConstants.Source) ??
                         BindingServiceProvider.ContextManager.GetBindingContext(
                             context.GetData(BindingBuilderConstants.Target, true),
                             context.GetData(BindingBuilderConstants.TargetPath, true).Path);
            return new BindingSource(BindingServiceProvider.ObserverProvider.Observe(src, path, false));
        }

        #endregion

        #region Internal

        /// <summary>
        /// Creates an instance of <see cref="IExpressionInvoker"/> to invoke the specified node expression.
        /// </summary>
        protected virtual IExpressionInvoker CreateExpressionInvoker(IExpressionNode expressionNode, IList<KeyValuePair<string, BindingMemberExpressionNode>> members, bool isEmpty)
        {
            return new CompiledExpressionInvoker(expressionNode, isEmpty);
        }

        /// <summary>
        ///     Gets an instance of <see cref="Tokenizer" /> to parse expression.
        /// </summary>
        protected virtual ITokenizer CreateTokenizer(string source)
        {
            _defaultTokenizer.SetSource(source);
            return _defaultTokenizer;
        }

        protected TokenType NextToken(bool ignoreWhitespace)
        {
            return Tokenizer.NextToken(ignoreWhitespace);
        }

        protected bool IsBinaryToken()
        {
            return IsAnyOf(BinaryOperationTokens.Keys) || IsAnyOf(BinaryOperationAliases.Keys);
        }

        protected bool IsAnyOf(ICollection<TokenType> collection)
        {
            return collection.Contains(Tokenizer.Token);
        }

        protected bool IsAnyOf(ICollection<string> collection)
        {
            return collection.Contains(Tokenizer.Value);
        }

        protected void ValidateToken(TokenType t)
        {
            ValidateToken(Tokenizer.Token, t);
        }

        protected void ValidateToken(TokenType current, TokenType expected)
        {
            if (expected != current)
                throw BindingExceptionManager.UnexpectedTokenParser(current, expected, Tokenizer, Expression);
        }

        protected string HandleTargetPath(string targetPath, IDataContext context)
        {
            for (int i = 0; i < _handlers.Count; i++)
                _handlers[i].HandleTargetPath(ref targetPath, context);
            return targetPath;
        }

        protected string Handle(string bindingExpression, IDataContext context)
        {
            for (int i = 0; i < _handlers.Count; i++)
                _handlers[i].Handle(ref bindingExpression, context);
            return bindingExpression;
        }

        protected IExpressionNode Handle(IExpressionNode expression, bool isPrimary, IDataContext context, List<Action<IDataContext>> actions)
        {
            for (int i = 0; i < _handlers.Count; i++)
                actions.AddIfNotNull(_handlers[i].Handle(ref expression, isPrimary, context));
            return expression;
        }

        private int GetMaxPriorityTokenIndex(List<TokenType> tokens)
        {
            if (tokens.Count == 0)
                return -1;
            if (tokens.Count == 1)
                return 0;
            int index = -1;
            int priority = int.MinValue;
            for (int i = 0; i < tokens.Count; i++)
            {
                var itemPriority = BinaryOperationTokens[tokens[i]];
                if (itemPriority > priority)
                {
                    priority = itemPriority;
                    index = i;
                }
            }
            return index;
        }

        private static int OrderByMemberPriority(KeyValuePair<KeyValuePair<string, int>, Action<IDataContext>[]> path1, KeyValuePair<KeyValuePair<string, int>, Action<IDataContext>[]> path2)
        {
            int x1;
            int x2;
            BindingServiceProvider.BindingMemberPriorities.TryGetValue(path1.Key.Key, out x1);
            BindingServiceProvider.BindingMemberPriorities.TryGetValue(path2.Key.Key, out x2);
            return x2.CompareTo(x1);
        }

        #endregion

        #endregion
    }
}