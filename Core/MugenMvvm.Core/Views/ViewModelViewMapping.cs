using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Views
{
    public class ViewModelViewMapping : IViewModelViewMapping
    {
        #region Constructors

        public ViewModelViewMapping(string id, Type viewType, Type viewModelType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(id, nameof(id));
            Should.NotBeNull(viewType, nameof(viewType));
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            Id = id;
            ViewType = viewType;
            ViewModelType = viewModelType;
            Metadata = metadata.DefaultIfNull();
        }

        #endregion

        #region Properties

        public bool HasMetadata => Metadata.Count != 0;

        public IReadOnlyMetadataContext Metadata { get; }

        public string Id { get; }

        public Type ViewType { get; }

        public Type ViewModelType { get; }

        #endregion
    }
}