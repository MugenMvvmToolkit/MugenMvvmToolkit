#region Copyright

// ****************************************************************************
// <copyright file="NavigatingCancelEventArgs.cs">
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

#if TOUCH
namespace MugenMvvmToolkit.iOS.Models.EventArg
#else
namespace MugenMvvmToolkit.Xamarin.Forms.Models.EventArg
#endif

{
    public class NavigatingCancelEventArgs : NavigatingCancelEventArgsBase
    {
        #region Fields

#if !TOUCH
        private readonly bool _isBackButton;
#endif
        private readonly bool _isCancelable;
        private readonly IViewMappingItem _mapping;
        private readonly NavigationMode _navigationMode;
        private readonly string _parameter;

        #endregion

        #region Constructors

#if TOUCH
        public NavigatingCancelEventArgs(IViewMappingItem mapping, NavigationMode navigationMode, string parameter)
        {
            _mapping = mapping;
            _navigationMode = navigationMode;
            _parameter = parameter;
            _isCancelable = true;
        }
#else
        public NavigatingCancelEventArgs(IViewMappingItem mapping, NavigationMode navigationMode, string parameter, bool isCancelable, bool isBackButton)
        {
            _mapping = mapping;
            _navigationMode = navigationMode;
            _parameter = parameter;
            _isCancelable = isCancelable;
            _isBackButton = isBackButton;
        }
#endif


        #endregion

        #region Properties

        [CanBeNull]
        public IViewMappingItem Mapping
        {
            get { return _mapping; }
        }

        public string Parameter
        {
            get { return _parameter; }
        }

#if !TOUCH
        public bool IsBackButtonNavigation
        {
            get { return _isBackButton; }
        }

#endif
        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        public override bool Cancel { get; set; }

        public override NavigationMode NavigationMode
        {
            get { return _navigationMode; }
        }

        public override bool IsCancelable
        {
            get { return _isCancelable; }
        }

        #endregion
    }
}
