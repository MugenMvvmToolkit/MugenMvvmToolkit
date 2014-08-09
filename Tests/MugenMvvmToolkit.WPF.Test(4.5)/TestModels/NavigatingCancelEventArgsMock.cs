using System;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class NavigatingCancelEventArgsMock : NavigatingCancelEventArgsBase
    {
        #region Fields

        private readonly bool _isCancelable;
        private readonly NavigationMode _mode;

        #endregion

        #region Constructors

        public NavigatingCancelEventArgsMock(NavigationMode mode, bool isCancelable)
        {
            _mode = mode;
            _isCancelable = isCancelable;
        }

        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        /// <summary>
        ///     Specifies whether a pending navigation should be canceled.
        /// </summary>
        /// <returns>
        ///     true to cancel the pending cancelable navigation; false to continue with navigation.
        /// </returns>
        public override bool Cancel { get; set; }

        /// <summary>
        ///     Gets a value that indicates the type of navigation that is occurring.
        /// </summary>
        public override NavigationMode NavigationMode
        {
            get { return _mode; }
        }

        /// <summary>
        ///     Gets a value that indicates whether you can cancel the navigation.
        /// </summary>
        public override bool IsCancelable
        {
            get { return _isCancelable; }
        }

        #endregion
    }
}