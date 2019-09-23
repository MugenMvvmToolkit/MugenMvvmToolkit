using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing.Components
{
    public class TokenExpressionParserContextProviderComponent : AttachableComponentBase<IExpressionParser>, IExpressionParserContextProviderComponent<string>
    {
        #region Fields

        private readonly ComponentTracker<IExpressionParserComponent<ITokenExpressionParserContext>, IExpressionParser> _componentTracker;
        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly TokenExpressionParserContext _parserContext;

        public static readonly HashSet<char> TargetDelimiters = new HashSet<char> { ',', ';', ' ' };
        public static readonly HashSet<char> Delimiters = new HashSet<char> { ',', ';' };

        #endregion

        #region Constructors

        public TokenExpressionParserContextProviderComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            _parserContext = new TokenExpressionParserContext(this);
            _componentTracker = new ComponentTracker<IExpressionParserComponent<ITokenExpressionParserContext>, IExpressionParser>();
        }

        #endregion

        #region Implementation of interfaces

        public IExpressionParserContext? TryGetParserContext(in string expression, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(expression, nameof(expression));
            _parserContext.Initialize(expression, metadata);
            return _parserContext;
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IExpressionParser owner, IReadOnlyMetadataContext metadata)
        {
            _componentTracker.Attach(owner);
        }

        protected override void OnDetachedInternal(IExpressionParser owner, IReadOnlyMetadataContext metadata)
        {
            _componentTracker.Detach();
        }

        #endregion

        #region Nested types

        private sealed class TokenExpressionParserContext : ITokenExpressionParserContext
        {
            #region Fields

            private readonly TokenExpressionParserContextProviderComponent _provider;
            private IReadOnlyMetadataContext? _metadata;
            private string _source;

            #endregion

            #region Constructors

            public TokenExpressionParserContext(TokenExpressionParserContextProviderComponent provider)
            {
                _provider = provider;
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

                    Interlocked.CompareExchange(ref _metadata, _metadata.ToNonReadonly(this, _provider._metadataContextProvider), null);
                    return (IMetadataContext)_metadata!;
                }
            }

            public int Position { get; private set; }

            public int Length { get; private set; }

            #endregion

            #region Implementation of interfaces

            public ExpressionParserResult TryParseNext(IReadOnlyMetadataContext? metadata)
            {
                if (this.IsEof())
                    return default;

                var delimiterPos = this.SkipWhitespaces().FindAnyOf(TargetDelimiters);
                var length = Length;
                if (delimiterPos > 0)
                    SetLimit(delimiterPos);

                var target = this.ParseWhileAnyOf(Delimiters);
                SetLimit(length);

                IExpressionNode? source = null;
                if (this.IsToken(' '))
                    source = this.ParseWhileAnyOf(Delimiters);

                List<IExpressionNode>? parameters = null;
                IExpressionNode? parameter = null;
                while (this.IsToken(','))
                {
                    var param = this.MoveNext().ParseWhileAnyOf(Delimiters);
                    if (parameter == null)
                        parameter = param;
                    else
                    {
                        if (parameters == null)
                            parameters = new List<IExpressionNode> { parameter };
                        parameters.Add(param);
                    }
                }

                if (this.IsEof() || this.IsToken(';'))
                {
                    if (this.IsToken(';'))
                        this.MoveNext();
                    return new ExpressionParserResult(target, source, parameters ?? new ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>>(parameter!), this);
                }

                BindingExceptionManager.CannotParseExpression(this);
                return default;
            }

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

            public IExpressionNode? TryParse(IExpressionNode? expression = null, IReadOnlyMetadataContext? metadata = null)
            {
                var components = _provider._componentTracker.GetComponents();
                for (var i = 0; i < components.Length; i++)
                {
                    var result = components[i].TryParse(this, expression, metadata);
                    if (result != null)
                        return result;
                }

                return null;
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

            private void SetLimit(int? limit)
            {
                Length = limit.GetValueOrDefault(_source.Length);
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