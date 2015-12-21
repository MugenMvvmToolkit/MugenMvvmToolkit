#region Copyright

// ****************************************************************************
// <copyright file="NavigationEventArgsWrapper.cs">
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

using Windows.UI.Xaml.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models.EventArg;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

namespace MugenMvvmToolkit.WinRT.Models.EventArg
{
    public sealed class NavigationEventArgsWrapper : NavigationEventArgsBase
    {
        #region Fields

        private readonly NavigationEventArgs _args;
        private readonly bool _bringToFront;

        #endregion

        #region Constructors

        public NavigationEventArgsWrapper([NotNull] NavigationEventArgs args, bool bringToFront)
        {
            Should.NotBeNull(args, nameof(args));
            _args = args;
            _bringToFront = bringToFront;
        }

        #endregion

        #region Properties

        public NavigationEventArgs Args
        {
            get { return _args; }
        }

        #endregion

        #region Overrides of NavigationEventArgsBase

        public override object Content
        {
            get { return _args.Content; }
        }

        public override NavigationMode Mode
        {
            get
            {
                var mode = _args.NavigationMode.ToNavigationMode();
                if (_bringToFront && mode == NavigationMode.New)
                    return NavigationMode.Refresh;
                return mode;
            }
        }

        #endregion
    }
}
