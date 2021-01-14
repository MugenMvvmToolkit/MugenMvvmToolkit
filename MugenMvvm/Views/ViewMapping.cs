using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Views
{
    public class ViewMapping : IViewMapping
    {
        public static readonly IViewMapping Undefined = new ViewMapping("-", typeof(IViewModelBase), typeof(object));

        public ViewMapping(string id, Type viewModelType, Type viewType, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(id, nameof(id));
            Should.NotBeNull(viewType, nameof(viewType));
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            Id = id;
            ViewType = viewType;
            ViewModelType = viewModelType;
            Metadata = metadata.DefaultIfNull();
        }

        public string Id { get; }

        public bool HasMetadata => Metadata.Count != 0;

        public IReadOnlyMetadataContext Metadata { get; }

        public Type ViewType { get; }

        public Type ViewModelType { get; }
    }
}