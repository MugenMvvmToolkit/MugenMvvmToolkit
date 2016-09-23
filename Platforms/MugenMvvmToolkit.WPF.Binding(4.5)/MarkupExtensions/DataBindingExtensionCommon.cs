#region Copyright

// ****************************************************************************
// <copyright file="DataBindingExtensionCommon.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Globalization;
using System.Reflection;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
// ReSharper disable CheckNamespace
#if XAMARIN_FORMS
using Xamarin.Forms;
using MugenMvvmToolkit.Xamarin.Forms.Binding.Converters;
using MugenMvvmToolkit.Xamarin.Forms.Binding.Models;

namespace MugenMvvmToolkit.Xamarin.Forms.MarkupExtensions
#elif WPF
using System.Windows;
using System.Windows.Data;
using MugenMvvmToolkit.WPF.Binding.Converters;
using MugenMvvmToolkit.WPF.Binding.Models;

namespace MugenMvvmToolkit.WPF.MarkupExtensions
#elif SILVERLIGHT
using System.Windows;
using System.Windows.Data;
using MugenMvvmToolkit.Silverlight.Binding.Converters;
using MugenMvvmToolkit.Silverlight.Binding.Models;

namespace MugenMvvmToolkit.Silverlight.MarkupExtensions
#endif
// ReSharper restore CheckNamespace
{
    public partial class DataBindingExtension
    {
        #region Fields

        private static readonly Func<object> NoDoFunc = () => null;
        private static readonly Dictionary<EventInfo, Delegate> CachedDelegates = new Dictionary<EventInfo, Delegate>();

        private object _defaultValueOnException;
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
        private bool? _hasStablePath;
        private bool? _observable;
        private bool? _optional;
        private string _debugTag;

        #endregion

        #region Properties

#if WPF
        [System.Windows.Markup.ConstructorArgument("path")]
#endif
        public string Path { get; set; }

        public string Expression
        {
            get { return Path; }
            set { Path = value; }
        }

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

        public object Converter
        {
            get { return _converter; }
            set
            {
                if (value == null)
                    _converter = null;
                else
                    _converter = value as IBindingValueConverter ?? new ValueConverterWrapper((IValueConverter)value);
                HasConverter = true;
                HasValue = true;
            }
        }

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

        public object DefaultValueOnException
        {
            get { return _defaultValueOnException; }
            set
            {
                _defaultValueOnException = value;
                HasValue = true;
                HasDefaultValueOnException = true;
            }
        }

        public bool Validate
        {
            get { return ValidatesOnNotifyDataErrors && ValidatesOnExceptions; }
            set
            {
                ValidatesOnExceptions = value;
                ValidatesOnNotifyDataErrors = value;
            }
        }

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

        public bool? Observable
        {
            get { return _observable; }
            set
            {
                if (value.HasValue)
                    HasValue = true;
                _observable = value;
            }
        }

        public bool? Optional
        {
            get { return _optional; }
            set
            {
                if (value.HasValue)
                    HasValue = true;
                _optional = value;
            }
        }

        public bool? HasStablePath
        {
            get { return _hasStablePath; }
            set
            {
                if (value.HasValue)
                    HasValue = true;
                _hasStablePath = value;
            }
        }

        public string DebugTag
        {
            get { return _debugTag; }
            set
            {
                _debugTag = value;
                if (!string.IsNullOrEmpty(value))
                    HasValue = true;
            }
        }

        protected bool HasValue { get; set; }

        protected bool HasConverter { get; set; }

        protected bool HasConverterCulture { get; set; }

        protected bool HasConverterParameter { get; set; }

        protected bool HasCommandParameter { get; set; }

        protected bool HasFallback { get; set; }

        protected bool HasDefaultValueOnException { get; set; }

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
            if (ServiceProvider.IsDesignMode)
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
#if WPF
            eventInfo = _targetMemberInfo.Member as EventInfo;
            if (eventInfo != null)
                return CreateDelegateForEvent(eventInfo);
#endif
            return _targetMemberInfo.GetValue(targetObject, null);
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
                syntaxBuilder.WithConverter(d => _converter);
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
            if (HasStablePath.HasValue)
                syntaxBuilder.HasStablePath(HasStablePath.Value);
            if (Observable.HasValue)
                syntaxBuilder.Observable(Observable.Value);
            if (Optional.HasValue)
                syntaxBuilder.Optional(Optional.Value);
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
            if (HasDefaultValueOnException)
                syntaxBuilder.DefaultValueOnException(DefaultValueOnException);
            if (!string.IsNullOrEmpty(DebugTag))
                syntaxBuilder.WithDebugTag(DebugTag);
            return builder;
        }

        private IDataBinding CreateBinding(object targetObject, string targetPath, bool isDesignMode)
        {
            if (isDesignMode)
            {
                return BindingServiceProvider
                      .BindingProvider
                      .CreateBindingsFromStringWithBindings(targetObject, ToBindingExpression(targetPath))[0];
            }
            BindingServiceProvider
                .BindingProvider
                .CreateBindingsFromString(targetObject, ToBindingExpression(targetPath));
            return null;
        }

        private void SetMode(IBindingModeSyntax<object> syntax)
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

        private void SetUpdateSourceTrigger(IBindingBehaviorSyntax<object> syntax)
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
                    .GetMethodEx(nameof(Action.Invoke))
                    .GetParameters()
                    .ToArrayEx(parameter => System.Linq.Expressions.Expression.Parameter(parameter.ParameterType));

                var callExpression = System.Linq.Expressions.Expression
                    .Call(System.Linq.Expressions.Expression.Constant(NoDoFunc, typeof(Func<object>)), nameof(Action.Invoke), Empty.Array<Type>());
                value = System.Linq.Expressions.Expression
                    .Lambda(eventInfo.EventHandlerType, callExpression, parameters)
                    .Compile();
            }
            return value;
        }

        #endregion
    }
}
