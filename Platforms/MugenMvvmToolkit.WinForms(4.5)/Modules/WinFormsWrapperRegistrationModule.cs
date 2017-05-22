#region Copyright

// ****************************************************************************
// <copyright file="WinFormsWrapperRegistrationModule.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Modules;
using MugenMvvmToolkit.WinForms.Interfaces.Views;

namespace MugenMvvmToolkit.WinForms.Modules
{
    public class WinFormsWrapperRegistrationModule : WrapperRegistrationModuleBase
    {
        #region Nested types

        public class FormViewWrapper : IWindowView, IDisposable, IViewWrapper
        {
            #region Fields

            protected readonly Form Form;

            #endregion

            #region Constructors

            public FormViewWrapper(Form form)
            {
                Should.NotBeNull(form, nameof(form));
                Form = form;
            }

            #endregion

            #region Implementation of IWindowView

            public void Show()
            {
                Form.Show();
            }

            public DialogResult ShowDialog()
            {
                return Form.ShowDialog();
            }

            public void Close()
            {
                Form.Close();
            }

            public void Activate()
            {
                Form.Activate();
            }

            public object Owner
            {
                get { return Form.Owner; }
                set { Form.Owner = ToolkitExtensions.GetUnderlyingView<object>(value) as Form; }
            }

            event CancelEventHandler IWindowView.Closing
            {
                add { Form.Closing += value; }
                remove { Form.Closing -= value; }
            }

            public void Dispose()
            {
                Form.Dispose();
            }

            public object View => Form;

            #endregion
        }

        #endregion

        #region Overrides of WrapperRegistrationModuleBase

        protected override void RegisterWrappers(IConfigurableWrapperManager wrapperManager)
        {
            wrapperManager.AddWrapper<IWindowView, FormViewWrapper>(
                (type, context) => typeof(Form).IsAssignableFrom(type),
                (wrapper, context) => new FormViewWrapper((Form)wrapper));
        }

        #endregion
    }
}
