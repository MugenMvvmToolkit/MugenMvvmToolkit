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

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    internal class NotifyDataErrorsAggregatorBehavior : ValidatesOnNotifyDataErrorsBehavior
    {
        #region Fields

        [NotNull]
        public IList<object> Errors;

        private bool _updating;
        private readonly Guid _id;

        #endregion

        #region Constructors

        public NotifyDataErrorsAggregatorBehavior(Guid id)
        {
            _id = id;
        }

        #endregion

        #region Overrides of ValidatesOnNotifyDataErrorsBehavior

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public override Guid Id
        {
            get { return _id; }
        }

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
            return new NotifyDataErrorsAggregatorBehavior(_id);
        }

        /// <summary>
        ///     Updates the current errors.
        /// </summary>
        protected override void UpdateErrors(IList<object> errors, IDataContext context)
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
            }, null);
        }

        #endregion
    }
}