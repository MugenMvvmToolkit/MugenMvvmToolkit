#region Copyright

// ****************************************************************************
// <copyright file="NavigationEventArgs.cs">
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
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Android.Models.EventArg
{
    /// <summary>
    ///     Provides data for navigation methods and event handlers that cannot cancel the navigation request.
    /// </summary>
    public class NavigationEventArgs : NavigationEventArgsBase
    {
        #region Fields

        private readonly object _content;
        private readonly NavigationMode _navigationMode;
        private readonly string _parameter;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationEventArgs" /> class.
        /// </summary>
        public NavigationEventArgs([NotNull] object content, string parameter, NavigationMode navigationMode)
        {
            _content = content;
            _navigationMode = navigationMode;
            _parameter = parameter;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets any Parameter object passed to the target page for the navigation.
        /// </summary>
        public string Parameter
        {
            get { return _parameter; }
        }

        #endregion

        #region Overrides of NavigationEventArgsBase

        /// <summary>
        ///     Gets the content of the target being navigated to.
        /// </summary>
        public override object Content
        {
            get { return _content; }
        }

        /// <summary>
        ///     Gets a value that indicates the type of navigation that is occurring.
        /// </summary>
        public override NavigationMode Mode
        {
            get { return _navigationMode; }
        }

        #endregion
    }
}