#region Copyright

// ****************************************************************************
// <copyright file="OneWayBindingMode.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
    public sealed class OneWayBindingMode : BindingModeBase
    {
        #region Overrides of BindingBehaviorBase

        protected override bool OnAttached()
        {
            Binding.UpdateTarget();
            SubscribeSources();
            if (!Binding.TargetAccessor.IsAllMembersAvailable())
                Binding.TargetAccessor.Source.ValueChanged += OneTimeTargetHandler;
            return true;
        }

        protected override void OnDetached()
        {
            UnsubscribeSources();
        }

        protected override IBindingBehavior CloneInternal()
        {
            return new OneWayBindingMode();
        }

        #endregion

        #region Methods

        private void OneTimeTargetHandler(IObserver sender, ValueChangedEventArgs args)
        {
            IDataBinding binding = Binding;
            if (binding != null && binding.TargetAccessor.IsAllMembersAvailable() && binding.SourceAccessor.IsAllMembersAvailable())
            {
                binding.TargetAccessor.Source.ValueChanged -= OneTimeTargetHandler;
                binding.UpdateTarget();
            }
        }

        #endregion
    }
}
