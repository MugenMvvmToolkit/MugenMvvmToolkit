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

        /// <summary>
        ///     Gets the source object.
        /// </summary>
        public object Source { get; set; }

        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                _dataContext = value;
                OnDataContextChanged();
            }
        }

        public event EventHandler<IBindingContext, EventArgs> DataContextChanged;

        #endregion

        #region Methods

        private void OnDataContextChanged()
        {
            var handler = DataContextChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion
    }
}