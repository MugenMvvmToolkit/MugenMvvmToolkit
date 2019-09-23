using System;
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

        private IExpressionParserContextProviderComponent[] _contextProviders;

        #endregion

        #region Constructors

        public ExpressionParser(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _contextProviders = Default.EmptyArray<IExpressionParserContextProviderComponent>();
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IExpressionParser>>.OnComponentAdded(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _contextProviders, this, collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IExpressionParser>>.OnComponentRemoved(IComponentCollection<IComponent<IExpressionParser>> collection,
            IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _contextProviders, collection, component, metadata);
        }

        public ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> Parse<TExpression>(in TExpression expression, IReadOnlyMetadataContext metadata)
        {
            ExpressionParserResult itemResult = default;
            List<ExpressionParserResult>? result = null;
            var context = GetParserContext(expression, metadata);
            while (true)
            {
                var r = context.TryParse(metadata);
                if (r.IsEmpty)
                    break;
                if (itemResult.IsEmpty)
                    itemResult = r;
                else
                {
                    if (result == null)
                        result = new List<ExpressionParserResult> {itemResult};
                    result.Add(r);
                }
            }

            if (result == null)
            {
                if (itemResult.IsEmpty)
                    throw new Exception(); //todo cannot parse exception
                return itemResult;
            }

            return result;
        }

        #endregion

        #region Methods

        private IExpressionParserContext GetParserContext<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            for (var i = 0; i < _contextProviders.Length; i++)
            {
                var context = (_contextProviders[i] as IExpressionParserContextProviderComponent<TExpression>)?.TryGetParserContext(expression, metadata);
                if (context != null)
                    return context;
            }

            throw new Exception(); //todo cannot parse exception
        }

        #endregion
    }
}