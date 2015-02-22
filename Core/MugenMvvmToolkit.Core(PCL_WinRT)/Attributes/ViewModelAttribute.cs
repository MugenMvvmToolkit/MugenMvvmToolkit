#region Copyright

// ****************************************************************************
// <copyright file="ViewModelAttribute.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Attributes
{
    /// <summary>
    ///     Attribute class used to specify a specific View Model derivement or visual representation to be used on the target element.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ViewModelAttribute : Attribute
    {
        #region Fields

        private readonly Type _viewModelType;
        private readonly string _name;

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModelAttribute" /> class.
        /// </summary>
        /// <param name="viewModelType">The specified type of view model.</param>
        /// <param name="name">The name of view mapping</param>
        public ViewModelAttribute([NotNull, ViewModelTypeRequired] Type viewModelType, string name = null)
        {
            Should.NotBeNull(viewModelType, "viewModelType");
            Should.BeOfType<IViewModel>(viewModelType, "viewModelType");
            _viewModelType = viewModelType;
            _name = name;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the url.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        ///     Gets the type of view model.
        /// </summary>
        [NotNull]
        public Type ViewModelType
        {
            get { return _viewModelType; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="UriKind" />.
        /// </summary>
        public UriKind UriKind { get; set; }

        /// <summary>
        ///     Gets or sets the name of view binding.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        #endregion
    }
}