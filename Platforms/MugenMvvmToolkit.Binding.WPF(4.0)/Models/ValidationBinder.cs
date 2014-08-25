#region Copyright
// ****************************************************************************
// <copyright file="ValidationBinder.cs">
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    public sealed class ValidationBinder : NotifyPropertyChangedBase, IDataErrorInfo
    {
        #region Attached properties

        internal static readonly DependencyProperty ErrorContainerProperty =
            DependencyProperty.RegisterAttached("ErrorContainer", typeof(object), typeof(ValidationBinder),
                new PropertyMetadata(default(object)));

        internal static void SetErrorContainer(DependencyObject element, object value)
        {
            element.SetValue(ErrorContainerProperty, value);
        }

        internal static object GetErrorContainer(DependencyObject element)
        {
            return element.GetValue(ErrorContainerProperty);
        }

        #endregion

        #region Fields

        public const string PropertyName = "Value";
        private static readonly PropertyChangedEventArgs ChangedArgs = new PropertyChangedEventArgs(PropertyName);
        private IEnumerable _errors;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the fake value.
        /// </summary>
        public object Value
        {
            get { return this; }
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        #endregion

        #region Methods

        internal void SetErrors(IEnumerable enumerable)
        {
            _errors = enumerable;
            OnErrorsChanged();
        }

        private void OnErrorsChanged()
        {
            OnPropertyChanged(ChangedArgs, ExecutionMode.AsynchronousOnUiThread);
        }

        #endregion

        #region Implementation of IDataErrorInfo

        /// <summary>
        ///     Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        ///     The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        public string this[string columnName]
        {
            get
            {
                IEnumerable errors = _errors;
                if (errors == null)
                    return null;
                object result = errors.OfType<object>().FirstOrDefault();
                if (result == null)
                    return null;
                return result.ToString();
            }
        }

        /// <summary>
        ///     Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        ///     An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error
        {
            get
            {
                IEnumerable errors = _errors;
                if (errors == null)
                    return null;
                IEnumerable<string> errorsSt = errors.OfType<object>().Select(o => o.ToString());
                return string.Join(Environment.NewLine, errorsSt);
            }
        }

        #endregion
    }
}