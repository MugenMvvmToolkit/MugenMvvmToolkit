using System;
using Xamarin.Forms;

#if ANDROID
using Android.App;
using Android.Content;
namespace MugenMvvmToolkit.Xamarin.Forms.Android
{
    public static class AndroidInitializationExtensions
#elif WINDOWS_UWP
using Application = Windows.UI.Xaml.Application;
namespace MugenMvvmToolkit.Xamarin.Forms.UWP
{
    public static class UwpInitializationExtensions
#elif NETFX_CORE
using Application = Windows.UI.Xaml.Application;
namespace MugenMvvmToolkit.Xamarin.Forms.WinRT
{
    public static class WinRTInitializationExtensions
#elif WINDOWS_PHONE
using Application = System.Windows.Application;

namespace MugenMvvmToolkit.Xamarin.Forms.WinPhone
{
    public static class WinPhoneInitializationExtensions
#endif
    {
        public static Func<Page, Action> GetSendBackButtonPressedImpl()
        {
#if ANDROID            
            return page =>
            {
                if (!IsLastPage(page))
                    return null;
                var activity = GetActivity(global::Xamarin.Forms.Forms.Context);
                if (activity == null)
                    return null;
                return activity.OnBackPressed;
            };
#elif WINDOWS_UWP || NETFX_CORE
            return page =>
            {
                if (!IsLastPage(page))
                    return null;
                var application = Application.Current;
                if (application == null)
                    return null;
                return application.Exit;
            };
#elif WINDOWS_PHONE
            return page =>
            {
                if (!IsLastPage(page))
                    return null;
                var application = Application.Current;
                if (application == null)
                    return null;
                return application.Terminate;
            };
#endif            
        }

#if ANDROID
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
    }
}