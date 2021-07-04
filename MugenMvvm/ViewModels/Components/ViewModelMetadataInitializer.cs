using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.ViewModels.Components
{
    public sealed class ViewModelMetadataInitializer : IViewModelLifecycleListener, IHasPriority
    {
        public ViewModelMetadataInitializer()
        {
            MetadataMergeKeys = new List<IMetadataContextKey>(2)
            {
                ViewModelMetadata.ParentViewModel
            };
        }

        public List<IMetadataContextKey> MetadataMergeKeys { get; }

        public int Priority { get; init; } = ViewModelComponentPriority.PreInitializer;

        private static bool ContainsKey(IViewModelBase viewModel, IMetadataContextKey key) => viewModel.HasMetadata && viewModel.Metadata.Contains(key);

        public void OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            if (metadata == null || MetadataMergeKeys.Count == 0 || lifecycleState != ViewModelLifecycleState.Created)
                return;

            for (var i = 0; i < MetadataMergeKeys.Count; i++)
            {
                var key = MetadataMergeKeys[i];
                if (ContainsKey(viewModel, key))
                    continue;

                if (!metadata.TryGetRaw(key, out var v))
                    continue;

                viewModel.Metadata.Merge(new KeyValuePair<IMetadataContextKey, object?>(key, v));
            }
        }
    }
}