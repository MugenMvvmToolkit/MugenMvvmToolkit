#region Copyright

// ****************************************************************************
// <copyright file="BindingContextMock.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
