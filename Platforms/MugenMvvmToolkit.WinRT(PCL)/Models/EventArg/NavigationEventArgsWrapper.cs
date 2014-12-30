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

namespace MugenMvvmToolkit.Models.EventArg
{
    public sealed class NavigationEventArgsWrapper : NavigationEventArgsBase
    {
        #region Fields

        private readonly NavigationEventArgs _args;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigatingCancelEventArgsWrapper" /> class.
        /// </summary>
        public NavigationEventArgsWrapper([NotNull] NavigationEventArgs args)
        {
            Should.NotBeNull(args, "args");
            _args = args;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the original args.
        /// </summary>
        public NavigationEventArgs Args
        {
            get { return _args; }
        }

        #endregion

        #region Overrides of NavigationEventArgsBase

        /// <summary>
        ///     Gets the content of the target being navigated to.
        /// </summary>
        public override object Content
        {
            get { return _args.Content; }
        }

        /// <summary>
        ///     Gets a value that indicates the type of navigation that is occurring.
        /// </summary>
        public override NavigationMode Mode
        {
            get { return _args.NavigationMode.ToNavigationMode(); }
        }

        #endregion
    }
}