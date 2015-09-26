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
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    public sealed class OneWayToSourceBindingMode : BindingModeBase
    {
        #region Overrides of BindingBehaviorBase

        protected override bool OnAttached()
        {
            Binding.UpdateSource();
            SubscribeTarget();
            if (!Binding.SourceAccessor.IsAllMembersAvailable())
                SubscribeSources(OneTimeSourceHandler);
            return true;
        }

        protected override void OnDetached()
        {
            UnsubscribeTarget();
        }

        protected override IBindingBehavior CloneInternal()
        {
            return new OneWayToSourceBindingMode();
        }

        #endregion

        #region Methods

        private void OneTimeSourceHandler(IObserver sender, ValueChangedEventArgs args)
        {
            IDataBinding binding = Binding;
            if (binding != null && binding.SourceAccessor.IsAllMembersAvailable() && binding.TargetAccessor.IsAllMembersAvailable())
            {
                UnsubscribeSources(OneTimeSourceHandler);
                binding.UpdateSource();
            }
        }

        #endregion
    }
}
