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
    /// <summary>
    ///     Represents the binding expression object.
    /// </summary>
    public class BindingResourceObject : ISourceValue
    {
        #region Fields

        private object _value;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingResourceObject" /> class.
        /// </summary>
        public BindingResourceObject(object value)
        {
            _value = value;
        }

        #endregion

        #region Implementation of ISourceValue

        /// <summary>
        ///     Gets an indication whether the object referenced by the current <see cref="ISourceValue" /> object has
        ///     been garbage collected.
        /// </summary>
        /// <returns>
        ///     true if the object referenced by the current <see cref="ISourceValue" /> object has not been garbage
        ///     collected and is still accessible; otherwise, false.
        /// </returns>
        bool ISourceValue.IsAlive
        {
            get { return true; }
        }

        /// <summary>
        ///     Gets the value.
        /// </summary>
        public object Value
        {
            get { return _value; }
            set
            {
                if (Equals(_value, value))
                    return;
                _value = value;
                var handler = ValueChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Occurs when the <see cref="Value"/>  property changed.
        /// </summary>
        public event EventHandler<ISourceValue, EventArgs> ValueChanged;

        #endregion
    }
}