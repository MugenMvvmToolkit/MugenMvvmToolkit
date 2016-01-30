#region Copyright

// ****************************************************************************
// <copyright file="NavigatingCancelEventArgs.cs">
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

using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Android.Models.EventArg
{
    public class NavigatingCancelEventArgs : NavigatingCancelEventArgsBase
    {
        #region Fields

        public static readonly NavigatingCancelEventArgs NonCancelableEventArgs;

        private readonly bool _isCancelable;
        private readonly IViewMappingItem _mapping;
        private readonly NavigationMode _navigationMode;
        private readonly string _parameter;

        #endregion

        #region Constructors

        static NavigatingCancelEventArgs()
        {
            NonCancelableEventArgs = new NavigatingCancelEventArgs();
        }

        private NavigatingCancelEventArgs()
        {
            _navigationMode = NavigationMode.New;
            _isCancelable = false;
        }

        public NavigatingCancelEventArgs(NavigationMode navigationMode)
        {
            _navigationMode = navigationMode;
            _isCancelable = true;
        }

        public NavigatingCancelEventArgs(IViewMappingItem mapping, NavigationMode navigationMode, string parameter)
        {
            _mapping = mapping;
            _navigationMode = navigationMode;
            _parameter = parameter;
            _isCancelable = true;
        }

        #endregion

        #region Properties

        public IViewMappingItem Mapping => _mapping;

        public string Parameter => _parameter;

        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        public override bool Cancel { get; set; }

        public override NavigationMode NavigationMode => _navigationMode;

        public override bool IsCancelable => _isCancelable;

        #endregion
    }
}
