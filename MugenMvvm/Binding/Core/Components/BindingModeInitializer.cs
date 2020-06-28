﻿using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingModeInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public BindingModeInitializer()
        {
            BindingModes = new Dictionary<string, object?>
            {
                {BindingModeNameConstant.None, null},
                {BindingModeNameConstant.OneTime, OneTimeBindingMode.Instance},
                {BindingModeNameConstant.OneWay, OneWayBindingMode.Instance},
                {BindingModeNameConstant.OneWayToSource, OneWayToSourceBindingMode.Instance},
                {BindingModeNameConstant.TwoWay, TwoWayBindingMode.Instance}
            };
            DefaultMode = OneWayBindingMode.Instance;
        }

        #endregion

        #region Properties

        public object? DefaultMode { get; set; }

        public Dictionary<string, object?> BindingModes { get; }

        public int Priority { get; set; } = BindingComponentPriority.BindingParameterInitializer;

        #endregion

        #region Implementation of interfaces

        public void Initialize(IBindingExpressionInitializerContext context)
        {
            if (context.BindingComponents.TryGetValue(BindingParameterNameConstant.Mode, out var value))
            {
                if (value == null)
                    context.BindingComponents.Remove(BindingParameterNameConstant.Mode);
                return;
            }

            var modeName = context.TryGetParameterValue<string>(BindingParameterNameConstant.Mode);
            if (modeName != null)
            {
                if (BindingModes.TryGetValue(modeName, out var mode))
                {
                    if (mode != null)
                        context.BindingComponents[BindingParameterNameConstant.Mode] = mode;
                }
                else
                    BindingExceptionManager.ThrowCannotParseBindingParameter(BindingParameterNameConstant.Mode, string.Join(",", BindingModes.Keys), modeName);
                return;
            }

            foreach (var mode in BindingModes)
            {
                if (context.TryGetParameterValue<bool>(mode.Key))
                {
                    if (mode.Value != null)
                        context.BindingComponents[BindingParameterNameConstant.Mode] = mode.Value;
                    return;
                }
            }

            if (DefaultMode != null)
                context.BindingComponents[BindingParameterNameConstant.Mode] = DefaultMode;
        }

        #endregion
    }
}