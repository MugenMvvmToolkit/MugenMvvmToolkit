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
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MugenMvvmToolkit.Binding.Models
{
    internal sealed class ValidationBinder : INotifyDataErrorInfo
    {
        #region Attached properties

        internal static readonly DependencyProperty ErrorContainerProperty =
            DependencyProperty.RegisterAttached("ErrorContainer", typeof(object), typeof(ValidationBinder), new PropertyMetadata(null));

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
        private IEnumerable _errors;
        private static readonly DataErrorsChangedEventArgs ChangedArgs = new DataErrorsChangedEventArgs(PropertyName);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the fake value.
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
            EventHandler<DataErrorsChangedEventArgs> handler = ErrorsChanged;
            if (handler != null) handler(this, ChangedArgs);
        }

        #endregion

        #region Implementation of INotifyDataErrorInfo

        /// <summary>
        ///     Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <returns>
        ///     The validation errors for the property or entity.
        /// </returns>
        /// <param name="propertyName">
        ///     The name of the property to retrieve validation errors for; or null or
        ///     <see cref="F:System.String.Empty" />, to retrieve entity-level errors.
        /// </param>
        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return _errors;
        }

        /// <summary>
        ///     Gets a value that indicates whether the entity has validation errors.
        /// </summary>
        /// <returns>
        ///     true if the entity currently has validation errors; otherwise, false.
        /// </returns>
        bool INotifyDataErrorInfo.HasErrors
        {
            get
            {
                IEnumerable errors = _errors;
                return errors == null || errors.OfType<object>().Any();
            }
        }

        /// <summary>
        ///     Occurs when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        #endregion
    }
}