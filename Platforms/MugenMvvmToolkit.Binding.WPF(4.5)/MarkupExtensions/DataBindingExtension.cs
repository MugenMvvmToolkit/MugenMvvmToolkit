#region Copyright
// ****************************************************************************
// <copyright file="DataBindingExtension.cs">
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
using System.Windows.Markup;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Exceptions;

namespace MugenMvvmToolkit.Binding.MarkupExtensions
{
    /// <summary>
    ///     Provides high-level access to the definition of a binding, which connects the properties of binding target objects.
    /// </summary>
    public class DataBindingExtension : MarkupExtension
    {
        #region Fields

        private static readonly Func<object> NoDoFunc = () => null;
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
        private object _source;
        private string _elementName;
        private string _resourceMemberName;
        private string _targetMemberName;
        private IBindingMemberInfo _targetMemberInfo;
        private object _commandParameter;
        private bool? _toggleEnabledState;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of a class derived from <see cref="T:System.Windows.Markup.MarkupExtension"/>. 
        /// </summary>
        static DataBindingExtension()
        {
            CachedDelegates = new Dictionary<EventInfo, Delegate>();
        }

        /// <summary>
        ///     Initializes a new instance of a class derived from <see cref="DataBindingExtension" />.
        /// </summary>
        public DataBindingExtension()
        {
            _targetMemberName = string.Empty;
        }

#if WPF
        /// <summary>
        ///     Initializes a new instance of a class derived from <see cref="DataBindingExtension" />.
        /// </summary>
        public DataBindingExtension(string path)
            : this()
        {
            Path = path;
        }
#endif
        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the path to the binding source property.
        /// </summary>
#if WPF
        [ConstructorArgument("path")]
#endif
        public string Path { get; set; }

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
        ///     Gets or sets the object to use as the binding source.
        /// </summary>
        public object Source
        {
            get { return _source; }
            set
            {
                _source = value;
                if (value != null)
                    HasSource = true;
            }
        }

        /// <summary>
        /// Gets or sets the name of the element to use as the binding source object.
        /// </summary>
        public string ElementName
        {
            get { return _elementName; }
            set
            {
                _elementName = value;
                if (!string.IsNullOrWhiteSpace(value))
                    HasSource = true;
            }
        }

        /// <summary>
        ///     Gets or sets the name of dynamic member.
        /// </summary>
        public string ResourceMemberName
        {
            get { return _resourceMemberName; }
            set
            {
                _resourceMemberName = value;
                if (!string.IsNullOrWhiteSpace(value))
                    HasSource = true;
            }
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to include data errors behavior.
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
        ///     Gets or sets a value that indicates whether to include exception errors behavior.
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
        ///     Gets or sets a value that indicates whether to include exception errors behavior and data errors behavior.
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

        protected bool HasSource { get; set; }

        protected bool HasValue { get; set; }

        protected bool HasConverter { get; set; }

        protected bool HasConverterCulture { get; set; }

        protected bool HasConverterParameter { get; set; }

        protected bool HasCommandParameter { get; set; }

        protected bool HasFallback { get; set; }

        #endregion

        #region Overrides of MarkupExtension

        /// <summary>
        ///     When implemented in a derived class, returns an object that is provided as the value of the target property for
        ///     this markup extension.
        /// </summary>
        /// <returns>
        ///     The object value to set on the property where the extension is applied.
        /// </returns>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = serviceProvider.GetService<IProvideValueTarget>();
            if (target == null)
                return GetEmptyValue();
            var targetObject = target.TargetObject;
            var targetProperty = target.TargetProperty;
            if (targetObject == null || targetProperty == null)
                return GetEmptyValue();
            if (!(targetObject is DependencyObject) && targetObject.GetType().FullName == "System.Windows.SharedDp")
                return this;
#if WPF
            if (targetObject is Setter || targetObject is DataTrigger || targetObject is Condition)
#else
            if (targetObject is Setter)
#endif
                return this;

            if (_targetMemberName == string.Empty)
                _targetMemberName = GetMemberName(targetObject, targetProperty);
            if (_targetMemberName == null)
                return GetEmptyValue();

            IDataBinding binding = HasValue
                ? CreateBindingBuilder(targetObject, _targetMemberName).Build()
                : CreateBinding(targetObject, _targetMemberName);

            if (ApplicationSettings.IsDesignMode && binding is InvalidDataBinding)
                throw new DesignTimeException(((InvalidDataBinding)binding).Exception);
            return GetDefaultValue(targetObject, targetProperty, binding, _targetMemberName);
        }

        #endregion

        #region Methods

