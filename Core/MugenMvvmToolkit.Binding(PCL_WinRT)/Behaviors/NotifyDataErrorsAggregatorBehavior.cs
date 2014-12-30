#region Copyright

// ****************************************************************************
// <copyright file="NotifyDataErrorsAggregatorBehavior.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    internal class NotifyDataErrorsAggregatorBehavior : ValidatesOnNotifyDataErrorsBehavior
    {
        #region Fields

        [NotNull]
        public IList<object> Errors;

        private bool _updating;

        #endregion

        #region Overrides of ValidatesOnNotifyDataErrorsBehavior

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected override bool OnAttached()
        {
            if (base.OnAttached())
            {
                Binding.BindingException += OnBindingException;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        protected override void OnDetached()
        {
            Binding.BindingException -= OnBindingException;
            base.OnDetached();
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        protected override IBindingBehavior CloneInternal()
        {
            return new NotifyDataErrorsAggregatorBehavior();
        }

        /// <summary>
        ///     Updates the current errors.
        /// </summary>
        protected override void UpdateErrors(IList<object> errors)
        {
            Errors = errors ?? Empty.Array<object>();
            IDataBinding dataBinding = Binding;
            if (_updating || dataBinding == null || !IsAttached)
                return;
            try
            {
                _updating = true;
                dataBinding.UpdateTarget();
            }
            finally
            {
                _updating = false;
            }
        }

        /// <summary>
        ///     Defines the method that determines whether the behavior can attach to binding.
        /// </summary>
        protected override bool CanAttach()
        {
            return true;
        }

        #endregion

        #region Methods

        private void OnBindingException(IDataBinding sender, BindingExceptionEventArgs args)
        {
            UpdateErrors(new object[]
            {
                ValidatesOnExceptionsBehavior.ShowOriginalException
                    ? args.OriginalException.Message
                    : args.Exception.Message
            });
        }

        #endregion
    }
}