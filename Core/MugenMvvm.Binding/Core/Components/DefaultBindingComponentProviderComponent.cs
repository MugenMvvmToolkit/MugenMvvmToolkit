using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class DefaultBindingComponentProviderComponent : IBindingComponentProviderComponent, IHasPriority
    {
        #region Constructors

        public DefaultBindingComponentProviderComponent()
        {
            DefaultBindingComponents = new List<IBindingComponentBuilder>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.DefaultPreInitializer;

        public List<IBindingComponentBuilder> DefaultBindingComponents { get; }

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingComponentBuilder, IReadOnlyList<IBindingComponentBuilder>> TryGetComponentBuilders(IExpressionNode targetExpression, IExpressionNode sourceExpression,
            ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
        {
            return DefaultBindingComponents;
        }

        #endregion
    }
}