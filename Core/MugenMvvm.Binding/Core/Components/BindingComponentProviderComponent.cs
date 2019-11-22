using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingComponentProviderComponent : IBindingComponentProviderComponent, IHasPriority
    {
        #region Fields

        private readonly BindingParameterContext _context;
        private readonly List<IProvider> _providers;

        #endregion

        #region Constructors

        public BindingComponentProviderComponent()
        {
            _context = new BindingParameterContext();
            _providers = new List<IProvider>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = int.MinValue;

        public IList<IProvider> Providers => _providers;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingComponentBuilder?, IReadOnlyList<IBindingComponentBuilder>> TryGetComponentBuilders(IExpressionNode targetExpression, IExpressionNode sourceExpression,
            ItemOrList<IExpressionNode?, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
        {
            if (_providers.Count == 0)
                return default;

            _context.Clear();
            _context.Initialize(parameters);

            for (var i = 0; i < _providers.Count; i++)
                _providers[i].Initialize(targetExpression, sourceExpression, _context, metadata);

            return _context.GetComponents();
        }

        #endregion

        #region Nested types

        public interface IProvider
        {
            void Initialize(IExpressionNode targetExpression, IExpressionNode sourceExpression, BindingParameterContext context, IReadOnlyMetadataContext? metadata);
        }

        #endregion
    }
}