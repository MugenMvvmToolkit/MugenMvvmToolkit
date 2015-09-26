#region Copyright

// ****************************************************************************
// <copyright file="NavigatingCancelEventArgsWrapper.cs">
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
    public sealed class NavigatingCancelEventArgsWrapper : NavigatingCancelEventArgsBase
    {
        #region Fields

        private readonly NavigatingCancelEventArgs _args;
        private readonly string _parameter;

        #endregion

        #region Constructors

        public NavigatingCancelEventArgsWrapper([NotNull] NavigatingCancelEventArgs args, string parameter)
        {
            Should.NotBeNull(args, "args");
            _args = args;
            _parameter = parameter;
        }

        #endregion

        #region Properties

        public NavigatingCancelEventArgs Args
        {
            get { return _args; }
        }

        public string Parameter
        {
            get { return _parameter; }
        }

        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        public override bool Cancel
        {
            get { return _args.Cancel; }
            set { _args.Cancel = value; }
        }

        public override NavigationMode NavigationMode
        {
            get { return _args.NavigationMode.ToNavigationMode(); }
        }

        public override bool IsCancelable
        {
            get { return true; }
        }

        #endregion
    }
}
