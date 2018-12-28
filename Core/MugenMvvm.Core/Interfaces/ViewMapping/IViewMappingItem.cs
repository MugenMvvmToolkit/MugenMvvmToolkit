using System;

namespace MugenMvvm.Interfaces.ViewMapping
{
    public interface IViewMappingItem
    {
        string? Name { get; }

        Type ViewType { get; }

        Type ViewModelType { get; }

        string? Uri { get; }

        UriKind UriKind { get; }
    }
}