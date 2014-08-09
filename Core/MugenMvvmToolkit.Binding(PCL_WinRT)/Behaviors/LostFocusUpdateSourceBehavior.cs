#region Copyright
// ****************************************************************************
// <copyright file="LostFocusUpdateSourceBehavior.cs">
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
using System;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Represents the binding behavior that allows to update binding source whenever the binding target element loses
    ///     focus.
    /// </summary>
    public sealed class LostFocusUpdateSourceBehavior : BindingBehaviorBase, IEventListener
    {
        #region Fields

        /// <summary>
        ///     Gets the id of behavior.
        /// </summary>
        public static readonly Guid IdLostFocusUpdateSourceBehavior = new Guid("9C7F0186-64A6-47F8-B0D0-1420A890B3BE");

        private IDisposable _subscriber;
        private IBindingMemberInfo _member;

        #endregion

        #region Overrides of BindingBehaviorBase

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public override Guid Id
        {
            get { return IdLostFocusUpdateSourceBehavior; }
        }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        public override int Priority
        {
            get { return 0; }
        }

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected override bool OnAttached()
        {
            object value = Binding.TargetAccessor.Source.GetPathMembers(false).PenultimateValue;
            if (value == null || value.IsUnsetValue())
                return false;
            _member = BindingProvider.Instance.MemberProvider.GetBindingMember(value.GetType(),
                AttachedMemberConstants.Focused, false, false);

            if (_member == null)
                return false;
            _subscriber = _member.TryObserveMember(value, this);
            if (_subscriber == null)
                return false;
            Binding.SourceAccessor.ValueChanging += SourceOnValueChanging;
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        protected override void OnDetached()
        {
            Binding.SourceAccessor.ValueChanging -= SourceOnValueChanging;
            if (_subscriber != null)
                _subscriber.Dispose();
            _subscriber = null;
            _member = null;
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
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
                args.Cancel = (bool) _member.GetValue(value, null);
        }

        private void OnLostFocus()
        {
            var binding = Binding;
            if (binding != null)
                binding.UpdateSource();
        }

        #endregion

        #region Implementation of IEventListener

        /// <summary>
        ///     Handles the message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        void IEventListener.Handle(object sender, object message)
        {
            OnLostFocus();
        }

        #endregion
    }
}