using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class ExpressionParserComponent : AttachableComponentBase<IExpressionParser>, IExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private readonly TokenParserContext _parserContext;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ExpressionParserComponent()
        {
            _parserContext = new TokenParserContext();
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<ITokenParserComponent, TokenParserContext>((components, state, _) => state.Parsers = components, _parserContext);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.TokenParser;

        #endregion

        #region Implementation of interfaces

        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> IExpressionParserComponent.TryParse(IExpressionParser parser, object expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is string stringExpression)
            {
                _parserContext.Initialize(stringExpression, metadata);
                return _parserContext.ParseExpression();
            }

            return default;
        }

        #endregion

        #region Methods

        protected override void OnAttached(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            _componentTracker.Attach(owner, metadata);
        }

        protected override void OnDetached(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _componentTracker.Detach(owner, metadata);
        }

        #endregion
    }
}