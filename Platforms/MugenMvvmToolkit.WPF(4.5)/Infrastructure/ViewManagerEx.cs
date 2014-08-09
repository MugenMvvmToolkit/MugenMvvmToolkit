#region Copyright
// ****************************************************************************
// <copyright file="ViewManagerEx.cs">
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
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using JetBrains.Annotations;
#if WINFORMS || ANDROID
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces.Models;
#endif
#if WINFORMS
using System.Windows.Forms;
#elif SILVERLIGHT
using System.Windows.Controls;
#elif WINDOWSCOMMON
using Windows.UI.Xaml;
using System.Runtime.InteropServices.WindowsRuntime;
#endif
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ViewManagerEx : ViewManager
    {
        #region Nested types
#if WPF
        internal sealed class WindowView : IWindowView, IViewWrapper
        {
        #region Fields

            private readonly Window _window;

            #endregion

        #region Constructors

            public WindowView(Window window)
            {
                Should.NotBeNull(window, "window");
                _window = window;
            }

            #endregion

        #region Implementation of IWindowView

            public object DataContext
            {
                get { return _window.DataContext; }
                set { _window.DataContext = value; }
            }

            public void Show()
            {
                _window.Show();
            }

            public bool? ShowDialog()
            {
                return _window.ShowDialog();
            }

            public void Close()
            {
                _window.Close();
            }

            event CancelEventHandler IWindowView.Closing
            {
                add { _window.Closing += value; }
                remove { _window.Closing -= value; }
            }

            public Type ViewType
            {
                get { return _window.GetType(); }
            }

            public object View
            {
                get { return _window; }
            }

            #endregion
        }
#elif WINFORMS
        private sealed class FormView : IWindowView, IDisposable, IViewWrapper
        {
        #region Fields

            private readonly Form _form;
            private readonly IBindingContext _context;

        #endregion

        #region Constructors

            public FormView(Form form)
            {
                Should.NotBeNull(form, "form");
                _form = form;
                _context = BindingProvider.Instance.ContextManager.GetBindingContext(form);
            }

        #endregion

        #region Implementation of IWindowView

            public object DataContext
            {
                get { return _context.DataContext; }
                set { _context.DataContext = value; }
            }

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

            public Type ViewType
            {
                get { return _form.GetType(); }
            }

            public object View
            {
                get { return _form; }
            }

        #endregion
        }
#elif SILVERLIGHT
        private sealed class WindowView : IWindowView, IViewWrapper
        {
        #region Fields

            private readonly ChildWindow _window;

        #endregion

        #region Constructors

            public WindowView(ChildWindow window)
            {
                Should.NotBeNull(window, "window");
                _window = window;
            }

        #endregion

        #region Implementation of IWindowView

            public object DataContext
            {
                get { return _window.DataContext; }
                set { _window.DataContext = value; }
            }

            public void Show()
            {
                _window.Show();
            }

            public void Close()
            {
                _window.Close();
            }

            event EventHandler<CancelEventArgs> IWindowView.Closing
            {
                add { _window.Closing += value; }
                remove { _window.Closing -= value; }
            }

            public Type ViewType
            {
                get { return _window.GetType(); }
            }

            public object View
            {
                get { return _window; }
            }

        #endregion
        }
#elif WINDOWSCOMMON
        private const string ContentDialogFullName = "Windows.UI.Xaml.Controls.ContentDialog";

        private sealed class WindowView : DisposableObject, IWindowView
        {
            #region Fields

            private static readonly MethodInfo OnClosingMethod = typeof(WindowView).GetMethodEx("OnClosing", MemberFlags.Instance | MemberFlags.NonPublic);
            private static EventInfo _closingEvent;
            private static PropertyInfo _cancelProperty;
            private static MethodInfo _showAsyncMethod;
            private static MethodInfo _hideMethod;

            private readonly FrameworkElement _window;
            private readonly EventRegistrationToken? _token;

            #endregion

            #region Constructors

            public WindowView(FrameworkElement window)
            {
                _window = window;
                if (_closingEvent == null)
                    _closingEvent = window.GetType().GetRuntimeEvent("Closing");

                var handler = ServiceProvider
                    .ReflectionManager
                    .TryCreateDelegate(_closingEvent.EventHandlerType, this, OnClosingMethod);
                if (handler == null)
                {
                    Tracer.Error("The provider cannot create delegate for event '{0}'", _closingEvent.EventHandlerType);
                    return;
                }
                _token = (EventRegistrationToken)_closingEvent.AddMethod.InvokeEx(window, handler);
            }

            #endregion

            #region Methods

            [UsedImplicitly]
            private void OnClosing(object sender, object args)
            {
                var handler = Closing;
                if (handler == null)
                    return;
                var eventArgs = new CancelEventArgs();
                handler(this, eventArgs);
                if (_cancelProperty == null)
                    _cancelProperty = args.GetType().GetPropertyEx("Cancel", MemberFlags.Instance | MemberFlags.Public);
                if (_cancelProperty != null)
                    _cancelProperty.SetValueEx(args, eventArgs.Cancel);
            }

            #endregion

            #region Implementation of IWindowView

            public object DataContext
            {
                get { return _window.DataContext; }
                set { _window.DataContext = value; }
            }

            public void Show()
            {
                if (_showAsyncMethod == null)
                    _showAsyncMethod = _window.GetType().GetMethodEx("ShowAsync", MemberFlags.Instance | MemberFlags.Public);
                if (_showAsyncMethod != null)
                    _showAsyncMethod.InvokeEx(_window);
            }

            public void Close()
            {
                if (_hideMethod == null)
                    _hideMethod = _window.GetType().GetMethodEx("Hide", MemberFlags.Instance | MemberFlags.Public);
                if (_hideMethod != null)
                    _hideMethod.InvokeEx(_window);
            }

            public event EventHandler<object, CancelEventArgs> Closing;

            protected override void OnDispose(bool disposing)
            {
                if (disposing)
                {
                    if (_token.HasValue)
                        _closingEvent.RemoveMethod.Invoke(_window, new object[] { _token.Value });
                }
                base.OnDispose(disposing);
            }

            #endregion
        }
#endif
        #endregion

        #region Constructors

#if WINFORMS || ANDROID
        static ViewManagerEx()
        {
            GetDataContext = o => BindingProvider.Instance.ContextManager.GetBindingContext(o).DataContext;
            SetDataContext = (o, o1) => BindingProvider.Instance.ContextManager.GetBindingContext(o).DataContext = o1;
        }
#endif
        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewManagerEx" /> class.
        /// </summary>
        public ViewManagerEx([NotNull] IThreadManager threadManager,
            [NotNull] IViewMappingProvider viewMappingProvider)
            : base(threadManager, viewMappingProvider)
        {
        }

        #endregion

        #region Methods

        internal static void Initialize()
        {
            //NOTE: to invoke static constructor.
        }

        #endregion

        #region Overrides of ViewManager

        /// <summary>
        ///     Gets the type of view wrapper.
        /// </summary>
        protected override Type GetViewTypeInternal(Type viewType, IDataContext dataContext)
        {
#if WPF
            if (typeof(Window).IsAssignableFrom(viewType))
                return typeof(WindowView);
#elif WINFORMS
            if (typeof(Form).IsAssignableFrom(viewType))
                return typeof(FormView);
#elif SILVERLIGHT
            if (typeof(ChildWindow).IsAssignableFrom(viewType))
                return typeof(WindowView);
#elif WINDOWSCOMMON
            if (IsContentDialog(viewType))
                return typeof(WindowView);
#endif
            return base.GetViewTypeInternal(viewType, dataContext);
        }

        /// <summary>
        ///     Wraps the specified view object to a <see cref="IView" />.
        /// </summary>
        protected override IView WrapToViewInternal(object view, IDataContext dataContext)
        {
#if WPF
            var window = view as Window;
            if (window != null)
                return new WindowView(window);
#elif WINFORMS
            var form = view as Form;
            if (form != null)
                return new FormView(form);
#elif SILVERLIGHT
            var window = view as ChildWindow;
            if (window != null)
                return new WindowView(window);
#elif WINDOWSCOMMON
            if (IsContentDialog(view.GetType()))
                return new WindowView((FrameworkElement)view);
#endif
            return base.WrapToViewInternal(view, dataContext);
        }

#if WINDOWSCOMMON
        private static bool IsContentDialog(Type type)
        {
            while (typeof(object) != type)
            {
                if (type.FullName == ContentDialogFullName)
                    return true;
                type = type.GetTypeInfo().BaseType;
            }
            return false;
        }
#endif

        #endregion
    }
}