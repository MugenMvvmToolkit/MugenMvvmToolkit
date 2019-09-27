using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing
{
    public sealed class ExpressionParser : ComponentOwnerBase<IExpressionParser>, IExpressionParser, IComponentOwnerAddedCallback<IComponent<IExpressionParser>>,
        IComponentOwnerRemovedCallback<IComponent<IExpressionParser>>
    {
        #region Fields

        private IExpressionParserComponent[] _parsers;

        #endregion

        #region Constructors

        public ExpressionParser(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _parsers = Default.EmptyArray<IExpressionParserComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IExpressionParser>>.OnComponentAdded(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _parsers, this, collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IExpressionParser>>.OnComponentRemoved(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _parsers, collection, component, metadata);
        }

        public ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> Parse<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata = null)
        {
            for (var i = 0; i < _parsers.Length; i++)
            {
                var component = _parsers[i] as IExpressionParserComponent<TExpression>;
                if (component == null)
                    continue;
                var result = component.TryParse(expression, metadata);
                if (!result.Item.IsEmpty || result.IsList)
                    return result;
            }

            BindingExceptionManager.ThrowCannotParseExpression(expression);
            return default;
        }

        #endregion
    }
}