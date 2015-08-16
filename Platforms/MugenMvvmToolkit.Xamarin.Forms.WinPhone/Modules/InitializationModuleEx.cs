#region Copyright

// ****************************************************************************
// <copyright file="InitializationModuleEx.cs">
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

using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Xamarin.Forms.Modules;

#if ANDROID && XAMARIN_FORMS
using MugenMvvmToolkit.Xamarin.Forms.Android.Infrastructure.Presenters;
namespace MugenMvvmToolkit.Xamarin.Forms.Android.Modules
#elif TOUCH
using MugenMvvmToolkit.Xamarin.Forms.iOS.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Xamarin.Forms.iOS.Modules
#elif WINDOWS_PHONE
using MugenMvvmToolkit.Xamarin.Forms.WinPhone.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Xamarin.Forms.WinPhone.Modules
#elif WINDOWSCOMMON
using MugenMvvmToolkit.Xamarin.Forms.WinRT.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Xamarin.Forms.WinRT.Modules
#endif
{
    /// <summary>
    ///     Represents the class that is used to initialize the IOC adapter.
    /// </summary>
    public class InitializationModuleEx : InitializationModule
    {
        #region Cosntructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="InitializationModuleEx" /> class.
        /// </summary>
        public InitializationModuleEx()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InitializationModuleEx" /> class.
        /// </summary>
        protected InitializationModuleEx(LoadMode mode = LoadMode.All, int priority = InitializationModulePriority)
            : base(mode, priority)
        {
        }

        #endregion

        #region Overrides of InitializationModule

        /// <summary>
        ///     Gets the <see cref="IMessagePresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IMessagePresenter" />.</returns>
        protected override BindingInfo<IMessagePresenter> GetMessagePresenter()
        {
            return BindingInfo<IMessagePresenter>.FromType<MessagePresenter>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IToastPresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IToastPresenter" />.</returns>
        protected override BindingInfo<IToastPresenter> GetToastPresenter()
        {
            return BindingInfo<IToastPresenter>.FromType<ToastPresenter>(DependencyLifecycle.SingleInstance);
        }

        #endregion
    }
}