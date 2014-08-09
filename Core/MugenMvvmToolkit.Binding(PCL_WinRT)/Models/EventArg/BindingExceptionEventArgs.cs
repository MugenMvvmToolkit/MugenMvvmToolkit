#region Copyright
// ****************************************************************************
// <copyright file="BindingExceptionEventArgs.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Models.EventArg
{
    public class BindingExceptionEventArgs : BindingEventArgs
    {
        #region Fields

        private readonly Exception _exception;
        private readonly Exception _originalException;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingExceptionEventArgs" /> class.
        /// </summary>
        public BindingExceptionEventArgs(BindingAction action, [NotNull] Exception exception, [NotNull] Exception originalException)
            : base(action)
        {
            Should.NotBeNull(exception, "exception");
            Should.NotBeNull(originalException, "originalException");
            _exception = exception;
            _originalException = originalException;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current <see cref="Exception" />.
        /// </summary>
        [NotNull]
        public Exception Exception
        {
            get { return _exception; }
        }

        /// <summary>
        ///     Gets the current <see cref="Exception" />.
        /// </summary>
        [NotNull]
        public Exception OriginalException
        {
            get { return _originalException; }
        }

        #endregion
    }
}