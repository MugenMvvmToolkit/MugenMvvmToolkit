#region Copyright

// ****************************************************************************
// <copyright file="ViewModelAttribute.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ViewModelAttribute : Attribute
    {
        #region Fields

        private readonly Type _viewModelType;
        private readonly string _name;

        #endregion

        #region Constructor

        public ViewModelAttribute([NotNull, ViewModelTypeRequired] Type viewModelType, string name = null)
        {
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            Should.BeOfType<IViewModel>(viewModelType, "viewModelType");
            _viewModelType = viewModelType;
            _name = name;
        }

        #endregion

        #region Properties

        public string Uri { get; set; }

        [NotNull]
        public Type ViewModelType => _viewModelType;

        public UriKind UriKind { get; set; }

        public string Name => _name;

        #endregion
    }
}
