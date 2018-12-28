using System;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ViewModelAttribute : Attribute
    {
        #region Constructors

        public ViewModelAttribute(Type viewModelType, string? name = null)
        {
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            Should.BeOfType<IViewModel>(viewModelType, "viewModelType");
            ViewModelType = viewModelType;
            Name = name;
        }

        #endregion

        #region Properties

        public Type ViewModelType { get; }

        public string? Name { get; }

        public string? Uri { get; set; }

        public UriKind UriKind { get; set; }

        #endregion
    }
}