        protected virtual string GetMemberName(object targetObject, object targetProperty)
        {
#if WPF
            var depProp = targetProperty as DependencyProperty;
            if (depProp != null)
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(depProp, targetObject.GetType());
                if (descriptor != null && descriptor.IsAttached)
                    return RegisterAttachedProperty(depProp, targetObject);
                return depProp.Name;
            }
#endif
            var member = (MemberInfo)targetProperty;
#if !WPF
            if (member.MemberType == MemberTypes.Method)
                return RegisterAttachedProperty((MethodInfo)member, targetObject);
#endif
            return member.Name;
        }

        protected virtual IBindingBuilder CreateBindingBuilder(object targetObject, string targetPath)
        {
            IBindingBuilder builder = BindingProvider
                .Instance
                .CreateBuilderFromString(targetObject, targetPath, Path, GetSource(targetObject));

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
                syntaxBuilder.WithDelay(_delay);
            if (DefaultValueOnException)
                syntaxBuilder.DefaultValueOnException();
            return builder;
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
                _targetMemberInfo = BindingProvider.Instance
                    .MemberProvider
                    .GetBindingMember(targetObject.GetType(), targetPath, false, false);
            if (_targetMemberInfo == null)
                return GetEmptyValue();
            return _targetMemberInfo.GetValue(targetObject, null);
        }

        protected virtual object GetEmptyValue()
        {
#if WPF
            return DependencyProperty.UnsetValue;
#else
            //NOTE Сannot set property values ​​in the designer, this error will handled by MS code.
            if (ApplicationSettings.IsDesignMode)
                throw new InvalidOperationException();
            return DependencyProperty.UnsetValue;
#endif
        }

        protected object GetSource(object target)
        {
            if (!HasSource)
                return null;
            if (Source != null)
                return Source;
            if (!string.IsNullOrWhiteSpace(ResourceMemberName))
                return BindingProvider.Instance
                                      .ResourceResolver
                                      .ResolveObject(ResourceMemberName, DataContext.Empty, true)
                                      .Value;
            if (string.IsNullOrWhiteSpace(ElementName))
                return null;
            var element = BindingProvider.Instance
                                         .VisualTreeManager
                                         .FindByName(target, ElementName);
            if (element == null)
                throw BindingExceptionManager.ElementSourceNotFound(target, ElementName);
            return element;
        }

#if WPF
        private static string RegisterAttachedProperty(DependencyProperty property, object target)
        {
            var targetType = target.GetType();
            var path = property.Name + "Property";
            var member = BindingProvider.Instance
                                        .MemberProvider
                                        .GetBindingMember(targetType, path, false, false);
            if (member == null)
            {
                BindingProvider.Instance
                               .MemberProvider
                               .Register(targetType,
                                   new DependencyPropertyBindingMember(property, path, property.PropertyType,
                                       property.ReadOnly, null, null), true);
            }
            return path;
        }
#else
        private static string RegisterAttachedProperty(MethodInfo method, object target)
        {
            if (!(target is DependencyObject))
                return method.Name;

            Type reflectedType = method.ReflectedType;
            if (reflectedType == null)
                return null;

            var targetType = target.GetType();
            string name = method.Name.Replace("Get", string.Empty) + "Property";
            var memberInfo = BindingProvider.Instance
                                            .MemberProvider
                                            .GetBindingMember(targetType, name, false, false);
            if (memberInfo != null)
                return name;

            var fullName = string.Format("_attached_{0}_{1}", reflectedType.FullName.Replace(".", "_"), name);
            memberInfo = BindingProvider.Instance
                                        .MemberProvider
                                        .GetBindingMember(targetType, fullName, false, false);

            if (memberInfo != null)
                return fullName;

            FieldInfo fieldInfo = reflectedType.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (fieldInfo == null)
                return null;

            BindingProvider.Instance
                           .MemberProvider
                           .Register(method.GetParameters()[0].ParameterType,
                               new DependencyPropertyBindingMember((DependencyProperty)fieldInfo.GetValue(null), fullName, method.ReturnType, false, method, null), true);
            return fullName;
        }
#endif

        private IDataBinding CreateBinding(object targetObject, string targetPath)
        {
            return BindingProvider
                .Instance
                .CreateBindingFromString(targetObject, targetPath, Path, GetSource(targetObject));
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

        private static Delegate CreateDelegateForEvent(EventInfo eventInfo)
        {
            Delegate value;
            if (!CachedDelegates.TryGetValue(eventInfo, out value))
            {
                var parameters = eventInfo
                   .EventHandlerType
                   .GetMethod("Invoke")
                   .GetParameters()
                   .ToArrayFast(parameter => System.Linq.Expressions.Expression.Parameter(parameter.ParameterType));

                var callExpression = System.Linq.Expressions.Expression
                    .Call(System.Linq.Expressions.Expression.Constant(NoDoFunc, typeof(Func<object>)), "Invoke", EmptyValue<Type>.ArrayInstance);
                value = System.Linq.Expressions.Expression
                    .Lambda(eventInfo.EventHandlerType, callExpression, parameters)
                    .Compile();
            }
            return value;
        }

        #endregion
    }
}