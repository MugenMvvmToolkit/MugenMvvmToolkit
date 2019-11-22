using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    public sealed class DelayBindingComponentProvider : BindingComponentProviderComponent.IProvider
    {
        #region Implementation of interfaces

        public void Initialize(IExpressionNode targetExpression, IExpressionNode sourceExpression, BindingParameterContext context, IReadOnlyMetadataContext? metadata)
        {
            TryAddDelay(context, BindingParameterNameConstants.Delay);
            TryAddDelay(context, BindingParameterNameConstants.TargetDelay);
        }

        #endregion

        #region Methods

        private static void TryAddDelay(BindingParameterContext context, string parameterName)
        {
            if (context.ComponentBuilders.ContainsKey(parameterName))
                return;

            var delay = context.TryGetValue<int?>(parameterName, null);
            if (delay != null)
            {
                var builder = parameterName == BindingParameterNameConstants.Delay
                    ? new DelegateBindingComponentBuilder<int>((i, _, __, ___, ____) => DelayBindingComponent.GetSource(i), parameterName, delay.Value)
                    : new DelegateBindingComponentBuilder<int>((i, _, __, ___, ____) => DelayBindingComponent.GetTarget(i), parameterName, delay.Value);
                context.ComponentBuilders[parameterName] = builder;
            }
        }

        #endregion
    }
}