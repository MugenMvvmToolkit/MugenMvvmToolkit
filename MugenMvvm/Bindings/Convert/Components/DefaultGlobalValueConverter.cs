using System;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Bindings.Interfaces.Convert.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Convert.Components
{
    public sealed class DefaultGlobalValueConverter : IGlobalValueConverterComponent, IHasPriority
    {
        public Func<IFormatProvider>? FormatProvider { get; set; }

        public int Priority { get; set; } = ConverterComponentPriority.Converter;

        public bool TryConvert(IGlobalValueConverter converter, ref object? value, Type targetType, object? member, IReadOnlyMetadataContext? metadata) =>
            BindingMugenExtensions.TryConvert(ref value, targetType, FormatProvider);
    }
}