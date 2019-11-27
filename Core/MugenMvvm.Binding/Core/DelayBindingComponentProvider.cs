using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    public sealed class DelayBindingComponentProvider : BindingComponentProviderComponent.IProvider
    {
        #region Implementation of interfaces

        public void Initialize(IExpressionNode targetExpression, IExpressionNode sourceExpression, BindingParameterContext context, IReadOnlyMetadataContext? metadata)
        {
            TryAddDelay(context, BindingParameterNameConstant.Delay);
            TryAddDelay(context, BindingParameterNameConstant.TargetDelay);
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
                var builder = parameterName == BindingParameterNameConstant.Delay
                    ? new DelegateBindingComponentBuilder<ushort>((i, _, __, ___, ____) => DelayBindingComponent.GetSource(i), parameterName, (ushort)delay.Value)
                    : new DelegateBindingComponentBuilder<ushort>((i, _, __, ___, ____) => DelayBindingComponent.GetTarget(i), parameterName, (ushort)delay.Value);
                context.ComponentBuilders[parameterName] = builder;
            }
        }

        #endregion
    }
}