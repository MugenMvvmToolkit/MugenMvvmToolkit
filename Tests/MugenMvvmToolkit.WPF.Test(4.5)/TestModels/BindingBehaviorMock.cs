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

        public Guid Id { get; set; }

        public int Priority { get; set; }

        void IBindingBehavior.Detach(IDataBinding binding)
        {
            Detach(binding);
        }

        bool IBindingBehavior.Attach(IDataBinding binding)
        {
            return Attach(binding);
        }

        public IBindingBehavior Clone()
        {
            return this;
        }

        #endregion
    }
}
