using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class DelayBindingInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        public int Priority { get; set; } = BindingComponentPriority.ParameterPostInitializer;

        private static void TryAddDelay(IBindingExpressionInitializerContext context, string parameterName)
        {
            if (context.Components.ContainsKey(parameterName))
                return;

            var delay = context.TryGetParameterValue<int?>(parameterName);
            if (delay != null)
            {
                var builder = parameterName == BindingParameterNameConstant.Delay
                    ? new DelegateBindingComponentProvider<ushort>((i, _, __, ___, ____) => DelayBindingHandler.GetSource(i), (ushort) delay.Value)
                    : new DelegateBindingComponentProvider<ushort>((i, _, __, ___, ____) => DelayBindingHandler.GetTarget(i), (ushort) delay.Value);
                context.Components[parameterName] = builder;
            }
        }

        public void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            TryAddDelay(context, BindingParameterNameConstant.Delay);
            TryAddDelay(context, BindingParameterNameConstant.TargetDelay);
        }
    }
}