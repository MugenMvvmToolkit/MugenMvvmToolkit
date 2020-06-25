using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewModelViewMapping : IMetadataOwner<IReadOnlyMetadataContext>, IHasId<string>
    {
        Type ViewType { get; }

        Type ViewModelType { get; }
    }
}