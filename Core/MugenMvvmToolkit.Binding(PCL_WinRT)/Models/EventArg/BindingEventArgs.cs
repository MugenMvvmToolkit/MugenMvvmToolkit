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

        public static BindingEventArgs SourceTrueArgs = new BindingEventArgs(BindingAction.UpdateSource, true);

        public static BindingEventArgs SourceFalseArgs = new BindingEventArgs(BindingAction.UpdateSource, false);

        public static BindingEventArgs TargetTrueArgs = new BindingEventArgs(BindingAction.UpdateTarget, true);

        public static BindingEventArgs TargetFalseArgs = new BindingEventArgs(BindingAction.UpdateTarget, false);

        private readonly BindingAction _action;
        private readonly bool _result;

        #endregion

        #region Constructors

        public BindingEventArgs(BindingAction action, bool result)
        {
            _action = action;
            _result = result;
        }

        #endregion

        #region Properties

        public BindingAction Action
        {
            get { return _action; }
        }

        public bool Result
        {
            get { return _result; }
        }

        #endregion
    }
}
