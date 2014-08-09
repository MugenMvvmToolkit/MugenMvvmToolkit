using System;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class BindingBehaviorMock : IBindingBehavior
    {
        #region Properties

        public Action<IDataBinding> Detach { get; set; }

        public Func<IDataBinding, bool> Attach { get; set; }

        #endregion

        #region Implementation of IBindingBehavior

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        void IBindingBehavior.Detach(IDataBinding binding)
        {
            Detach(binding);
        }

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        /// <param name="binding">The binding to attach to.</param>
        bool IBindingBehavior.Attach(IDataBinding binding)
        {
            return Attach(binding);
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        public IBindingBehavior Clone()
        {
            return this;
        }

        #endregion
    }
}