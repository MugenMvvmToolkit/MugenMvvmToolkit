#region Copyright

// ****************************************************************************
// <copyright file="InitializationModuleEx.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Xamarin.Forms.Modules;
using Xamarin.Forms;
#if ANDROID && XAMARIN_FORMS
using Android.App;
using Android.Content;
using MugenMvvmToolkit.Xamarin.Forms.Android.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.Android.Infrastructure.Presenters;
namespace MugenMvvmToolkit.Xamarin.Forms.Android.Modules
#elif TOUCH
using MugenMvvmToolkit.Xamarin.Forms.iOS.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Xamarin.Forms.iOS.Modules
#elif WINDOWS_PHONE
using Microsoft.Phone.Controls;
using MugenMvvmToolkit.Xamarin.Forms.WinPhone.Infrastructure.Presenters;
using Application = System.Windows.Application;

namespace MugenMvvmToolkit.Xamarin.Forms.WinPhone.Modules
#elif WINDOWS_UWP
using MugenMvvmToolkit.Xamarin.Forms.UWP.Infrastructure.Presenters;
using Application = Windows.UI.Xaml.Application;

namespace MugenMvvmToolkit.Xamarin.Forms.UWP.Modules
#elif NETFX_CORE
using MugenMvvmToolkit.Xamarin.Forms.WinRT.Infrastructure.Presenters;
using Application = Windows.UI.Xaml.Application;

namespace MugenMvvmToolkit.Xamarin.Forms.WinRT.Modules
#endif
{
    public class InitializationModuleEx : XamarinFormsInitializationModule
    {
        #region Methods

        public override bool Load(IModuleContext context)
        {//todo move to ext method
#if ANDROID
            XamarinFormsExtensions.SendBackButtonPressed = page =>
            {
                if (!IsLastPage(page))
                    return null;
                var activity = GetActivity(global::Xamarin.Forms.Forms.Context);
                if (activity == null)
                    return null;
                return activity.OnBackPressed;
            };
#elif WINDOWS_UWP || NETFX_CORE
            XamarinFormsExtensions.SendBackButtonPressed = page =>
            {
                if (!IsLastPage(page))
                    return null;
                var application = Application.Current;
                if (application == null)
                    return null;
                return application.Exit;
            };
#elif WINDOWS_PHONE
            XamarinFormsExtensions.SendBackButtonPressed = page =>
            {
                if (!IsLastPage(page))
                    return null;
                var application = Application.Current;
                if (application == null)
                    return null;
                return application.Terminate;
            };
#endif
            return base.Load(context);
        }

        protected override void BindMessagePresenter(IModuleContext context, IIocContainer container)
        {
            container.Bind<IMessagePresenter, MessagePresenter>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindToastPresenter(IModuleContext context, IIocContainer container)
        {
            container.Bind<IToastPresenter, ToastPresenter>(DependencyLifecycle.SingleInstance);
        }

#if ANDROID
        protected override void BindTracer(IModuleContext context, IIocContainer container)
        {
            ITracer tracer = new TracerEx();
            ServiceProvider.Tracer = tracer;
            container.BindToConstant(tracer);
        }

        private static Activity GetActivity(Context context)
        {
            while (true)
            {
                var activity = context as Activity;
                if (activity == null)
                {
                    var wrapper = context as ContextWrapper;
                    if (wrapper == null)
                        return null;
                    context = wrapper.BaseContext;
                    continue;
                }
                return activity;
            }
        }
#endif

        private static bool IsLastPage(Page page)
        {
            return page.Navigation != null && page.Navigation.NavigationStack.Count == 1;
        }

        #endregion
    }
}
