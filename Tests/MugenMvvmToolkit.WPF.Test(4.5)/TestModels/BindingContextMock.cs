using System;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public sealed class BindingContextMock : IBindingContext
    {
        #region Fields

        private object _dataContext;

        #endregion

        #region Implementation of IBindingContext

        public object Source { get; set; }

        public bool IsAlive { get; set; }

        public object Value
        {
            get { return _dataContext; }
            set
            {
                _dataContext = value;
                OnDataContextChanged();
            }
        }

        public event EventHandler<ISourceValue, EventArgs> ValueChanged;

        #endregion

        #region Methods

        private void OnDataContextChanged()
        {
            var handler = ValueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion
    }
}