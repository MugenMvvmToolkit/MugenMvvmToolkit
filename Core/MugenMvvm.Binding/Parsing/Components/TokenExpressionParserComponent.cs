using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing.Components
{
    //todo use span/memory?
    public sealed class TokenExpressionParserComponent : AttachableComponentBase<IExpressionParser>, IExpressionParserComponent, IHasPriority,
        IComponentCollectionChangedListener<IComponent<IExpressionParser>>
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly TokenParserContext _parserContext;
        private readonly FuncEx<string, IReadOnlyMetadataContext?, ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>>> _tryParseStringDelegate;
        private ITokenParserComponent[] _parsers;

        #endregion

        #region Constructors

        public TokenExpressionParserComponent(IMetadataContextProvider? metadataContextProvider = null) //todo review input parameter usage
        {
            _metadataContextProvider = metadataContextProvider;
            _parserContext = new TokenParserContext(this);
            _tryParseStringDelegate = ParseInternal;
            _parsers = Default.EmptyArray<ITokenParserComponent>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener<IComponent<IExpressionParser>>.OnAdded(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _parsers, collection, component);
        }

        void IComponentCollectionChangedListener<IComponent<IExpressionParser>>.OnRemoved(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _parsers, component);
        }

        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> IExpressionParserComponent.TryParse<TExpression>(in TExpression expression,
            IReadOnlyMetadataContext? metadata)
        {
            if (_tryParseStringDelegate is FuncEx<TExpression, IReadOnlyMetadataContext?, ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>>> parser)
                return parser.Invoke(expression, metadata);
            return default;
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            _parsers = owner.Components.GetItems().OfType<ITokenParserComponent>().ToArray();
            owner.Components.Components.Add(this);
        }

        protected override void OnDetachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.Components.Remove(this);
            _parsers = Default.EmptyArray<ITokenParserComponent>();
        }

        private ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> ParseInternal(in string expression, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(expression, nameof(expression));
            _parserContext.Initialize(expression, metadata);
            return _parserContext.ParseExpression();
        }

        #endregion

        #region Nested types

        private sealed class TokenParserContext : ITokenParserContext
        {
            #region Fields

            private readonly TokenExpressionParserComponent _parser;
            private IMetadataContext? _metadata;
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
                    if (_metadata == null)
                        _parser._metadataContextProvider.LazyInitialize(ref _metadata, _parser);
                    return _metadata!;
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

            public IExpressionNode? TryParse(IExpressionNode? expression = null, Func<ITokenParserComponent, bool>? condition = null)
            {
                var components = _parser._parsers;
                for (var i = 0; i < components.Length; i++)
                {
                    var component = components[i];
                    if (condition != null && !condition(component))
                        continue;

                    var result = component.TryParse(this, expression);
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
                _metadata?.Clear();
                if (metadata != null && metadata.Count != 0)
                    Metadata.Merge(metadata);
                Position = 0;
                Length = source.Length;
            }

            public override string ToString()
            {
                return
                    $"Position '{Position.ToString()}' CurrentToken '{GetToken(Position)}' PrevToken '{GetToken(Position - 1)}' NextToken '{GetToken(Position + 1)}' Source '{_source}'";
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