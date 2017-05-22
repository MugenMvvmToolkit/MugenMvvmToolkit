#region Copyright

// ****************************************************************************
// <copyright file="WpfWrapperRegistrationModule.cs">
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

using System.ComponentModel;
using System.Windows;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Modules;
using MugenMvvmToolkit.WPF.Interfaces.Views;

namespace MugenMvvmToolkit.WPF.Modules
{
    public class WpfWrapperRegistrationModule : WrapperRegistrationModuleBase
    {
        #region Nested types

        public class WindowViewWrapper : IWindowView, IViewWrapper
        {
            #region Fields

            protected readonly Window Window;

            #endregion

            #region Constructors

            public WindowViewWrapper(Window window)
            {
                Should.NotBeNull(window, nameof(window));
                Window = window;
            }

            #endregion

            #region Implementation of IWindowView

            public object View => Window;

            public void Show()
            {
                Window.Show();
            }

            public bool? ShowDialog()
            {
                return Window.ShowDialog();
            }

            public void Close()
            {
                Window.Close();
            }

            public bool Activate()
            {
                return Window.Activate();
            }

            public object Owner
            {
                get { return Window.Owner; }
                set { Window.Owner = ToolkitExtensions.GetUnderlyingView<object>(value) as Window; }
            }

            event CancelEventHandler IWindowView.Closing
            {
                add { Window.Closing += value; }
                remove { Window.Closing -= value; }
            }

            #endregion
        }

        #endregion

        #region Overrides of WrapperRegistrationModuleBase

        protected override void RegisterWrappers(IConfigurableWrapperManager wrapperManager)
        {
            wrapperManager.AddWrapper<IWindowView, WindowViewWrapper>(
                (type, context) => typeof(Window).IsAssignableFrom(type),
                (o, context) => new WindowViewWrapper((Window)o));
        }

        #endregion
    }
}
