#region Copyright

// ****************************************************************************
// <copyright file="BindingEventArgs.cs">
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

using System;

namespace MugenMvvmToolkit.Binding.Models.EventArg
{
    public class BindingEventArgs : EventArgs
    {
        #region Fields

        /// <summary>
        ///     Gets an instance of <see cref="BindingEventArgs" /> with Action = <see cref="BindingAction.UpdateSource" /> and
        ///     Result = <c>true</c>.
        /// </summary>
        public static BindingEventArgs SourceTrueArgs = new BindingEventArgs(BindingAction.UpdateSource, true);

        /// <summary>
        ///     Gets an instance of <see cref="BindingEventArgs" /> with Action = <see cref="BindingAction.UpdateSource" /> and
        ///     Result = <c>false</c>.
        /// </summary>
        public static BindingEventArgs SourceFalseArgs = new BindingEventArgs(BindingAction.UpdateSource, false);

        /// <summary>
        ///     Gets an instance of <see cref="BindingEventArgs" /> with Action = <see cref="BindingAction.UpdateTarget" /> and
        ///     Result = <c>true</c>.
        /// </summary>
        public static BindingEventArgs TargetTrueArgs = new BindingEventArgs(BindingAction.UpdateTarget, true);

        /// <summary>
        ///     Gets an instance of <see cref="BindingEventArgs" /> with Action = <see cref="BindingAction.UpdateTarget" /> and
        ///     Result = <c>false</c>.
        /// </summary>
        public static BindingEventArgs TargetFalseArgs = new BindingEventArgs(BindingAction.UpdateTarget, false);

        private readonly BindingAction _action;
        private readonly bool _result;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingEventArgs" /> class.
        /// </summary>
        public BindingEventArgs(BindingAction action, bool result)
        {
            _action = action;
            _result = result;
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

        /// <summary>
        ///     Gets the result of operation.
        /// </summary>
        public bool Result
        {
            get { return _result; }
        }

        #endregion
    }
}