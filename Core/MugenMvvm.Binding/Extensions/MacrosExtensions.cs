using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class MugenBindingExtensions
    {
        #region Methods

        [Preserve(Conditional = true)]
        public static IBinding? GetBinding(IReadOnlyMetadataContext? metadata = null)
        {
            return metadata?.Get(BindingMetadata.Binding);
        }

        [Preserve(Conditional = true)]
        public static object? GetEventArgs(IReadOnlyMetadataContext? metadata = null)
        {
            //todo add impl
            return null;
        }

        #endregion
    }
}