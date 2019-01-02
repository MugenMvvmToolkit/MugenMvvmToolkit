using System;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewMappingInfo
    {
        string? Name { get; }

        Type ViewType { get; }

        Type ViewModelType { get; }

        string? Uri { get; }

        UriKind UriKind { get; }
    }
}