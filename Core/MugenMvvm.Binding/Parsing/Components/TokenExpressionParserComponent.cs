using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing.Components
{
    //todo use span/memory?
    public class TokenExpressionParserComponent : AttachableComponentBase<IExpressionParser>, IExpressionParserComponent<string>
    {
        #region Fields

        private readonly ComponentTracker<IParser, IExpressionParser> _componentTracker;
        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly TokenParserContext _parserContext;

        public static readonly HashSet<char> TargetDelimiters = new HashSet<char> { ',', ';', ' ' };
        public static readonly HashSet<char> Delimiters = new HashSet<char> { ',', ';' };

        #endregion

        #region Constructors

        public TokenExpressionParserComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            _parserContext = new TokenParserContext(this);
            _componentTracker = new ComponentTracker<IParser, IExpressionParser>();
        }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> TryParse(in string expression, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(expression, nameof(expression));
            return ParseInternal(expression, metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            _componentTracker.Attach(owner);
        }

        protected override void OnDetachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            _componentTracker.Detach();
        }

        protected virtual ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> ParseInternal(in string expression, IReadOnlyMetadataContext? metadata)
        {
            _parserContext.Initialize(expression, metadata);
            return ParseInternal(_parserContext);
        }

        protected ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> ParseInternal(IContext context)
        {
            ExpressionParserResult itemResult = default;
            List<ExpressionParserResult>? result = null;
            while (!context.IsEof())
            {
                var r = TryParseNext(context);
                if (r.IsEmpty)
                    break;
                if (itemResult.IsEmpty)
                    itemResult = r;
                else
                {
                    if (result == null)
                        result = new List<ExpressionParserResult> { itemResult };
                    result.Add(r);
                }
            }

            if (result == null)
                return itemResult;
            return result;
        }

        private ExpressionParserResult TryParseNext(IContext context)
        {
            var delimiterPos = context.SkipWhitespaces().FindAnyOf(TargetDelimiters);
            var length = context.Length;
            if (delimiterPos > 0)
                context.SetLimit(delimiterPos);

            var target = context.ParseWhileAnyOf(Delimiters);
            context.SetLimit(length);

            IExpressionNode? source = null;
            if (context.IsToken(' '))
                source = context.ParseWhileAnyOf(Delimiters);

            List<IExpressionNode>? parameters = null;
            IExpressionNode? parameter = null;
            while (context.IsToken(','))
            {
                var param = context.MoveNext().ParseWhileAnyOf(Delimiters);
                if (parameter == null)
                    parameter = param;
                else
                {
                    if (parameters == null)
                        parameters = new List<IExpressionNode> { parameter };
                    parameters.Add(param);
                }
            }

            if (context.IsEof() || context.IsToken(';'))
            {
                if (context.IsToken(';'))
                    context.MoveNext();
                return new ExpressionParserResult(target, source, parameters ?? new ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>>(parameter), context);
            }

            BindingExceptionManager.ThrowCannotParseExpression(this);
            return default;
        }

        #endregion

        #region Nested types

        public interface IContext : IMetadataOwner<IMetadataContext>
        {
            int Position { get; }

            int Length { get; }

            char TokenAt(int position);

            string GetValue(int start, int end);

            void SetPosition(int position);

            void SetLimit(int? limit);

            IExpressionNode? TryParse(IExpressionNode? expression = null);
        }

        public interface IParser : IComponent<IExpressionParser>
        {
            IExpressionNode? TryParse(IContext context, IExpressionNode? expression);
        }

        private sealed class TokenParserContext : IContext
        {
            #region Fields

            private readonly TokenExpressionParserComponent _parser;
            private IReadOnlyMetadataContext? _metadata;
            private string _source;

            #endregion

            #region Constructors

            public TokenParserContext(TokenExpressionParserComponent parser)
            {
                _source = string.Empty;
                _parser = parser;
            }

            #endregion

            #region Properties

            public bool HasMetadata => _metadata != null;

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata is IMetadataContext ctx)
                        return ctx;

                    Interlocked.CompareExchange(ref _metadata, _metadata.ToNonReadonly(this, _parser._metadataContextProvider), null);
                    return (IMetadataContext)_metadata!;
                }
            }

            public int Position { get; private set; }

            public int Length { get; private set; }

            #endregion

            #region Implementation of interfaces

            public char TokenAt(int position)
            {
                return _source[position];
            }

            public string GetValue(int start, int end)
            {
                return _source.Substring(start, end - start);
            }

            public void SetPosition(int position)
            {
                Position = position;
            }

            public IExpressionNode? TryParse(IExpressionNode? expression = null)
            {
                var components = _parser._componentTracker.GetComponents();
                for (var i = 0; i < components.Length; i++)
                {
                    var result = components[i].TryParse(this, expression);
                    if (result != null)
                        return result;
                }

                return null;
            }

            public void SetLimit(int? limit)
            {
                Length = limit.GetValueOrDefault(_source.Length);
            }

            #endregion

            #region Methods

            public void Initialize(string source, IReadOnlyMetadataContext? metadata)
            {
                _source = source;
                _metadata = metadata;
                Length = source.Length;
            }

            public override string ToString()
            {
                return $"Position '{Position}' CurrentToken '{GetToken(Position)}' PrevToken '{GetToken(Position - 1)}' NextToken '{GetToken(Position + 1)}' Source '{_source}'";
            }

            private string GetToken(int position)
            {
                if (this.IsEof(position))
                    return "EOF";
                if (position < 0)
                    return "BOF";
                return _source[position].ToString();
            }

            #endregion
        }

        #endregion
    }
}