#region Copyright

// ****************************************************************************
// <copyright file="PlatformWrapperRegistrationModule.cs">
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
using System.ComponentModel;
using System.Windows.Forms;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Modules;
using MugenMvvmToolkit.WinForms.Interfaces.Views;

namespace MugenMvvmToolkit.WinForms.Modules
{
    public class PlatformWrapperRegistrationModule : WrapperRegistrationModuleBase
    {
        #region Nested types

        private sealed class FormViewWrapper : IWindowView, IDisposable, IViewWrapper
        {
            #region Fields

            private readonly Form _form;

            #endregion

            #region Constructors

            public FormViewWrapper(Form form)
            {
                Should.NotBeNull(form, "form");
                _form = form;
            }

            #endregion

            #region Implementation of IWindowView

            public void Show()
            {
                _form.Show();
            }

            public DialogResult ShowDialog()
            {
                return _form.ShowDialog();
            }

            public void Close()
            {
                _form.Close();
            }

            event CancelEventHandler IWindowView.Closing
            {
                add { _form.Closing += value; }
                remove { _form.Closing -= value; }
            }

            public void Dispose()
            {
                _form.Dispose();
            }

            public object View
            {
                get { return _form; }
            }

            #endregion
        }

        #endregion

        #region Overrides of WrapperRegistrationModuleBase

        /// <summary>
        ///     Registers the wrappers using <see cref="WrapperManager" /> class.
        /// </summary>
        protected override void RegisterWrappers(WrapperManager wrapperManager)
        {
            wrapperManager.AddWrapper<IWindowView, FormViewWrapper>(
                (type, context) => typeof(Form).IsAssignableFrom(type),
                (wrapper, context) => new FormViewWrapper((Form)wrapper));
        }

        #endregion
    }
}