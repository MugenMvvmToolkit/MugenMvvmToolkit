using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingModeInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        [Preserve(Conditional = true)]
        public BindingModeInitializer()
        {
            BindingModes = new Dictionary<string, object?>
            {
                {BindingModeNameConstant.None, null},
                {BindingModeNameConstant.OneTime, OneTimeBindingMode.Instance},
                {BindingModeNameConstant.OneWay, OneWayBindingMode.Instance},
                {BindingModeNameConstant.OneWayToSource, OneWayToSourceBindingMode.Instance},
                {BindingModeNameConstant.TwoWay, TwoWayBindingMode.Target},
                {BindingModeNameConstant.TwoWayToSource, TwoWayBindingMode.Source}
            };
            DefaultMode = OneWayBindingMode.Instance;
        }

        public object? DefaultMode { get; set; }

        public Dictionary<string, object?> BindingModes { get; }

        public int Priority { get; init; } = BindingComponentPriority.ParameterPostInitializer;

        public void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            if (context.Components.TryGetValue(BindingParameterNameConstant.Mode, out var value))
            {
                if (value == null)
                    context.Components.Remove(BindingParameterNameConstant.Mode);
                return;
            }

            var modeName = context.TryGetParameterValue<string>(BindingParameterNameConstant.Mode);
            if (modeName != null)
            {
                if (BindingModes.TryGetValue(modeName, out var mode))
                {
                    if (mode != null)
                        context.Components[BindingParameterNameConstant.Mode] = mode;
                }
                else
                    ExceptionManager.ThrowCannotParseBindingParameter(BindingParameterNameConstant.Mode, string.Join(",", BindingModes.Keys), modeName);

                return;
            }

            foreach (var mode in BindingModes)
            {
                if (context.TryGetParameterValue<bool>(mode.Key))
                {
                    if (mode.Value != null)
                        context.Components[BindingParameterNameConstant.Mode] = mode.Value;
                    return;
                }
            }

            if (DefaultMode != null)
                context.Components[BindingParameterNameConstant.Mode] = DefaultMode;
        }
    }
}