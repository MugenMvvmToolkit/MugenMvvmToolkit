#region Copyright

// ****************************************************************************
// <copyright file="NavigationContext.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models
{
    public class NavigationContext : DataContext, INavigationContext
    {
        #region Fields

        private readonly NavigationMode _navigationMode;
        private readonly object _navigationProvider;
        private readonly NavigationType _type;
        private readonly IViewModel _viewModelFrom;
        private readonly IViewModel _viewModelTo;

        #endregion

        #region Constructors

        public NavigationContext([NotNull] NavigationType type, NavigationMode navigationMode, [CanBeNull] IViewModel viewModelFrom, [CanBeNull] IViewModel viewModelTo,
             [CanBeNull] object navigationProvider)
        {
            Should.NotBeNull(type, "type");
            _type = type;
            _navigationMode = navigationMode;
            _navigationProvider = navigationProvider;
            _viewModelFrom = viewModelFrom;
            _viewModelTo = viewModelTo;
        }

        #endregion

        #region Implementation of INavigationContext

        public NavigationMode NavigationMode
        {
            get { return _navigationMode; }
        }

        public IViewModel ViewModelFrom
        {
            get { return _viewModelFrom; }
        }

        public IViewModel ViewModelTo
        {
            get { return _viewModelTo; }
        }

        public NavigationType NavigationType
        {
            get { return _type; }
        }

        public object NavigationProvider
        {
            get { return _navigationProvider; }
        }

        #endregion
    }
}
