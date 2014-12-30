#region Copyright

// ****************************************************************************
// <copyright file="BindingBehaviorBase.cs">
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
using System.Threading;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Represents the base class that allows to extend the <see cref="IDataBinding" /> by adding new features.
    /// </summary>
    public abstract class BindingBehaviorBase : IBindingBehavior
    {
        #region Fields

        private const int Attached = 2;
        private const int Attaching = 1;
        private const int Available = 0;

        private int _state;
        private IDataBinding _binding;

        #endregion

        #region Implementation of IBindingBehavior

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public abstract Guid Id { get; }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        public abstract int Priority { get; }

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        /// <param name="binding">The binding to attach to.</param>
        public bool Attach(IDataBinding binding)
        {
            if (Interlocked.Exchange(ref _state, Attaching) != Available)
                throw BindingExceptionManager.BehaviorInitialized(this);
            try
            {
                _binding = binding;
                if (OnAttached())
                {
                    _state = Attached;
                    return true;
                }
                _state = Available;
                _binding = null;
                return false;
            }
            catch (Exception)
            {
                _state = Available;
                _binding = null;
                throw;
            }
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        public void Detach(IDataBinding binding)
        {
            if (_state == Available)
                return;
            try
            {
                OnDetached();
            }
            finally
            {
                _state = Available;
                _binding = null;
            }
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        public IBindingBehavior Clone()
        {
            return CloneInternal();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the associated binding.
        /// </summary>
        protected IDataBinding Binding
        {
            get { return _binding; }
        }

        /// <summary>
        /// Gets the value that indicates that current behavior is attached to a binding.
        /// </summary>
        protected bool IsAttached
        {
            get { return _state == Attached; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected abstract bool OnAttached();

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        protected abstract void OnDetached();

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        protected abstract IBindingBehavior CloneInternal();

        #endregion
    }
}