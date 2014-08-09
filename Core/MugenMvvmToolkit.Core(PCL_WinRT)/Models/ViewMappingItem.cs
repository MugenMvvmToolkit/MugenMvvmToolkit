#region Copyright
// ****************************************************************************
// <copyright file="ViewMappingItem.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
    /// <summary>
    ///     Represents the interface which contains information about binding a view to a view model.
    /// </summary>
    public class ViewMappingItem : IViewMappingItem
    {
        #region Fields

        private static readonly Uri Empty = new Uri("app://empty/", UriKind.Absolute);

        private readonly string _name;
        private readonly Uri _uri;
        private readonly Type _viewModelType;
        private readonly Type _viewType;

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewMappingItem" /> class.
        /// </summary>
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

        /// <summary>
        ///     Gets the name of mapping.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Gets the type of view.
        /// </summary>
        public Type ViewType
        {
            get { return _viewType; }
        }

        /// <summary>
        ///     Gets or sets the type of view model.
        /// </summary>
        public Type ViewModelType
        {
            get { return _viewModelType; }
        }

        /// <summary>
        ///     Gets the uri, if any.
        /// </summary>
        public Uri Uri
        {
            get { return _uri; }
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            return string.Format("View: {0}, ViewModelType: {1}, Name: {2}, Uri: {3}", ViewType, ViewModelType, Name,
                Uri);
        }

        #endregion
    }
}