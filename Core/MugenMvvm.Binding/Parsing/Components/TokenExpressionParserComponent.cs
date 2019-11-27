using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
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

        private readonly TokenParserContext _parserContext;
        private readonly FuncEx<string, IReadOnlyMetadataContext?, ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>>> _tryParseStringDelegate;

        #endregion

        #region Constructors

        public TokenExpressionParserComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _parserContext = new TokenParserContext(metadataContextProvider);
            _tryParseStringDelegate = ParseInternal;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.TokenParser;

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener<IComponent<IExpressionParser>>.OnAdded(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            var parsers = _parserContext.Parsers;
            MugenExtensions.ComponentTrackerOnAdded(ref parsers, collection, component);
            _parserContext.Parsers = parsers;
        }

        void IComponentCollectionChangedListener<IComponent<IExpressionParser>>.OnRemoved(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            var parsers = _parserContext.Parsers;
            MugenExtensions.ComponentTrackerOnRemoved(ref parsers, component);
            _parserContext.Parsers = parsers;
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
            owner.ComponentTrackerInitialize(out ITokenParserComponent[] parsers);
            _parserContext.Parsers = parsers;
            owner.Components.Components.Add(this);
        }

        protected override void OnDetachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.Components.Remove(this);
            _parserContext.Parsers = Default.EmptyArray<ITokenParserComponent>();
        }

        private ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> ParseInternal(in string expression, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(expression, nameof(expression));
            _parserContext.Initialize(expression, metadata);
            return _parserContext.ParseExpression();
        }

        #endregion
    }
}