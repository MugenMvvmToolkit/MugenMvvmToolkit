#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsInitializationModuleEx.cs">
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

using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Xamarin.Forms.Modules;
#if ANDROID && XAMARIN_FORMS
using MugenMvvmToolkit.Xamarin.Forms.Android.Binding;
using MugenMvvmToolkit.Xamarin.Forms.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.Android.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.Android.Infrastructure.Presenters;
namespace MugenMvvmToolkit.Xamarin.Forms.Android.Modules
#elif TOUCH
using MugenMvvmToolkit.Xamarin.Forms.iOS.Binding;
using MugenMvvmToolkit.Xamarin.Forms.iOS.Binding.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.iOS.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Xamarin.Forms.iOS.Modules
#elif WINDOWS_PHONE
using MugenMvvmToolkit.Xamarin.Forms.WinPhone.Binding;
using MugenMvvmToolkit.Xamarin.Forms.WinPhone.Binding.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.WinPhone.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Xamarin.Forms.WinPhone.Modules
#elif WINDOWS_UWP
using MugenMvvmToolkit.Xamarin.Forms.UWP.Binding;
using MugenMvvmToolkit.Xamarin.Forms.UWP.Binding.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.UWP.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Xamarin.Forms.UWP.Modules
#elif NETFX_CORE
using MugenMvvmToolkit.Xamarin.Forms.WinRT.Binding;
using MugenMvvmToolkit.Xamarin.Forms.WinRT.Binding.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.WinRT.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Xamarin.Forms.WinRT.Modules
#endif
{
    public class XamarinFormsInitializationModuleEx : XamarinFormsInitializationModule
    {
        #region Methods

        public override bool Load(IModuleContext context)
        {

            BindingServiceProvider.ValueConverter = BindingConverterExtensions.Convert;
#if ANDROID
            BindingServiceProvider.ErrorProvider = new XamarinFormsAndroidBindingErrorProvider();
            XamarinFormsExtensions.SendBackButtonPressed = AndroidInitializationExtensions.GetSendBackButtonPressedImpl();
#elif WINDOWS_UWP
            BindingServiceProvider.ErrorProvider = new XamarinFormsUwpBindingErrorProvider();
            XamarinFormsExtensions.SendBackButtonPressed = UwpInitializationExtensions.GetSendBackButtonPressedImpl();
#elif WINDOWS_PHONE
            BindingServiceProvider.ErrorProvider = new XamarinFormsWinPhoneBindingErrorProvider();
            XamarinFormsExtensions.SendBackButtonPressed = WinPhoneInitializationExtensions.GetSendBackButtonPressedImpl();
#elif NETFX_CORE
            BindingServiceProvider.ErrorProvider = new XamarinFormsWinRTBindingErrorProvider();
            XamarinFormsExtensions.SendBackButtonPressed = WinRTInitializationExtensions.GetSendBackButtonPressedImpl();
#elif TOUCH
            BindingServiceProvider.ErrorProvider = new XamarinFormsTouchBindingErrorProvider();
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
#endif

        #endregion
    }
}
