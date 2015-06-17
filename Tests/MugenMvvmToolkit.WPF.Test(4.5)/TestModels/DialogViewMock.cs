using System;
using System.ComponentModel;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Silverlight.Interfaces.Views;
using MugenMvvmToolkit.WPF.Interfaces.Views;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class DialogViewMock : IWindowView
    {
        #region Fields

        private bool _closing;

#if WPF
        private event CancelEventHandler CancelEventHandler;
#endif

        private event EventHandler<CancelEventArgs> CancelChildEventHandler;

        #endregion

        #region Properties

        public bool IsShowAny
        {
            get { return IsShow || IsShowDialog; }
        }

        public bool IsShow { get; set; }

        public bool IsShowDialog { get; set; }

        public bool IsClose { get; set; }

        #endregion

        #region Methods

#if WPF
        public void OnClosingDialog(CancelEventArgs e)
        {
            CancelEventHandler handler = CancelEventHandler;
            if (handler != null) handler(this, e);
        }
#endif

        public void OnClosingChildDialog(CancelEventArgs e)
        {
            EventHandler<CancelEventArgs> handler = CancelChildEventHandler;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Implementation of IDialogViewBase

        /// <summary>
        ///     Shows dialog as a window.
        /// </summary>
        public void Show()
        {
            IsShow = true;
        }

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        public void Close()
        {
            if (_closing || IsClose)
                throw new InvalidOperationException("The view is already closed.");
            _closing = true;
            using (new ActionToken(() => _closing = false))
            {
                var cancelEventArgs = new CancelEventArgs(false);
#if WPF
                OnClosingDialog(cancelEventArgs);
                if (cancelEventArgs.Cancel) return;
#endif

                OnClosingChildDialog(cancelEventArgs);
                if (cancelEventArgs.Cancel) return;
                IsClose = true;
            }
        }

        #endregion

        #region Implementation of IDialogView

#if WPF
        event CancelEventHandler IWindowView.Closing
        {
            add { CancelEventHandler += value; }
            remove { CancelEventHandler -= value; }
        }
#else
        event EventHandler<CancelEventArgs> IWindowView.Closing
        {
            add { CancelChildEventHandler += value; }
            remove { CancelChildEventHandler -= value; }
        }
#endif


        /// <summary>
        ///     Shows dialog as dialog window.
        /// </summary>
        /// <returns></returns>
        public bool? ShowDialog()
        {
            IsShowDialog = true;
            return true;
        }

        #endregion
    }
}