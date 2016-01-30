#region Copyright

// ****************************************************************************
// <copyright file="NotifyDataErrorsAggregatorBehavior.cs">
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

        public override Guid Id => _id;

        protected override bool OnAttached()
        {
            if (base.OnAttached())
            {
                Binding.BindingUpdated += OnBindingException;
                return true;
            }
            return false;
        }

        protected override void OnDetached()
        {
            Binding.BindingUpdated -= OnBindingException;
            base.OnDetached();
        }

        protected override IBindingBehavior CloneInternal()
        {
            return new NotifyDataErrorsAggregatorBehavior(_id);
        }

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

        protected override bool CanAttach()
        {
            return true;
        }

        #endregion

        #region Methods

        private void OnBindingException(IDataBinding sender, BindingEventArgs args)
        {
            if (args.Exception == null || args.OriginalException == null)
                return;
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
