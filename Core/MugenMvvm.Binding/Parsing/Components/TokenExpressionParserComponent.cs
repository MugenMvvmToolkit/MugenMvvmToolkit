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
    public sealed class TokenExpressionParserComponent : ComponentTrackerBase<IExpressionParser, ITokenParserComponent>, IExpressionParserComponent, IHasPriority
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

        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> IExpressionParserComponent.TryParse<TExpression>(in TExpression expression,
            IReadOnlyMetadataContext? metadata)
        {
            if (_tryParseStringDelegate is FuncEx<TExpression, IReadOnlyMetadataContext?, ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>>> parser)
                return parser.Invoke(expression, metadata);
            return default;
        }

        #endregion

        #region Methods

        protected override void OnComponentAdded(IComponentCollection<IComponent<IExpressionParser>> collection, IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            base.OnComponentAdded(collection, component, metadata);
            _parserContext.Parsers = Components;
        }

        protected override void OnComponentRemoved(IComponentCollection<IComponent<IExpressionParser>> collection, IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            base.OnComponentRemoved(collection, component, metadata);
            _parserContext.Parsers = Components;
        }

        protected override void OnAttachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            _parserContext.Parsers = Components;
        }

        protected override void OnDetachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            _parserContext.Parsers = Components;
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