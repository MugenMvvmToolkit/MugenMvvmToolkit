using System;
using MugenMvvm.Interfaces.ViewMapping;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Infrastructure.ViewMapping
{
    public class ViewMappingItem : IViewMappingItem
    {
        #region Constructors

        public ViewMappingItem(Type viewModelType, Type viewType, string? name, string? uri, UriKind uriKind)
        {
            Should.BeOfType<IViewModel>(viewModelType, "viewModelType");
            Should.NotBeNull(viewType, nameof(viewType));
            ViewModelType = viewModelType;
            ViewType = viewType;
            Name = name;
            Uri = uri;
            UriKind = uriKind;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"View: {ViewType}, ViewModelType: {ViewModelType}, Name: {Name}, Uri: {Uri}";
        }

        #endregion

        #region Implementation of IViewPageMappingItem

        public string? Name { get; }

        public Type ViewType { get; }

        public Type ViewModelType { get; }

        public string? Uri { get; }

        public UriKind UriKind { get; }

        #endregion
    }
}