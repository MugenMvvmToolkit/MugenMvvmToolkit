using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class DataBindingMock : DisposableObject, IDataBinding
    {
        #region Properties

        public Action UpdateSource { get; set; }

        public Action UpdateTarget { get; set; }

        public Func<bool> Validate { get; set; }

        public Func<IDataContext> GetContext { get; set; }

        #endregion

        #region Implementation of IBinding

        /// <summary>
        ///     Gets the current <see cref="MugenMvvmToolkit.Interfaces.Models.IDataContext" />.
        /// </summary>
        public IDataContext Context
        {
            get { return GetContext(); }
        }

        /// <summary>
        ///     Gets the binding target accessor.
        /// </summary>
        public ISingleBindingSourceAccessor TargetAccessor { get; set; }

        /// <summary>
        ///     Gets the binding source accessor.
        /// </summary>
        public IBindingSourceAccessor SourceAccessor { get; set; }

        /// <summary>
        ///     Gets the binding behaviors.
        /// </summary>
        public ICollection<IBindingBehavior> Behaviors { get; set; }

        /// <summary>
        ///     Sends the current value back to the source.
        /// </summary>
        bool IDataBinding.UpdateSource()
        {
            UpdateSource();
            return true;
        }

        /// <summary>
        ///     Forces a data transfer from source to target.
        /// </summary>
        bool IDataBinding.UpdateTarget()
        {
            UpdateTarget();
            return true;
        }

        /// <summary>
        ///     Validates the current binding and raises the BindingException event if needed.
        /// </summary>
        bool IDataBinding.Validate()
        {
            return Validate();
        }

        /// <summary>
        ///     Occurs when the binding updates the values.
        /// </summary>
        public event EventHandler<IDataBinding, BindingEventArgs> BindingUpdated;

        /// <summary>
        ///     Occurs when an exception is not caught.
        /// </summary>
        public event EventHandler<IDataBinding, BindingExceptionEventArgs> BindingException;

        #endregion

        #region Methods

        public void RaiseBindingException(BindingExceptionEventArgs e)
        {
            var handler = BindingException;
            if (handler != null) handler(this, e);
        }

        public void RaiseBindingUpdated(BindingEventArgs e)
        {
            var handler = BindingUpdated;
            if (handler != null) handler(this, e);
        }

        #endregion
    }
}