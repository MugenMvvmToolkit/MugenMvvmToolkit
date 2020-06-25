using System;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Resources.Internal
{
    public class TestResourceResolverComponent : IResourceResolverComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<string, object?, Type, IReadOnlyMetadataContext?, IResourceValue?>? TryGetResourceValue { get; set; }

        #endregion

        #region Implementation of interfaces

        IResourceValue? IResourceResolverComponent.TryGetResourceValue<TState>(string name, in TState state, IReadOnlyMetadataContext? metadata)
        {
            return TryGetResourceValue?.Invoke(name, state, typeof(TState), metadata);
        }

        #endregion
    }
}