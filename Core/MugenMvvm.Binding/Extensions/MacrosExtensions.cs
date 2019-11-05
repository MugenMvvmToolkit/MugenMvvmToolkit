using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
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
            var binding = metadata?.Get(BindingMetadata.Binding);
            if (binding == null)
                return metadata?.Get(BindingMetadata.EventArgs);

            var itemOrList = binding.GetComponents();
            var components = itemOrList.List;
            var component = itemOrList.Item;
            if (components == null)
                return (component as IHasEventArgsBindingComponent)?.EventArgs;

            for (var i = 0; i < components.Length; i++)
            {
                var args = (components[i] as IHasEventArgsBindingComponent)?.EventArgs;
                if (args != null)
                    return args;
            }

            return null;
        }

        #endregion
    }
}