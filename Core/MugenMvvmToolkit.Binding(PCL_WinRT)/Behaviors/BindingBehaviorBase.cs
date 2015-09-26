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

        public abstract Guid Id { get; }

        public abstract int Priority { get; }

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

        public IBindingBehavior Clone()
        {
            return CloneInternal();
        }

        #endregion

        #region Properties

        protected IDataBinding Binding
        {
            get { return _binding; }
        }

        protected bool IsAttached
        {
            get { return _state == Attached; }
        }

        #endregion

        #region Methods

        protected abstract bool OnAttached();

        protected abstract void OnDetached();

        protected abstract IBindingBehavior CloneInternal();

        #endregion
    }
}
