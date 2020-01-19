using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Resources.Components
{
    public sealed class BindingValueConverterResolverComponent : IBindingValueConverterResolverComponent, IHasPriority
    {
        #region Constructors

        public BindingValueConverterResolverComponent()
        {
            Converters = new Dictionary<string, IBindingValueConverter>();
        }

        #endregion

        #region Properties

        public Dictionary<string, IBindingValueConverter> Converters { get; }

        public int Priority { get; set; } = ResourceComponentPriority.ConverterResolver;

        #endregion

        #region Implementation of interfaces

        public IBindingValueConverter? TryGetConverter<TRequest>(string name, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Converters.TryGetValue(name, out var value);
            return value;
        }

        #endregion
    }
}