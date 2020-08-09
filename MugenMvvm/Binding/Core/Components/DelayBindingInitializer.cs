using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class DelayBindingInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.BindingParameterPostInitializer;

        #endregion

        #region Implementation of interfaces

        public void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            TryAddDelay(context, BindingParameterNameConstant.Delay);
            TryAddDelay(context, BindingParameterNameConstant.TargetDelay);
        }

        #endregion

        #region Methods

        private static void TryAddDelay(IBindingExpressionInitializerContext context, string parameterName)
        {
            if (context.BindingComponents.ContainsKey(parameterName))
                return;

            var delay = context.TryGetParameterValue<int?>(parameterName, null);
            if (delay != null)
            {
                var builder = parameterName == BindingParameterNameConstant.Delay
                    ? new DelegateBindingComponentProvider<ushort>((i, _, __, ___, ____) => DelayBindingComponent.GetSource(i), (ushort) delay.Value)
                    : new DelegateBindingComponentProvider<ushort>((i, _, __, ___, ____) => DelayBindingComponent.GetTarget(i), (ushort) delay.Value);
                context.BindingComponents[parameterName] = builder;
            }
        }

        #endregion
    }
}