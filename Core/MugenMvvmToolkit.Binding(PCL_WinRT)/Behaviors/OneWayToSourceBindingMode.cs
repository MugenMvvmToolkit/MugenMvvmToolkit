#region Copyright

// ****************************************************************************
// <copyright file="OneWayToSourceBindingMode.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Updates the source property when the target property changes.
    /// </summary>
    public sealed class OneWayToSourceBindingMode : BindingModeBase
    {
        #region Overrides of BindingBehaviorBase

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected override bool OnAttached()
        {
            SubscribeTarget();
            SubscribeSources(OneTimeSourceHandler);
            Binding.UpdateSource();
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        protected override void OnDetached()
        {
            UnsubscribeTarget();
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        protected override IBindingBehavior CloneInternal()
        {
            return new OneWayToSourceBindingMode();
        }

        #endregion

        #region Methods

        private void OneTimeSourceHandler(IBindingSource sender, ValueChangedEventArgs args)
        {
            var members = sender.GetPathMembers(false);
            if (!members.AllMembersAvailable)
                return;
            UnsubscribeSources(OneTimeSourceHandler);
            Binding.UpdateSource();
        }

        #endregion
    }
}