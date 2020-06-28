﻿using System;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Convert;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components.Binding
{
    public sealed class ParameterHandlerBindingComponent : ISourceValueInterceptorBindingComponent, ITargetValueInterceptorBindingComponent, IHasPriority, IDisposable
    {
        #region Fields

        private BindingParameterValue _converter;
        private BindingParameterValue _converterParameter;
        private BindingParameterValue _fallback;
        private BindingParameterValue _targetNullValue;

        #endregion

        #region Constructors

        public ParameterHandlerBindingComponent(BindingParameterValue converter, BindingParameterValue converterParameter, BindingParameterValue fallback,
            BindingParameterValue targetNullValue)
        {
            _converter = converter;
            _converterParameter = converterParameter;
            _fallback = fallback;
            _targetNullValue = targetNullValue;
        }

        #endregion

        #region Properties

        public static int Priority { get; set; } = BindingComponentPriority.ParameterHandler;

        public BindingParameterValue Converter => _converter;

        public BindingParameterValue ConverterParameter => _converterParameter;

        public BindingParameterValue Fallback => _fallback;

        public BindingParameterValue TargetNullValue => _targetNullValue;

        int IHasPriority.Priority => Priority;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            _converter.Dispose();
            _converterParameter.Dispose();
            _fallback.Dispose();
            _targetNullValue.Dispose();
            _converter = default;
            _converterParameter = default;
            _fallback = default;
            _targetNullValue = default;
        }

        public object? InterceptSourceValue(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata)
        {
            var converter = _converter.GetValue<IBindingValueConverter?>(metadata);
            if (converter != null)
                value = converter.ConvertBack(value, sourceMember.Member.Type, _converterParameter.GetValue<object?>(metadata), metadata);

            if (Equals(value, _targetNullValue.GetValue<object?>(metadata)))
                return null;
            return value;
        }

        public object? InterceptTargetValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata)
        {
            var converter = _converter.GetValue<IBindingValueConverter?>(metadata);
            if (converter != null)
                value = converter.Convert(value, targetMember.Member.Type, _converterParameter.GetValue<object?>(metadata), metadata);

            if (value.IsUnsetValue())
                value = _fallback.GetValue<object?>(metadata) ?? targetMember.Member.Type.GetDefaultValue();
            if (value == null)
                return _targetNullValue.GetValue<object?>(metadata);
            return value;
        }

        #endregion
    }
}