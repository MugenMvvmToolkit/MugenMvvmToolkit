#region Copyright

// ****************************************************************************
// <copyright file="NavigatingCancelEventArgsWrapper.cs">
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

using System.Windows.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models.EventArg;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;
#if SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Models.EventArg
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Models.EventArg
#endif
{
    public sealed class NavigatingCancelEventArgsWrapper : NavigatingCancelEventArgsBase
    {
        #region Fields

        private readonly NavigatingCancelEventArgs _args;

        #endregion

        #region Constructors

        public NavigatingCancelEventArgsWrapper([NotNull] NavigatingCancelEventArgs args)
        {
            Should.NotBeNull(args, nameof(args));
            _args = args;
        }

        #endregion

        #region Properties

        public NavigatingCancelEventArgs Args => _args;

        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        public override bool Cancel
        {
            get { return _args.Cancel; }
            set { _args.Cancel = value; }
        }

        public override NavigationMode NavigationMode => _args.NavigationMode.ToNavigationMode();

        public override bool IsCancelable => _args.IsCancelable;

        #endregion
    }
}
