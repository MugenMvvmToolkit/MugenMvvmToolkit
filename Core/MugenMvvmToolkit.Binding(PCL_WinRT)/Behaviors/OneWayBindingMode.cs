#region Copyright
// ****************************************************************************
// <copyright file="OneWayBindingMode.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************
#endregion
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Updates the binding target (target) property when the binding source (source) changes. This type of binding is
    ///     appropriate if the control being bound is implicitly read-only. For instance, you may bind to a source such as a
    ///     stock ticker. Or perhaps your target property has no control interface provided for making changes, such as a
    ///     data-bound background color of a table. If there is no need to monitor the changes of the target property, using
    ///     the OneWay binding mode avoids the overhead of the TwoWay binding mode.
    /// </summary>
    public sealed class OneWayBindingMode : BindingModeBase
    {
        #region Overrides of BindingBehaviorBase

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected override bool OnAttached()
        {
            SubscribeSources();
            Binding.UpdateTarget();
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        protected override void OnDetached()
        {
            UnsubscribeSources();
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        protected override IBindingBehavior CloneInternal()
        {
            return new OneWayBindingMode();
        }

        #endregion
    }
}