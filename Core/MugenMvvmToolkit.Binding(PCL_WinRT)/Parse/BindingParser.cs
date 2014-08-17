#region Copyright
// ****************************************************************************
// <copyright file="BindingParser.cs">
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
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Converters;
using MugenMvvmToolkit.Binding.Core;
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

        protected static readonly DataConstant<object> SourceExpressionConstant;

        private static readonly BindingSourceDelegate[] EmptyBindingSourceDelegates;
        private static readonly HashSet<string> LiteralConstants;
        private static readonly HashSet<TokenType> LiteralTokens;
        private static readonly HashSet<TokenType> ResourceTokens;
        private static readonly HashSet<TokenType> DelimeterTokens;
        private static readonly Action<IDataContext> EmptyPathSourceDelegate;

        private readonly Dictionary<string, TokenType> _binaryOperationAliases;
        private readonly Dictionary<TokenType, int> _binaryOperationTokens;
        private readonly Dictionary<string, IBindingBehavior> _bindingModeToAction;
        private readonly Dictionary<string, Func<BindingParser, IList<Action<IDataContext>>>> _bindingParameterToAction;

        private readonly Dictionary<string, Action<IDataContext>[][]> _cache;
        private readonly ExpressionCounterVisitor _counterVisitor;
        private readonly Tokenizer _defaultTokenizer;
        private readonly ICollection<string> _elementSourceAliases;
        private readonly BindingMemberVisitor _memberVisitor;
        private readonly ICollection<string> _relativeSourceAliases;
        private readonly IDictionary<string, TokenType> _unaryOperationAliases;
        private readonly ICollection<TokenType> _unaryOperationTokens;
        private readonly List<IBindingParserHandler> _handlers;

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
            SourceExpressionConstant = DataConstant.Create(() => SourceExpressionConstant, true);
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
            EmptyBindingSourceDelegates = new BindingSourceDelegate[]
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
            _cache = new Dictionary<string, Action<IDataContext>[][]>(StringComparer.Ordinal);
            _handlers = new List<IBindingParserHandler> { new DefaultBindingParserHandler() };
            _defaultTokenizer = new Tokenizer(true);
            _memberVisitor = new BindingMemberVisitor();
            _counterVisitor = new ExpressionCounterVisitor();
            _binaryOperationTokens = new Dictionary<TokenType, int>
            {
                {TokenType.Plus, 100},
                {TokenType.Minus, 100},
                {TokenType.Asterisk, 99},
                {TokenType.Slash, 99},
                {TokenType.Percent, 99},
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

            _bindingModeToAction = new Dictionary<string, IBindingBehavior>(StringComparer.OrdinalIgnoreCase)
            {
                {"Default", null},
                {"TwoWay", new TwoWayBindingMode()},
                {"OneWay", new OneWayBindingMode()},
                {"OneTime", new OneTimeBindingMode()},
                {"OneWayToSource", new OneWayToSourceBindingMode()},
                {"None", NoneBindingMode.Instance}
            };

            _bindingParameterToAction = new Dictionary<string, Func<BindingParser, IList<Action<IDataContext>>>>(StringComparer.OrdinalIgnoreCase)
            {
                {"Mode", parser => parser.GetBindingModeSetter()},
                {"M", parser => parser.GetBindingModeSetter()},
                {"ValidatesOnNotifyDataErrors", parser => parser.GetBehaviorSetter(ValidatesOnNotifyDataErrorsBehavior.Prototype)},
                {"ValidatesOnErrors", parser => parser.GetBehaviorSetter(ValidatesOnNotifyDataErrorsBehavior.Prototype)},
                {"ValidatesOnExceptions", parser => parser.GetBehaviorSetter(ValidatesOnExceptionsBehavior.Instance)},
                {"Validate", parser => parser.GetBehaviorSetter(ValidatesOnNotifyDataErrorsBehavior.Prototype, ValidatesOnExceptionsBehavior.Instance)},
                {"DefaultValueOnException", parser => parser.GetBehaviorSetter(DefaultValueOnExceptionBehavior.Instance) },
                {"SetDefaultValue", parser => parser.GetBehaviorSetter(DefaultValueOnExceptionBehavior.Instance)},
                {"Delay", parser => parser.GetDelaySetter()},
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
        /// <returns>A set of instances of <see cref="IDataContext" />.</returns>
        public IList<IDataContext> Parse(string bindingExpression, IDataContext context)
        {
            Should.NotBeNullOrWhitespace(bindingExpression, "bindingExpression");
            Action<IDataContext>[][] value;
            lock (_cache)
            {
                if (!_cache.TryGetValue(bindingExpression, out value))
                {
                    try
                    {
                        _context = context;
                        _expression = Handle(bindingExpression, context);
                        _tokenizer = CreateTokenizer(Expression);
                        value = ParseInternal().ToArrayFast();
                        _cache[bindingExpression] = value;
                    }
                    finally
                    {
                        _tokenizer = null;
                        _expression = null;
                        _context = null;
                    }
                }
            }
            IList<object> sources = context.GetData(BindingBuilderConstants.RawSources);
            var result = new IDataContext[value.Length];
            bool hasSources = sources != null && sources.Count > 0;
            if (hasSources)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    object src = sources[i];
                    var dataContext = new DataContext();
                    if (src != null)
                        dataContext.Add(SourceExpressionConstant, src);
                    IList<Action<IDataContext>> actions = value[i];
                    for (int j = 0; j < actions.Count; j++)
                        actions[j].Invoke(dataContext);
                    result[i] = dataContext;
                }
            }
            else
            {
                for (int i = 0; i < value.Length; i++)
                {
                    IList<Action<IDataContext>> actions = value[i];
                    var dataContext = new DataContext();
                    for (int j = 0; j < actions.Count; j++)
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

        protected Dictionary<string, IBindingBehavior> BindingModeToAction
        {
            get { return _bindingModeToAction; }
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
        protected virtual IList<Action<IDataContext>[]> ParseInternal()
        {
            NextToken(true);
            var list = new List<Action<IDataContext>[]>(1);
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

        private Action<IDataContext>[] Parse()
        {
            var actions = new List<Action<IDataContext>>();
            ValidateToken(TokenType.Identifier);
            _parsingTarget = true;
            IExpressionNode target = ParsePrimary();
            _parsingTarget = false;
            string targetPath = target.TryGetMemberName(true, false);
            if (string.IsNullOrEmpty(targetPath))
                throw BindingExceptionManager.InvalidExpressionParser(target.ToString(), Tokenizer, Expression);

            actions.Add(context => context.Add(BindingBuilderConstants.TargetPath, BindingPath.Create(targetPath)));
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
                var setters = GetBindingParameterSetter(left);
                if (setters != null)
                {
                    for (int i = 0; i < setters.Count; i++)
                        actions.AddIfNotNull(setters[i]);
                }
            }
            source = Handle(source, true, Context, actions);
            actions.Add(GetBindingSourceSetter(source));
            return actions.ToArray();
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
                return new RelativeSourceExpressionNode(memberName, false);
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
                expression = new RelativeSourceExpressionNode(path, true);
            else if (isRelativeSource)
                expression = new RelativeSourceExpressionNode(typeName, level, path);
            else
                //Element source
                expression = new RelativeSourceExpressionNode(typeName, path);
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
                value = true;
                NextToken(true);
            }
            else if (Tokenizer.Value.Equals(FalseLiteral, StringComparison.OrdinalIgnoreCase))
            {
                value = false;
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
                return EmptyValue<IExpressionNode>.ArrayInstance;
            }
            ValidateToken(TokenType.OpenParen);
            NextToken(true);
            IList<IExpressionNode> args = Tokenizer.Token != TokenType.CloseParen
                ? ParseArguments()
                : EmptyValue<IExpressionNode>.ArrayInstance;
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
            string s = Tokenizer.Value.Substring(1, Tokenizer.Value.Length - 2);
            int start = 0;
            while (true)
            {
                int i = s.IndexOf(quote, start);
                if (i < 0) break;
                s = s.Remove(i, 1);
                start = i + 1;
            }
            if (quote == '\'')
            {
                if (s.Length != 1)
                    throw BindingExceptionManager.InvalidCharacterLiteral(Tokenizer);
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

        private IList<string> GetMemberNames(IList<IExpressionNode> nodes)
        {
            var args = new string[nodes.Count];
            for (int index = 0; index < nodes.Count; index++)
            {
                var expressionNode = nodes[index] as IMemberExpressionNode;
                if (expressionNode == null || expressionNode.Target != null)
                    throw BindingExceptionManager.InvalidExpressionParser(nodes[index].ToString(), Tokenizer, Expression);
                args[index] = expressionNode.Member;
            }
            return args;
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
                KeyValuePair<string, BindingMemberExpressionNode>[] members = _memberVisitor.Members.ToArrayFast();
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
                                    converter = InverseBooleanConverterCore.Instance;
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

                IList<BindingSourceDelegate> bindingSource;
                if (isEmpty)
                    bindingSource = EmptyBindingSourceDelegates;
                else
                {
                    bindingSource = new BindingSourceDelegate[members.Length];
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
                                converter = BindingProvider.Instance
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
                };
            }
            finally
            {
                _memberVisitor.Clear();
            }
        }

        [CanBeNull]
        protected virtual IList<Action<IDataContext>> GetBindingParameterSetter(string left)
        {
            Func<BindingParser, IList<Action<IDataContext>>> value;
            if (BindingParameterToAction.TryGetValue(left, out value))
                return value(this);
            if (ReadBoolValue())
                return new Action<IDataContext>[]
                {
                    context =>
                    {
                        var behavior = BindingProvider.Instance
                            .ResourceResolver
                            .ResolveBehavior(left, context, EmptyValue<string>.ListInstance, true);
                        context.GetOrAddBehaviors().Add(behavior);
                    }
                };
            return null;
        }

        private IList<Action<IDataContext>> GetCustomBehaviorSetter()
        {
            NextToken(true);
            var actions = new List<Action<IDataContext>>();
            var expressionNode = Handle(ParseMemberAccess(null), false, Context, actions);
            var memberName = expressionNode.TryGetMemberName(false, false);
            IList<string> args;
            if (memberName == null)
            {
                var method = expressionNode as IMethodCallExpressionNode;
                if (method == null)
                    throw BindingExceptionManager.InvalidExpressionParser(expressionNode.ToString(), Tokenizer, Expression);
                memberName = method.Method;
                args = GetMemberNames(method.Arguments);
            }
            else
                args = EmptyValue<string>.ListInstance;
            return new Action<IDataContext>[]
            {
                context =>
                {
                    var behavior = BindingProvider.Instance
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
            if (!BindingModeToAction.TryGetValue(mode, out behavior))
                throw BindingExceptionManager.UnknownIdentifierParser(mode, Tokenizer, Expression,
                    BindingModeToAction.Keys.ToArrayFast());
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

        private IList<Action<IDataContext>> GetDelaySetter()
        {
            ValidateToken(NextToken(true), TokenType.IntegerLiteral);
            uint delay = uint.Parse(Tokenizer.Value);
            Action<IDataContext> result = context => context.GetOrAddBehaviors().Add(new DelayBindingBehavior(delay));
            NextToken(true);
            return new[] { result };
        }

        private IList<Action<IDataContext>> GetBindingValueSetter(Action<IDataContext, object> setSimpleValue,
            Action<IDataContext, Func<IDataContext, object>> setComplexValue, bool useBindingForMember)
        {
            ValidateToken(TokenType.Equal);
            NextToken(true);
            var actions = new List<Action<IDataContext>>();
            IExpressionNode node = Handle(ParsePrimary(), false, Context, actions);
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

            var nodes = new List<IExpressionNode>();
            var members = new List<string>();
            string memberName = node.TryGetMemberName(true, true, nodes, members);
            var resourceExpression = nodes[0] as ResourceExpressionNode;

            if (memberName != null)
            {
                if (resourceExpression != null)
                {
                    if (resourceExpression.Dynamic)
                        return context => setComplexValue(context, d => GetResourceObject(memberName, d));
                    return context => setSimpleValue(context, GetResourceObject(memberName, context));
                }
                if (!useBindingForMember)
                    return context => setSimpleValue(context, memberName);

                node = new RelativeSourceExpressionNode(memberName, false);
            }

            var methodCall = nodes[0] as IMethodCallExpressionNode;
            if (methodCall == null)
            {
                var relativeSrc = node as IRelativeSourceExpressionNode;
                if (relativeSrc == null)
                    throw BindingExceptionManager.UnknownIdentifierParser(node.ToString(), Tokenizer, Expression);

                return context =>
                {
                    var relativeSource = new RelativeSourceBehavior(relativeSrc);
                    context.GetOrAddBehaviors().Add(relativeSource);
                    setComplexValue(context, d => relativeSource.Value);
                };
            }

            resourceExpression = methodCall.Target as ResourceExpressionNode;
            if (resourceExpression == null || methodCall.Arguments.Any(expressionNode => !(expressionNode is IConstantExpressionNode)))
                throw BindingExceptionManager.UnknownIdentifierParser(node.ToString(), Tokenizer, Expression);

            var method = methodCall.Method;
            var args = methodCall.Arguments.ToArrayFast(ex => ((IConstantExpressionNode)ex).Value);
            var memberPath = members.Count == 0 ? null : string.Join(".", members);

            if (resourceExpression.Dynamic)
                return context => setComplexValue(context, d => InvokeMethod(d, method, args, memberPath));
            return context => setSimpleValue(context, InvokeMethod(context, method, args, memberPath));
        }

        private static object GetResourceObject(string name, IDataContext context)
        {
            return BindingProvider.Instance.ResourceResolver.ResolveObject(name, context, true).Value;
        }

        private static object InvokeMethod(IDataContext context, string methodName, object[] args, string memberPath)
        {
            var method = BindingProvider.Instance.ResourceResolver.ResolveMethod(methodName, context, true);
            var result = method.Invoke(EmptyValue<Type>.ListInstance, args, context);
            if (memberPath == null)
                return result;
            return BindingExtensions.GetValueFromPath(result, memberPath);
        }

        private IList<Action<IDataContext>> GetConverterSetter()
        {
            return GetBindingValueSetter((context, o) =>
            {
                var converter = o as IBindingValueConverter ?? BindingProvider.Instance.ResourceResolver.ResolveConverter((string)o, context, true);
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

        #endregion

        #region Bind members

        protected static BindingSourceDelegate GetBindingSourceDelegate(BindingMemberExpressionNode node)
        {
            IBindingPath path = BindingPath.Create(node.Path);
            if (node.IsDynamic)
            {
                string resourceName = node.ResourceName;
                return (provider, context) =>
                {
                    var resourceObject = provider.ResourceResolver.ResolveObject(resourceName, context, true);
                    return new BindingSource(provider.ObserverProvider.Observe(resourceObject, path, false));
                };
            }
            if (node.IsRelativeSource)
            {
                IRelativeSourceExpressionNode r = node.RelativeSourceExpression;
                return (provider, context) =>
                {
                    var relativeSource = new RelativeSourceBehavior(r);
                    object target = context.GetData(BindingBuilderConstants.Target, true);
                    relativeSource.UpdateSource(target);
                    return relativeSource.BindingSource;
                };
            }
            return (provider, context) =>
            {
                var src = context.GetData(SourceExpressionConstant) ??
                          provider.GetBindingContext(context.GetData(BindingBuilderConstants.Target, true),
                              context.GetData(BindingBuilderConstants.TargetPath, true).Path);
                return new BindingSource(provider.ObserverProvider.Observe(src, path, false));
            };
        }

        private static IBindingSource BindEmptyPathSource(IBindingProvider provider, IDataContext context)
        {
            object src = context.GetData(SourceExpressionConstant) ??
                         provider.GetBindingContext(context.GetData(BindingBuilderConstants.Target, true),
                             context.GetData(BindingBuilderConstants.TargetPath, true).Path);
            return new BindingSource(provider.ObserverProvider.Observe(src, BindingPath.Empty, false));
        }

        #endregion

        #region Internal

        /// <summary>
        /// Creates an instance of <see cref="IExpressionInvoker"/> to invoke the specified node expression.
        /// </summary>
        protected virtual IExpressionInvoker CreateExpressionInvoker(IExpressionNode expressionNode, IList<KeyValuePair<string, BindingMemberExpressionNode>> members, bool isEmpty)
        {
            return new CompileExpressionInvoker(expressionNode, members, isEmpty);
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

        #endregion

        #endregion
    }
}