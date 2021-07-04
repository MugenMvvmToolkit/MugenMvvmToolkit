using MugenMvvm.Android.Internal;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Android.Bindings
{
    public sealed class NativeStringExpressionParser : AttachableComponentBase<IExpressionParser>, IExpressionParserComponent, IHasPriority
    {
        private readonly ComponentTracker _componentTracker;
        private readonly NativeStringTokenParserContext _parserContext;

        [Preserve(Conditional = true)]
        public NativeStringExpressionParser()
        {
            _parserContext = new NativeStringTokenParserContext();
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<ITokenParserComponent, NativeStringTokenParserContext>((components, state, _) => state.Parsers = components, _parserContext);
        }

        public int Priority { get; init; } = ParsingComponentPriority.TokenParser;

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

        ItemOrIReadOnlyList<ExpressionParserResult> IExpressionParserComponent.TryParse(IExpressionParser parser, object expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is NativeStringAccessor stringExpression)
            {
                _parserContext.Initialize(stringExpression, metadata);
                return _parserContext.ParseExpression();
            }

            return default;
        }
    }
}