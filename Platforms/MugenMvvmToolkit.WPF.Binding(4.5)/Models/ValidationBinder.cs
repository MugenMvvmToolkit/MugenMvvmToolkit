#region Copyright

// ****************************************************************************
// <copyright file="ValidationBinder.cs">
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

#if WPF
using System.Linq;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WPF.Binding.Models
#elif WINDOWS_PHONE && XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.WinPhone.Binding.Models
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Binding.Models
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Binding.Models
#endif
{
    public sealed class ValidationBinder :
#if NET4
 NotifyPropertyChangedBase, IDataErrorInfo
#else
 INotifyDataErrorInfo
#endif

    {
        #region Fields

#if NET4
        private static readonly PropertyChangedEventArgs ChangedArgs;
#else
        private static readonly DataErrorsChangedEventArgs ChangedArgs;
#endif

        private static readonly DependencyProperty ErrorContainerProperty;

        private const string PropertyName = "Value";
        private IList<object> _errors;

        #endregion

        #region Constructors

        static ValidationBinder()
        {
            ErrorContainerProperty = DependencyProperty.RegisterAttached("ErrorContainer", typeof(object), typeof(ValidationBinder), new PropertyMetadata(null));
#if NET4
            ChangedArgs = new PropertyChangedEventArgs(PropertyName);
#else
            ChangedArgs = new DataErrorsChangedEventArgs(PropertyName);
#endif

        }

        private ValidationBinder()
        {
            _errors = Empty.Array<object>();
        }

        #endregion

        #region Properties

        public object Value
        {
            get { return this; }
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }


        #endregion

        #region Methods

        public static void SetErrors([NotNull] FrameworkElement element, [CanBeNull] IList<object> errors)
        {
            Should.NotBeNull(element, nameof(element));
            if (errors == null)
                errors = Empty.Array<object>();
            var binder = (ValidationBinder)element.GetValue(ErrorContainerProperty);
            if (binder == null)
            {
                if (errors.Count == 0)
                    return;
                binder = new ValidationBinder();
                element.SetValue(ErrorContainerProperty, binder);
                var binding = new System.Windows.Data.Binding(PropertyName)
                {
#if WPF && NET4
                    ValidatesOnDataErrors = true,
#else
                    ValidatesOnDataErrors = false,
                    ValidatesOnNotifyDataErrors = true,
#endif
                    Mode = System.Windows.Data.BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
                    Source = binder,
                    ValidatesOnExceptions = false,
                    NotifyOnValidationError = false,
#if WPF
                    NotifyOnSourceUpdated = false,
                    NotifyOnTargetUpdated = false
#endif
                };
                element.SetBinding(ErrorContainerProperty, binding);
            }
            binder._errors = errors;
            binder.OnErrorsChanged();
        }

        private void OnErrorsChanged()
        {
#if NET4
            OnPropertyChanged(ChangedArgs, ExecutionMode.None);
#else
            EventHandler<DataErrorsChangedEventArgs> handler = ErrorsChanged;
            if (handler != null)
                handler(this, ChangedArgs);
#endif
        }

        #endregion

        #region Implementation of interfaces

#if NET4
        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                var error = _errors.FirstOrDefault();
                if (error == null)
                    return null;
                return error.ToString();
            }
        }

        string IDataErrorInfo.Error
        {
            get
            {
                if (_errors.Count == 0)
                    return null;
                return string.Join(Environment.NewLine, _errors);
            }
        }
#else
        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return _errors;
        }

        bool INotifyDataErrorInfo.HasErrors => _errors.Count != 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
#endif
        #endregion
    }
}
