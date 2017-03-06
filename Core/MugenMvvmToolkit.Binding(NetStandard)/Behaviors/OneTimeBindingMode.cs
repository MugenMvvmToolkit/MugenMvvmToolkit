#region Copyright

// ****************************************************************************
// <copyright file="OneTimeBindingMode.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    public sealed class OneTimeBindingMode : BindingModeBase
    {
        #region Fields

        private readonly bool _disposeBinding;

        #endregion

        #region Constructors

        public OneTimeBindingMode()
            : this(true)
        {
        }

        public OneTimeBindingMode(bool disposeBinding)
        {
            _disposeBinding = disposeBinding;
        }

        #endregion

        #region Overrides of BindingBehaviorBase

        protected override bool OnAttached()
        {
            if (!Binding.TargetAccessor.IsAllMembersAvailable() || !Binding.SourceAccessor.IsAllMembersAvailable(true))
            {
                EventHandler<IObserver, ValueChangedEventArgs> handler = OneTimeHandler;
                SubscribeSources(handler);
                Binding.TargetAccessor.Source.ValueChanged += handler;
                return true;
            }
            Binding.UpdateTarget();
            if (_disposeBinding)
                Binding.Dispose();
            return false;
        }

        protected override void OnDetached()
        {
            UnsubscribeSources(OneTimeHandler);
        }

        protected override IBindingBehavior CloneInternal()
        {
            return new OneTimeBindingMode(_disposeBinding);
        }

        #endregion

        #region Methods

        private void OneTimeHandler(IObserver sender, ValueChangedEventArgs args)
        {
            IDataBinding binding = Binding;
            if (binding == null || !binding.TargetAccessor.IsAllMembersAvailable() || !binding.SourceAccessor.IsAllMembersAvailable(true))
                return;
            EventHandler<IObserver, ValueChangedEventArgs> handler = OneTimeHandler;
            UnsubscribeSources(handler);
            binding.TargetAccessor.Source.ValueChanged -= handler;
            binding.UpdateTarget();
            if (_disposeBinding)
                binding.Dispose();
        }

        #endregion
    }
}
