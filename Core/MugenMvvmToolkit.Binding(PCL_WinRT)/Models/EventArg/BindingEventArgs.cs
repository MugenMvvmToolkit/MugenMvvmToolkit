#region Copyright
// ****************************************************************************
// <copyright file="BindingEventArgs.cs">
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

namespace MugenMvvmToolkit.Binding.Models.EventArg
{
    public class BindingEventArgs : EventArgs
    {
        #region Fields

        private readonly BindingAction _action;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingEventArgs" /> class.
        /// </summary>
        public BindingEventArgs(BindingAction action)
        {
            _action = action;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current <see cref="BindingAction" />.
        /// </summary>
        public BindingAction Action
        {
            get { return _action; }
        }

        #endregion
    }
}