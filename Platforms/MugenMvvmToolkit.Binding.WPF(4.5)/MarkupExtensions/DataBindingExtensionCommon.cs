#region Copyright
// ****************************************************************************
// <copyright file="DataBindingExtensionCommon.cs">
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
using MugenMvvmToolkit.Binding.Models;

#if XAMARIN_FORMS
namespace MugenMvvmToolkit.MarkupExtensions
#else
namespace MugenMvvmToolkit.Binding.MarkupExtensions
#endif
{
    public partial class DataBindingExtension
    {
        #region Fields

        private static readonly Func<object> NoDoFunc;
        private static readonly Dictionary<EventInfo, Delegate> CachedDelegates;

        private bool _defaultValueOnException;
        private uint _delay;
        private object _fallback;
        private object _targetNullValue;
        private bool _validatesOnExceptions;
        private bool _validatesOnNotifyDataErrors;
        private object _converterParameter;
        private CultureInfo _converterCulture;
        private IBindingValueConverter _converter;
        private UpdateSourceTriggerCore _updateSourceTrigger;
        private BindingModeCore _mode;
        private IBindingMemberInfo _targetMemberInfo;
        private object _commandParameter;
        private bool? _toggleEnabledState;
        private uint _targetDelay;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of a class derived from <see cref="T:System.Windows.Markup.MarkupExtension"/>. 
        /// </summary>
        static DataBindingExtension()
        {
            CachedDelegates = new Dictionary<EventInfo, Delegate>();
            NoDoFunc = () => null;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the path to the binding source property.
        /// </summary>
#if WPF
        [System.Windows.Markup.ConstructorArgument("path")]
#endif
        public string Path { get; set; }

        /// <summary>
        ///     Gets or sets the path to the binding source property this property is the same as <see cref="Path"/>.
        /// </summary>
        public string Expression
        {
            get { return Path; }
            set { Path = value; }
        }

        /// <summary>
        ///     Gets or sets a value that indicates the direction of the data flow in the binding.
        /// </summary>
        public BindingModeCore Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                if (value != BindingModeCore.Default)
                    HasValue = true;
            }
        }

        /// <summary>
        ///     Gets or sets a value that determines the timing of binding source updates.
        /// </summary>
        public UpdateSourceTriggerCore UpdateSourceTrigger
        {
            get { return _updateSourceTrigger; }
            set
            {
                _updateSourceTrigger = value;
                if (value != UpdateSourceTriggerCore.Default)
                    HasValue = true;
            }
        }

        /// <summary>
        ///     Gets or sets the converter to use.
        /// </summary>
        public IBindingValueConverter Converter
        {
            get { return _converter; }
            set
            {
                _converter = value;
                HasValue = true;
                HasConverter = true;
            }
        }

        /// <summary>
        ///     Gets or sets the culture in which to evaluate the converter.
        /// </summary>
        public CultureInfo ConverterCulture
        {
            get { return _converterCulture; }
            set
            {
                _converterCulture = value;
                HasValue = true;
                HasConverterCulture = true;
            }
        }

        /// <summary>
        ///     Gets or sets the parameter to pass to the Converter.
        /// </summary>
        public object ConverterParameter
        {
            get { return _converterParameter; }
            set
            {
                _converterParameter = value;
                HasValue = true;
                HasConverterParameter = true;
            }
        }

        /// <summary>
        ///     Gets or sets the parameter to pass to the command.
        /// </summary>
        public object CommandParameter
        {
            get { return _commandParameter; }
            set
            {
                _commandParameter = value;
                HasValue = true;
                HasCommandParameter = true;
            }
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to include <see cref="ValidatesOnNotifyDataErrorsBehavior"/>
        /// </summary>
        public bool ValidatesOnNotifyDataErrors
        {
            get { return _validatesOnNotifyDataErrors; }
            set
            {
                _validatesOnNotifyDataErrors = value;
                if (value)
                    HasValue = true;
            }
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to include <see cref="ValidatesOnExceptionsBehavior"/>
        /// </summary>
        public bool ValidatesOnExceptions
        {
            get { return _validatesOnExceptions; }
            set
            {
                _validatesOnExceptions = value;
                if (value)
                    HasValue = true;
            }
        }

        /// <summary>
        ///     Gets or sets the value that is used in the target when the value of the source is null.
        /// </summary>
        public object TargetNullValue
        {
            get { return _targetNullValue; }
            set
            {
                _targetNullValue = value;
                if (value != null)
                    HasValue = true;
            }
        }

        /// <summary>
        ///     Gets or sets the value to use when the binding is unable to return a value.
        /// </summary>
        public object Fallback
        {
            get { return _fallback; }
            set
            {
                _fallback = value;
                HasValue = true;
                HasFallback = true;
            }
        }

        /// <summary>
        ///     Gets or sets the amount of time, in milliseconds, to wait before updating the binding source after the value on the
        ///     target changes.
        /// </summary>
        public int Delay
        {
            get { return (int)_delay; }
            set
            {
                _delay = (uint)value;
                if (value != 0)
                    HasValue = true;
            }
        }

        /// <summary>
        ///     Gets or sets the amount of time, in milliseconds, to wait before updating the binding target after the value on the
        ///     source changes.
        /// </summary>
        public int TargetDelay
        {
            get
            {
                return (int)_targetDelay;
            }
            set
            {
                _targetDelay = (uint)value;
                if (value != 0)
                    HasValue = true;
            }
        }

        /// <summary>
        ///     Gets or sets value that allows to set default value on a binding exception.
        /// </summary>
        public bool DefaultValueOnException
        {
            get { return _defaultValueOnException; }
            set
            {
                _defaultValueOnException = value;
                if (value)
                    HasValue = true;
            }
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to include <see cref="ValidatesOnNotifyDataErrorsBehavior"/> and <see cref="ValidatesOnExceptionsBehavior"/>.
        /// </summary>
        public bool Validate
        {
            get { return ValidatesOnNotifyDataErrors && ValidatesOnExceptions; }
            set
            {
                ValidatesOnExceptions = value;
                ValidatesOnNotifyDataErrors = value;
            }
        }

        /// <summary>
        ///     Gets or sets the property that is responsible for the automatic toggle enabled state for command.
        /// </summary>
        public bool? ToggleEnabledState
        {
            get { return _toggleEnabledState; }
            set
            {
                if (value.HasValue)
                    HasValue = true;
                _toggleEnabledState = value;
            }
        }

        protected bool HasValue { get; set; }

        protected bool HasConverter { get; set; }

        protected bool HasConverterCulture { get; set; }

        protected bool HasConverterParameter { get; set; }

        protected bool HasCommandParameter { get; set; }

        protected bool HasFallback { get; set; }

        #endregion

        #region Methods

        protected virtual object GetEmptyValue()
        {
#if WPF
            return DependencyProperty.UnsetValue;
#elif XAMARIN_FORMS
            return null;
#else
            //NOTE Сannot set property values ​​in the designer, this error will handled by MS code.
            if (ServiceProvider.DesignTimeManager.IsDesignMode)
                throw new InvalidOperationException();
            return DependencyProperty.UnsetValue;
#endif
        }

        protected virtual object GetDefaultValue(object targetObject, object targetProperty, IDataBinding binding, string targetPath)
        {
#if WPF
            var dp = targetProperty as DependencyProperty;
            if (dp != null)
                return ((DependencyObject)targetObject).GetValue(dp);
#endif
            var eventInfo = targetProperty as EventInfo;
            if (eventInfo != null)
                return CreateDelegateForEvent(eventInfo);

            if (_targetMemberInfo == null)
                _targetMemberInfo = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(targetObject.GetType(), targetPath, false, false);
            if (_targetMemberInfo == null)
                return GetEmptyValue();
#if XAMARIN_FORMS
            //BUG: null value throw an error https://bugzilla.xamarin.com/show_bug.cgi?id=24584
            var value = _targetMemberInfo.GetValue(targetObject, null);
            if (value == null && _targetMemberInfo.Type == typeof(string))
                return string.Empty;
            return value;
#else
            return _targetMemberInfo.GetValue(targetObject, null);
#endif
        }

        protected virtual IBindingBuilder CreateBindingBuilder(object targetObject, string targetPath)
        {
            IBindingBuilder builder = BindingServiceProvider
                .BindingProvider
                .CreateBuildersFromString(targetObject, ToBindingExpression(targetPath))[0];

            var syntaxBuilder = new SyntaxBuilder<object, object>(builder);
            SetMode(syntaxBuilder);
            SetUpdateSourceTrigger(syntaxBuilder);
            if (HasConverter)
                syntaxBuilder.WithConverter(d => Converter);
            if (HasConverterCulture)
                syntaxBuilder.WithConverterCulture(d => ConverterCulture);
            if (HasConverterParameter)
                syntaxBuilder.WithConverterParameter(d => ConverterParameter);
            if (HasFallback)
                syntaxBuilder.WithFallback(d => Fallback);
            if (HasCommandParameter)
                syntaxBuilder.WithCommandParameter(d => CommandParameter);

            if (ToggleEnabledState.HasValue)
                syntaxBuilder.ToggleEnabledState(ToggleEnabledState.Value);
            if (ValidatesOnExceptions)
                syntaxBuilder.ValidatesOnExceptions();
            if (ValidatesOnNotifyDataErrors)
                syntaxBuilder.ValidatesOnNotifyDataErrors();
            if (TargetNullValue != null)
                syntaxBuilder.WithTargetNullValue(TargetNullValue);
            if (Delay != 0)
                syntaxBuilder.WithDelay(_delay, false);
            if (TargetDelay != 0)
                syntaxBuilder.WithDelay(_targetDelay, true);
            if (DefaultValueOnException)
                syntaxBuilder.DefaultValueOnException();
            return builder;
        }

        private IDataBinding CreateBinding(object targetObject, string targetPath)
        {
            return BindingServiceProvider
                .BindingProvider
                .CreateBindingsFromString(targetObject, ToBindingExpression(targetPath))[0];
        }

        private void SetMode(IBindingModeSyntax syntax)
        {
            switch (Mode)
            {
                case BindingModeCore.TwoWay:
                    syntax.TwoWay();
                    break;
                case BindingModeCore.OneWay:
                    syntax.OneWay();
                    break;
                case BindingModeCore.OneTime:
                    syntax.OneTime();
                    break;
                case BindingModeCore.OneWayToSource:
                    syntax.OneWayToSource();
                    break;
                case BindingModeCore.None:
                    syntax.NoneMode();
                    break;
            }
        }

        private void SetUpdateSourceTrigger(IBindingBehaviorSyntax syntax)
        {
            switch (UpdateSourceTrigger)
            {
                case UpdateSourceTriggerCore.LostFocus:
                    syntax.LostFocusUpdateSourceTrigger();
                    break;
            }
        }

        private string ToBindingExpression(string targetPath)
        {
            return targetPath + " " + Path + ";";
        }

        private static Delegate CreateDelegateForEvent(EventInfo eventInfo)
        {
            Delegate value;
            if (!CachedDelegates.TryGetValue(eventInfo, out value))
            {
                var parameters = eventInfo
                    .EventHandlerType
                    .GetMethodEx("Invoke")
                    .GetParameters()
                    .ToArrayEx(parameter => System.Linq.Expressions.Expression.Parameter(parameter.ParameterType));

                var callExpression = System.Linq.Expressions.Expression
                    .Call(System.Linq.Expressions.Expression.Constant(NoDoFunc, typeof(Func<object>)), "Invoke", Empty.Array<Type>());
                value = System.Linq.Expressions.Expression
                    .Lambda(eventInfo.EventHandlerType, callExpression, parameters)
                    .Compile();
            }
            return value;
        }

        #endregion

    }
}
