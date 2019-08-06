using System;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingParametersInterceptorComponent : ISourceValueInterceptorDataBindingComponent, ITargetValueInterceptorDataBindingComponent
    {
        #region Fields

        public readonly Func<IReadOnlyMetadataContext, object?>? CommandParameterDelegate;
        public readonly Func<IReadOnlyMetadataContext, IBindingValueConverter?>? ConverterDelegate;
        public readonly Func<IReadOnlyMetadataContext, object?>? ConverterParameterDelegate;
        public readonly Func<IReadOnlyMetadataContext, object?>? FallbackDelegate;
        public readonly object? TargetNullValue;

        #endregion

        #region Constructors

        public BindingParametersInterceptorComponent(Func<IReadOnlyMetadataContext, IBindingValueConverter> converterDelegate,
            Func<IReadOnlyMetadataContext, object> converterParameterDelegate, Func<IReadOnlyMetadataContext, object> commandParameterDelegate,
            Func<IReadOnlyMetadataContext, object> fallbackDelegate, object targetNullValue)
        {
            ConverterDelegate = converterDelegate;
            ConverterParameterDelegate = converterParameterDelegate;
            CommandParameterDelegate = commandParameterDelegate;
            FallbackDelegate = fallbackDelegate;
            TargetNullValue = targetNullValue;
        }

        #endregion

        #region Properties

        public static int Priority { get; set; } = int.MaxValue;

        #endregion

        #region Implementation of interfaces

        int IComponent.GetPriority(object source)
        {
            return Priority;
        }

        public object? InterceptSourceValue(in BindingPathLastMember sourceMembers, object? value, IReadOnlyMetadataContext metadata)
        {
            var converter = ConverterDelegate?.Invoke(metadata);
            if (converter != null)
                value = converter.ConvertBack(value, sourceMembers.LastMember.Type, ConverterParameterDelegate?.Invoke(metadata), metadata);

            if (Equals(value, TargetNullValue))
                return null;
            return value;
        }

        public object? InterceptTargetValue(in BindingPathLastMember targetMembers, object? value, IReadOnlyMetadataContext metadata)
        {
            if (ConverterDelegate != null && !value.IsUnsetValue())
            {
                var converter = ConverterDelegate(metadata);
                if (converter != null)
                    value = converter.Convert(value, targetMembers.LastMember.Type, ConverterParameterDelegate?.Invoke(metadata), metadata);
            }

            if (value.IsUnsetValue())
                value = FallbackDelegate?.Invoke(metadata) ?? targetMembers.LastMember.Type.GetDefaultValue();
            if (value == null)
                return TargetNullValue;
            return value;
        }

        #endregion
    }
}