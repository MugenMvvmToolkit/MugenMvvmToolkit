#region Copyright
// ****************************************************************************
// <copyright file="BindingResourceObject.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
    public class BindingResourceObject : IBindingResourceObject
    {
        #region Fields

        private readonly Type _type;
        private readonly object _value;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingResourceObject" /> class.
        /// </summary>
        public BindingResourceObject(object value, Type type = null)
        {
            _value = value;
            if (type == null && value != null)
                type = value.GetType();
            _type = type ?? typeof(object);
        }

        #endregion

        #region Implementation of IBindingResourceObject

        /// <summary>
        ///     Gets the type of object.
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }

        /// <summary>
        ///     Gets the value.
        /// </summary>
        public object Value
        {
            get { return _value; }
        }

        /// <summary>
        ///     Occurs when the <see cref="ISourceValue.Value"/>  property changed.
        /// </summary>
        public event EventHandler<ISourceValue, EventArgs> ValueChanged
        {
            add { }
            remove { }
        }

        #endregion
    }
}