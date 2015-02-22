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
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Modules
{
    public class PlatformWrapperRegistrationModule : WrapperRegistrationModuleBase
    {
        #region Nested types

        private sealed class WindowViewWrapper : IDisposable, IWindowView, IViewWrapper
        {
            #region Fields

            private static readonly MethodInfo OnClosingMethod;
            private static EventInfo _closingEvent;
            private static PropertyInfo _cancelProperty;
            private static MethodInfo _showAsyncMethod;
            private static MethodInfo _hideMethod;

            private readonly EventRegistrationToken? _token;
            private readonly FrameworkElement _window;

            #endregion

            #region Constructors

            static WindowViewWrapper()
            {
                OnClosingMethod = typeof(WindowViewWrapper).GetMethodEx("OnClosing",
                    MemberFlags.Instance | MemberFlags.NonPublic);
            }

            public WindowViewWrapper(FrameworkElement window)
            {
                _window = window;
                if (_closingEvent == null)
                    _closingEvent = window.GetType().GetRuntimeEvent("Closing");

                Delegate handler = ServiceProvider
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
                EventHandler<object, CancelEventArgs> handler = Closing;
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

            public void Show()
            {
                if (_showAsyncMethod == null)
                    _showAsyncMethod = _window.GetType()
                        .GetMethodEx("ShowAsync", MemberFlags.Instance | MemberFlags.Public);
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

            #endregion

            #region Implementation of IViewWrapper

            public object View
            {
                get { return _window; }
            }

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
                if (_token.HasValue)
                    _closingEvent.RemoveMethod.Invoke(_window, new object[] { _token.Value });
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ContentDialogFullName = "Windows.UI.Xaml.Controls.ContentDialog";

        #endregion

        #region Overrides of WrapperRegistrationModuleBase

        /// <summary>
        ///     Registers the wrappers using <see cref="WrapperManager" /> class.
        /// </summary>
        protected override void RegisterWrappers(WrapperManager wrapperManager)
        {
            wrapperManager.AddWrapper<IWindowView, WindowViewWrapper>(IsContentDialog,
                (o, context) => new WindowViewWrapper((FrameworkElement)o));
        }

        #endregion

        #region Methods

        private static bool IsContentDialog(Type type, IDataContext context)
        {
            while (typeof(object) != type)
            {
                if (type.FullName == ContentDialogFullName)
                    return true;
                type = type.GetTypeInfo().BaseType;
            }
            return false;
        }

        #endregion
    }
}