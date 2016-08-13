#region Copyright

// ****************************************************************************
// <copyright file="BindingSourceAccessorBase.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Accessors
{
    public abstract class BindingSourceAccessorBase : IBindingSourceAccessor
    {
        #region Nested types

        protected sealed class BindingParameters
        {
            #region Fields

            private readonly Func<IDataContext, IBindingValueConverter> _converterDelegate;
            private readonly Func<IDataContext, CultureInfo> _converterCultureDelegate;
            private readonly Func<IDataContext, object> _converterParameterDelegate;
            private readonly Func<IDataContext, object> _commandParameterDelegate;
            private readonly Func<IDataContext, object> _fallbackDelegate;
            private readonly object _targetNullValue;

            #endregion

            #region Constructors

            internal BindingParameters(Func<IDataContext, IBindingValueConverter> converterDelegate, Func<IDataContext, CultureInfo> converterCultureDelegate, Func<IDataContext, object> converterParameterDelegate, Func<IDataContext, object> fallbackDelegate, object targetNullValue, Func<IDataContext, object> commandParameterDelegate)
            {
                _converterDelegate = converterDelegate;
                _converterCultureDelegate = converterCultureDelegate;
                _converterParameterDelegate = converterParameterDelegate;
                _fallbackDelegate = fallbackDelegate;
                _targetNullValue = targetNullValue;
                _commandParameterDelegate = commandParameterDelegate;
            }

            #endregion

            #region Properties

            [CanBeNull]
            public Func<IDataContext, IBindingValueConverter> ConverterDelegate => _converterDelegate;

            [CanBeNull]
            public Func<IDataContext, object> ConverterParameterDelegate => _converterParameterDelegate;

            [CanBeNull]
            public Func<IDataContext, CultureInfo> ConverterCultureDelegate => _converterCultureDelegate;

            [CanBeNull]
            public object TargetNullValue => _targetNullValue;

            [CanBeNull]
            public Func<IDataContext, object> FallbackDelegate => _fallbackDelegate;

            [CanBeNull]
            public Func<IDataContext, object> CommandParameterDelegate => _commandParameterDelegate;

            #endregion
        }

        #endregion

        #region Fields

        private readonly bool _isTarget;
        private BindingParameters _parameters;

        #endregion

        #region Constructors

        protected BindingSourceAccessorBase([NotNull] IDataContext context, bool isTarget)
        {
            Should.NotBeNull(context, nameof(context));
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
            Func<IDataContext, object> commandParameterDelegate = null;
            if (isTarget && context.TryGetData(BindingBuilderConstants.CommandParameter, out commandParameterDelegate))
                hasValue = true;
            if (hasValue)
                _parameters = new BindingParameters(converterDelegate, converterCultureDelegate,
                    converterParameterDelegate, fallbackDelegate, targetNullValue, commandParameterDelegate);
        }

        #endregion

        #region Properties

        protected bool IsTarget => _isTarget;

        [CanBeNull]
        protected BindingParameters Parameters => _parameters;

        protected abstract bool IsDebuggable { get; }

        protected abstract string DebugTag { get; }

        #endregion

        #region Implementation of IBindingSourceAccessor

        public abstract bool DisableEqualityChecking { get; set; }

        public abstract bool CanRead { get; }

        public abstract bool CanWrite { get; }

        public abstract IList<IObserver> Sources { get; }

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

        public virtual void Dispose()
        {
            if (IsDebuggable)
                DebugInfo("Dispose accessor");
            _parameters = null;
        }

        public abstract event EventHandler<IBindingSourceAccessor, ValueAccessorChangingEventArgs> ValueChanging;

        public abstract event EventHandler<IBindingSourceAccessor, ValueAccessorChangedEventArgs> ValueChanged;

        #endregion

        #region Methods

        protected abstract object GetRawValueInternal(IBindingMemberInfo targetMember, IDataContext context,
            bool throwOnError);

        protected abstract bool SetValueInternal(IBindingSourceAccessor targetAccessor, IDataContext context,
            bool throwOnError);

        protected virtual object GetValueInternal(IBindingMemberInfo targetMember, IDataContext context,
            bool throwOnError)
        {
            if (_parameters == null)
                return GetRawValueInternal(targetMember, context, throwOnError);
            if (_isTarget)
                return GetTargetValue(targetMember, context, throwOnError);
            return GetSourceValue(targetMember, context, throwOnError);
        }

        protected void DebugInfo(string message, object[] args = null)
        {
            BindingServiceProvider.DebugBinding(this, DebugTag, message, args);
        }

        private object GetTargetValue(IBindingMemberInfo targetMember, IDataContext context, bool throwOnError)
        {
            var isDebuggable = IsDebuggable;
            object value = GetRawValueInternal(targetMember, context, throwOnError);
            if (isDebuggable)
                DebugInfo($"Got a target value: '{value}'");
            if (value.IsUnsetValueOrDoNothing())
                return value;

            if (_parameters.ConverterDelegate != null)
            {
                IBindingValueConverter converter = _parameters.ConverterDelegate(context);
                if (converter != null)
                {
                    CultureInfo culture = _parameters.ConverterCultureDelegate.GetValueOrDefault(context, BindingServiceProvider.BindingCultureInfo());
                    object parameter = _parameters.ConverterParameterDelegate.GetValueOrDefault(context);
                    if (isDebuggable)
                    {
                        DebugInfo($"Applying converter for target value: '{value}', converter: '{converter}', parameter: '{parameter}', culture: {culture}, target type: '{targetMember.Type}'");
                        value = converter.ConvertBack(value, targetMember.Type, parameter, culture, context);
                        DebugInfo($"Converter '{converter}' returns value: '{value}'");
                    }
                    else
                        value = converter.ConvertBack(value, targetMember.Type, parameter, culture, context);
                }
            }
            if (Equals(value, _parameters.TargetNullValue))
            {
                if (isDebuggable)
                    DebugInfo("Target value equals to TargetNullValue, return null value");
                return null;
            }
            return value;
        }

        private object GetSourceValue(IBindingMemberInfo targetMember, IDataContext context, bool throwOnError)
        {
            var isDebuggable = IsDebuggable;
            object value = GetRawValueInternal(targetMember, context, throwOnError);
            if (isDebuggable)
                DebugInfo($"Got a source value: '{value}'");
            if (value.IsDoNothing())
                return BindingConstants.DoNothing;

            if (_parameters.ConverterDelegate != null && !value.IsUnsetValue())
            {
                IBindingValueConverter converter = _parameters.ConverterDelegate(context);
                if (converter != null)
                {
                    CultureInfo culture = _parameters.ConverterCultureDelegate.GetValueOrDefault(context, BindingServiceProvider.BindingCultureInfo());
                    object parameter = _parameters.ConverterParameterDelegate.GetValueOrDefault(context);
                    if (isDebuggable)
                    {
                        DebugInfo($"Applying converter for source value: '{value}', converter: '{converter}', parameter: '{parameter}', culture: {culture}, target type: '{targetMember.Type}'");
                        value = converter.Convert(value, targetMember.Type, parameter, culture, context);
                        DebugInfo($"Converter '{converter}' returns value: '{value}'");
                    }
                    else
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
