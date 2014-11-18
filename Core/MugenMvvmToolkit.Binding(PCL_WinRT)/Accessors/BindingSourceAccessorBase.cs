#region Copyright
// ****************************************************************************
// <copyright file="BindingSourceAccessorBase.cs">
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
using System.Globalization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Accessors
{
    /// <summary>
    ///     Represents the base accessor for the binding source.
    /// </summary>
    public abstract class BindingSourceAccessorBase : IBindingSourceAccessor
    {
        #region Nested types

        /// <summary>
        /// Represents the bindings parameters.
        /// </summary>
        protected sealed class BindingParameters
        {
            #region Fields

            private readonly Func<IDataContext, IBindingValueConverter> _converterDelegate;
            private readonly Func<IDataContext, CultureInfo> _converterCultureDelegate;
            private readonly Func<IDataContext, object> _converterParameterDelegate;
            private readonly Func<IDataContext, object> _fallbackDelegate;
            private readonly object _targetNullValue;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="BindingParameters"/> class.
            /// </summary>
            internal BindingParameters(Func<IDataContext, IBindingValueConverter> converterDelegate, Func<IDataContext, CultureInfo> converterCultureDelegate, Func<IDataContext, object> converterParameterDelegate, Func<IDataContext, object> fallbackDelegate, object targetNullValue)
            {
                _converterDelegate = converterDelegate;
                _converterCultureDelegate = converterCultureDelegate;
                _converterParameterDelegate = converterParameterDelegate;
                _fallbackDelegate = fallbackDelegate;
                _targetNullValue = targetNullValue;
            }

            #endregion

            #region Properties

            /// <summary>
            ///     Gets or sets the <see cref="IBindingValueConverter" /> delegate.
            /// </summary>
            [CanBeNull]
            public Func<IDataContext, IBindingValueConverter> ConverterDelegate
            {
                get { return _converterDelegate; }
            }

            /// <summary>
            ///     Gets or sets the converter parameter delegate.
            /// </summary>
            [CanBeNull]
            public Func<IDataContext, object> ConverterParameterDelegate
            {
                get { return _converterParameterDelegate; }
            }

            /// <summary>
            ///     Gets or sets the converter culture delegate.
            /// </summary>
            [CanBeNull]
            public Func<IDataContext, CultureInfo> ConverterCultureDelegate
            {
                get { return _converterCultureDelegate; }
            }

            /// <summary>
            ///     Gets the target null value.
            /// </summary>
            [CanBeNull]
            public object TargetNullValue
            {
                get { return _targetNullValue; }
            }

            /// <summary>
            ///     Gets or sets the fallback value delegate.
            /// </summary>
            [CanBeNull]
            public Func<IDataContext, object> FallbackDelegate
            {
                get { return _fallbackDelegate; }
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly bool _isTarget;
        private readonly BindingParameters _parameters;

        #endregion

        #region Constructors

        static BindingSourceAccessorBase()
        {
            AutoConvertValueDefault = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingSourceAccessorBase" /> class.
        /// </summary>
        protected BindingSourceAccessorBase([NotNull] IDataContext context, bool isTarget)
        {
            Should.NotBeNull(context, "context");
            _isTarget = isTarget;
            bool hasValue = false;
            Func<IDataContext, IBindingValueConverter> converterDelegate;
            Func<IDataContext, CultureInfo> converterCultureDelegate = null;
            Func<IDataContext, object> converterParameterDelegate = null;
            object targetNullValue;
            if (context.TryGetData(BindingBuilderConstants.Converter, out converterDelegate))
            {
                converterCultureDelegate = context.GetData(BindingBuilderConstants.ConverterCulture);
                converterParameterDelegate = context.GetData(BindingBuilderConstants.ConverterParameter);
                hasValue = true;
            }
            if (context.TryGetData(BindingBuilderConstants.TargetNullValue, out targetNullValue))
                hasValue = true;
            Func<IDataContext, object> fallbackDelegate = null;
            if (!isTarget && context.TryGetData(BindingBuilderConstants.Fallback, out fallbackDelegate))
                hasValue = true;
            if (hasValue)
                _parameters = new BindingParameters(converterDelegate, converterCultureDelegate,
                    converterParameterDelegate, fallbackDelegate, targetNullValue);
            AutoConvertValue = AutoConvertValueDefault;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the property that is responsible for the automatic value conversion.
        /// </summary>
        public static bool AutoConvertValueDefault { get; set; }

        /// <summary>
        /// Gets the value that indicats that accessor is a target.
        /// </summary>
        protected bool IsTarget
        {
            get { return _isTarget; }
        }

        /// <summary>
        /// Gets the current binding parameters.
        /// </summary>
        [CanBeNull]
        protected BindingParameters Parameters
        {
            get { return _parameters; }
        }

        #endregion

        #region Implementation of IBindingSourceAccessor

        /// <summary>
        ///     Gets or sets the property that is responsible for the automatic value conversion.
        /// </summary>
        public bool AutoConvertValue { get; set; }

        /// <summary>
        ///     Gets the underlying sources.
        /// </summary>
        public abstract IList<IBindingSource> Sources { get; }

        /// <summary>
        ///     Gets the source value.
        /// </summary>
        /// <param name="targetMember">The specified member to set value.</param>
        /// <param name="context">The specified operation context.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the value cannot be obtained; false to return
        ///     <see cref="BindingConstants.InvalidValue" /> if the value cannot be obtained.
        /// </param>
        public object GetValue(IBindingMemberInfo targetMember, IDataContext context, bool throwOnError)
        {
            try
            {
                var value = GetValueInternal(targetMember, context, throwOnError);
                if (value.IsUnsetValue())
                    return targetMember.Type.GetDefaultValue();
                return value;
            }
            catch (Exception)
            {
                if (throwOnError)
                    throw;
                return BindingConstants.InvalidValue;
            }
        }

        /// <summary>
        ///     Sets the source value.
        /// </summary>
        /// <param name="targetAccessor">The specified accessor to get value.</param>
        /// <param name="context">The specified operation context.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the value cannot be set.
        /// </param>
        public bool SetValue(IBindingSourceAccessor targetAccessor, IDataContext context, bool throwOnError)
        {
            try
            {
                return SetValueInternal(targetAccessor, context, throwOnError);
            }
            catch (Exception)
            {
                if (throwOnError)
                    throw;
                return false;
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        ///     Occurs before the value changes.
        /// </summary>
        public abstract event EventHandler<IBindingSourceAccessor, ValueAccessorChangingEventArgs> ValueChanging;

        /// <summary>
        ///     Occurs when value changed.
        /// </summary>
        public abstract event EventHandler<IBindingSourceAccessor, ValueAccessorChangedEventArgs> ValueChanged;

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the raw value from source.
        /// </summary>
        protected abstract object GetRawValueInternal(IBindingMemberInfo targetMember, IDataContext context,
            bool throwOnError);

        /// <summary>
        ///     Sets the source value.
        /// </summary>
        /// <param name="targetAccessor">The specified accessor to get value.</param>
        /// <param name="context">The specified operation context.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the value cannot be set.
        /// </param>
        protected abstract bool SetValueInternal(IBindingSourceAccessor targetAccessor, IDataContext context,
            bool throwOnError);

        /// <summary>
        ///     Gets the source value.
        /// </summary>
        protected virtual object GetValueInternal(IBindingMemberInfo targetMember, IDataContext context,
            bool throwOnError)
        {
            if (_parameters == null)
                return GetRawValueInternal(targetMember, context, throwOnError);
            if (_isTarget)
                return GetTargetValue(targetMember, context, throwOnError);
            return GetSourceValue(targetMember, context, throwOnError);
        }

        private object GetTargetValue(IBindingMemberInfo targetMember, IDataContext context, bool throwOnError)
        {
            object value = GetRawValueInternal(targetMember, context, throwOnError);
            if (value.IsUnsetValueOrDoNothing())
                return value;
            if (_parameters.ConverterDelegate != null)
            {
                IBindingValueConverter converter = _parameters.ConverterDelegate(context);
                if (converter != null)
                {
                    CultureInfo culture = _parameters.ConverterCultureDelegate.GetValueOrDefault(context, CultureInfo.CurrentCulture);
                    object parameter = _parameters.ConverterParameterDelegate.GetValueOrDefault(context);
                    value = converter.ConvertBack(value, targetMember.Type, parameter, culture, context);
                }
            }
            if (Equals(value, _parameters.TargetNullValue))
                return null;
            return value;
        }

        private object GetSourceValue(IBindingMemberInfo targetMember, IDataContext context, bool throwOnError)
        {
            object value = GetRawValueInternal(targetMember, context, throwOnError);
            if (value.IsDoNothing())
                return BindingConstants.DoNothing;
            if (_parameters.ConverterDelegate != null && !value.IsUnsetValue())
            {
                IBindingValueConverter converter = _parameters.ConverterDelegate(context);
                if (converter != null)
                {
                    CultureInfo culture = _parameters.ConverterCultureDelegate.GetValueOrDefault(context, CultureInfo.CurrentCulture);
                    object parameter = _parameters.ConverterParameterDelegate.GetValueOrDefault(context);
                    value = converter.Convert(value, targetMember.Type, parameter, culture, context);
                }
            }
            if (value.IsUnsetValue())
                value = _parameters.FallbackDelegate.GetValueOrDefault(context) ?? targetMember.Type.GetDefaultValue();
            if (value == null)
                return _parameters.TargetNullValue;
            return value;
        }

        #endregion
    }
}