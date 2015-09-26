#region Copyright

// ****************************************************************************
// <copyright file="BindingResourceObject.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    public class BindingResourceObject : ISourceValue
    {
        #region Fields

        private object _value;
        private readonly bool _isWeak;

        #endregion

        #region Constructors

        public BindingResourceObject(WeakReference value)
        {
            _isWeak = true;
            _value = value ?? Empty.WeakReference;
        }

        public BindingResourceObject(object value, bool isWeak = false)
        {
            _isWeak = isWeak;
            SetValue(value);
        }

        #endregion

        #region Implementation of ISourceValue

        bool ISourceValue.IsAlive
        {
            get
            {
                if (_isWeak)
                    return ((WeakReference)_value).Target != null;
                return true;
            }
        }

        public object Value
        {
            get
            {
                if (_isWeak)
                    return ((WeakReference)_value).Target;
                return _value;
            }
            set
            {
                if (Equals(Value, value))
                    return;

                SetValue(value);
                var handler = ValueChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler<ISourceValue, EventArgs> ValueChanged;

        #endregion

        #region Methods

        private void SetValue(object value)
        {
            _value = _isWeak ? ToolkitExtensions.GetWeakReference(value) : value;
        }

        #endregion
    }
}
