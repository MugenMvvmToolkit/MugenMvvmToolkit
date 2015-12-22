#region Copyright

// ****************************************************************************
// <copyright file="LostFocusUpdateSourceBehavior.cs">
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

using System;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    public sealed class LostFocusUpdateSourceBehavior : BindingBehaviorBase, IEventListener
    {
        #region Fields

        public static readonly Guid IdLostFocusUpdateSourceBehavior;

        private IDisposable _subscriber;
        private IBindingMemberInfo _member;

        #endregion

        #region Constructors

        static LostFocusUpdateSourceBehavior()
        {
            IdLostFocusUpdateSourceBehavior = new Guid("9C7F0186-64A6-47F8-B0D0-1420A890B3BE");
        }

        #endregion

        #region Overrides of BindingBehaviorBase

        public override Guid Id => IdLostFocusUpdateSourceBehavior;

        public override int Priority => 0;

        protected override bool OnAttached()
        {
            object value = Binding.TargetAccessor.Source.GetPathMembers(false).PenultimateValue;
            if (value == null || value.IsUnsetValue())
                return false;
            _member = BindingServiceProvider
                                     .MemberProvider
                                     .GetBindingMember(value.GetType(), AttachedMemberConstants.Focused, false, false);

            if (_member == null)
                return false;
            _subscriber = _member.TryObserve(value, this);
            if (_subscriber == null)
                return false;
            Binding.SourceAccessor.ValueChanging += SourceOnValueChanging;
            return true;
        }

        protected override void OnDetached()
        {
            Binding.SourceAccessor.ValueChanging -= SourceOnValueChanging;
            if (_subscriber != null)
                _subscriber.Dispose();
            _subscriber = null;
            _member = null;
        }

        protected override IBindingBehavior CloneInternal()
        {
            return new LostFocusUpdateSourceBehavior();
        }

        #endregion

        #region Methods

        private void SourceOnValueChanging(IBindingSourceAccessor sender, ValueAccessorChangingEventArgs args)
        {
            if (args.Cancel)
                return;
            object value = Binding.TargetAccessor.Source.GetPathMembers(false).PenultimateValue;
            if (value != null && !value.IsUnsetValue())
                args.Cancel = (bool)_member.GetValue(value, null);
        }

        private bool OnLostFocus()
        {
            var binding = Binding;
            if (binding == null)
                return false;
            binding.UpdateSource();
            return true;
        }

        #endregion

        #region Implementation of IEventListener

        bool IEventListener.IsAlive => Binding != null;

        bool IEventListener.IsWeak => false;

        bool IEventListener.TryHandle(object sender, object message)
        {
            return OnLostFocus();
        }

        #endregion
    }
}
