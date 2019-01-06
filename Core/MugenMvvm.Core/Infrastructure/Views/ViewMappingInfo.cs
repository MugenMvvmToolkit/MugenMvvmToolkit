using System;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Infrastructure.Views
{
    public class ViewMappingInfo : IViewMappingInfo
    {
        #region Constructors

        public ViewMappingInfo(Type viewModelType, Type viewType, string? name, string? uri, UriKind uriKind)
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

        #region Properties

        public string? Name { get; }

        public Type ViewType { get; }

        public Type ViewModelType { get; }

        public string? Uri { get; }

        public UriKind UriKind { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"View: {ViewType}, ViewModelType: {ViewModelType}, Name: {Name}, Uri: {Uri}";
        }

        #endregion
    }
}