#region Copyright
// ****************************************************************************
// <copyright file="NavigationEventArgsBase.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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

namespace MugenMvvmToolkit.Models.EventArg
{
    public abstract class NavigationEventArgsBase : EventArgs
    {
        /// <summary>
        ///     Gets the content of the target being navigated to.
        /// </summary>
        public abstract object Content { get; }

        /// <summary>
        ///     Gets a value that indicates the type of navigation that is occurring.
        /// </summary>
        public abstract NavigationMode Mode { get; }
    }
}