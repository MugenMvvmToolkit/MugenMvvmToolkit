#region Copyright

// ****************************************************************************
// <copyright file="ViewMappingItem.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models
{
    public class ViewMappingItem : IViewMappingItem
    {
        #region Fields

        private static readonly Uri Empty;

        private readonly string _name;
        private readonly Uri _uri;
        private readonly Type _viewModelType;
        private readonly Type _viewType;

        #endregion

        #region Constructor

        static ViewMappingItem()
        {
            Empty = new Uri("app://empty/", UriKind.Absolute);
        }

        public ViewMappingItem(Type viewModelType, Type viewType, string name, Uri uri)
        {
            Should.BeOfType<IViewModel>(viewModelType, "viewModelType");
            Should.NotBeNull(viewType, "viewType");
            _viewModelType = viewModelType;
            _viewType = viewType;
            _name = name;
            _uri = uri ?? Empty;
        }

        #endregion

        #region Implementation of IViewPageMappingItem

        public string Name
        {
            get { return _name; }
        }

        public Type ViewType
        {
            get { return _viewType; }
        }

        public Type ViewModelType
        {
            get { return _viewModelType; }
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return string.Format("View: {0}, ViewModelType: {1}, Name: {2}, Uri: {3}", ViewType, ViewModelType, Name,
                Uri);
        }

        #endregion
    }
}
