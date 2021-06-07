using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Components
{
    public sealed class SealedComponentConfigurationChecker : IComponentCollectionChangedListener
    {
        public static readonly SealedComponentConfigurationChecker Instance = new();

        private SealedComponentConfigurationChecker()
        {
        }

        public void OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            throw new InvalidOperationException(nameof(SealedComponentConfigurationChecker));

        public void OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            throw new InvalidOperationException(nameof(SealedComponentConfigurationChecker));
    }
}