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

using System.Windows.Navigation;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Models.EventArg
{
    public sealed class NavigatingCancelEventArgsWrapper : NavigatingCancelEventArgsBase
    {
        #region Fields

        private readonly NavigatingCancelEventArgs _args;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigatingCancelEventArgsWrapper" /> class.
        /// </summary>
        public NavigatingCancelEventArgsWrapper([NotNull] NavigatingCancelEventArgs args)
        {
            Should.NotBeNull(args, "args");
            _args = args;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the original args.
        /// </summary>
        public NavigatingCancelEventArgs Args
        {
            get { return _args; }
        }

        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        /// <summary>
        ///     Specifies whether a pending navigation should be canceled.
        /// </summary>
        /// <returns>
        ///     true to cancel the pending cancelable navigation; false to continue with navigation.
        /// </returns>
        public override bool Cancel
        {
            get { return _args.Cancel; }
            set { _args.Cancel = value; }
        }

        /// <summary>
        ///     Gets a value that indicates the type of navigation that is occurring.
        /// </summary>
        public override NavigationMode NavigationMode
        {
            get { return _args.NavigationMode.ToNavigationMode(); }
        }

        /// <summary>
        ///     Gets a value that indicates whether you can cancel the navigation.
        /// </summary>
        public override bool IsCancelable
        {
            get { return true; }
        }

        #endregion
    }
}