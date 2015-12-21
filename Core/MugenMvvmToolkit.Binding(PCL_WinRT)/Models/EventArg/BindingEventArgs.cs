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
using JetBrains.Annotations;

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
        private readonly Exception _exception;
        private readonly Exception _originalException;

        #endregion

        #region Constructors

        public BindingEventArgs(BindingAction action, bool result)
        {
            _action = action;
            _result = result;
        }

        public BindingEventArgs(BindingAction action, [NotNull] Exception exception, [NotNull] Exception originalException)
            : this(action, false)
        {
            Should.NotBeNull(exception, nameof(exception));
            Should.NotBeNull(originalException, nameof(originalException));
            _exception = exception;
            _originalException = originalException;
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

        [CanBeNull]
        public Exception Exception
        {
            get { return _exception; }
        }

        [CanBeNull]
        public Exception OriginalException
        {
            get { return _originalException; }
        }


        #endregion
    }
}
