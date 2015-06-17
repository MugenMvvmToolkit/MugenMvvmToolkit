#region Copyright

// ****************************************************************************
// <copyright file="Bootstrapper.cs">
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
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
#if WPF
using System.Windows;
using Bootstrapper = MugenMvvmToolkit.WPF.Infrastructure.WpfBootstrapperBase;

namespace MugenMvvmToolkit.WPF.Infrastructure
#elif WINFORMS
using Bootstrapper = MugenMvvmToolkit.WinForms.Infrastructure.WinFormsBootstrapperBase;

namespace MugenMvvmToolkit.WinForms.Infrastructure
#elif WINDOWS_PHONE
using Microsoft.Phone.Controls;
using Bootstrapper = MugenMvvmToolkit.WinPhone.Infrastructure.WindowsPhoneBootstrapperBase;

namespace MugenMvvmToolkit.WinPhone.Infrastructure
#elif SILVERLIGHT
using System.Windows;
using Bootstrapper = MugenMvvmToolkit.Silverlight.Infrastructure.SilverlightBootstrapperBase;

namespace MugenMvvmToolkit.Silverlight.Infrastructure
#elif NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml.Controls;
using Bootstrapper = MugenMvvmToolkit.WinRT.Infrastructure.WinRTBootstrapperBase;

namespace MugenMvvmToolkit.WinRT.Infrastructure
#elif TOUCH
using UIKit;
using Bootstrapper = MugenMvvmToolkit.iOS.Infrastructure.TouchBootstrapperBase;

namespace MugenMvvmToolkit.iOS.Infrastructure
#elif XAMARIN_FORMS
using Bootstrapper = MugenMvvmToolkit.Xamarin.Forms.Infrastructure.XamarinFormsBootstrapperBase;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure
#endif
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public class Bootstrapper<TRootViewModel> : Bootstrapper
        where TRootViewModel : IViewModel
    {
        #region Fields

        private readonly IIocContainer _iocContainer;
        private readonly IEnumerable<Assembly> _assemblies;
        private readonly IViewModelSettings _viewModelSettings;
        private readonly IModule[] _modules;

        #endregion

        #region Constructors

#if WPF || (SILVERLIGHT && !WINDOWS_PHONE)
        /// <summary>
        ///     Initializes a new instance of the <see cref="Bootstrapper{TRootViewModel}" /> class.
        /// </summary>
        public Bootstrapper([NotNull] Application application, [NotNull] IIocContainer iocContainer, IEnumerable<Assembly> assemblies = null,
            IViewModelSettings viewModelSettings = null, params IModule[] modules)
            : base(application)
#elif WINFORMS
        /// <summary>
        ///     Initializes a new instance of the <see cref="Bootstrapper{TRootViewModel}" /> class.
        /// </summary>
        public Bootstrapper([NotNull] IIocContainer iocContainer, IEnumerable<Assembly> assemblies = null,
            IViewModelSettings viewModelSettings = null, params IModule[] modules)
#elif WINDOWS_PHONE
        /// <summary>
        ///     Initializes a new instance of the <see cref="Bootstrapper{TRootViewModel}" /> class.
        /// </summary>
        public Bootstrapper([NotNull] PhoneApplicationFrame rootFrame, [NotNull] IIocContainer iocContainer, IEnumerable<Assembly> assemblies = null,
            IViewModelSettings viewModelSettings = null, params IModule[] modules)
            : base(rootFrame)
#elif NETFX_CORE || WINDOWSCOMMON
        /// <summary>
        ///     Initializes a new instance of the <see cref="Bootstrapper{TRootViewModel}" /> class.
        /// </summary>
        public Bootstrapper([NotNull] Frame rootFrame, [NotNull] IIocContainer iocContainer, IEnumerable<Assembly> assemblies = null,
            IViewModelSettings viewModelSettings = null, params IModule[] modules)
            : base(rootFrame, assemblies != null)
#elif TOUCH
        /// <summary>
        ///     Initializes a new instance of the <see cref="Bootstrapper{TRootViewModel}" /> class.
        /// </summary>
        public Bootstrapper([NotNull] UIWindow window, [NotNull] IIocContainer iocContainer, IEnumerable<Assembly> assemblies = null,
            IViewModelSettings viewModelSettings = null, params IModule[] modules)
            : base(window)
#elif XAMARIN_FORMS
        /// <summary>
        ///     Initializes a new instance of the <see cref="Bootstrapper{TRootViewModel}" /> class.
        /// </summary>
        public Bootstrapper([NotNull] IIocContainer iocContainer, IEnumerable<Assembly> assemblies = null,
            IViewModelSettings viewModelSettings = null, params IModule[] modules)            
#endif
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            _iocContainer = iocContainer;
            _assemblies = assemblies;
            _viewModelSettings = viewModelSettings;
            _modules = modules.IsNullOrEmpty() ? null : modules;
        }

        #endregion

        #region Overrides of BootstrapperBase

        /// <summary>
        ///     Creates an instance of <see cref="IViewModelSettings" />.
        /// </summary>
        /// <returns>An instance of <see cref="IViewModelSettings" />.</returns>
        protected override IViewModelSettings CreateViewModelSettings()
        {
            if (_viewModelSettings == null)
                return base.CreateViewModelSettings();
            return _viewModelSettings;
        }

        /// <summary>
        ///     Gets the application modules.
        /// </summary>
        protected override IList<IModule> GetModules()
        {
            if (_modules == null)
                return base.GetModules();
            return _modules;
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected override ICollection<Assembly> GetAssemblies()
        {
            if (_assemblies == null)
                return base.GetAssemblies();
            var assemblies = new HashSet<Assembly>(_assemblies);
#if WINDOWSCOMMON || NETFX_CORE || XAMARIN_FORMS
            assemblies.Add(GetType().GetTypeInfo().Assembly);
            assemblies.Add(typeof(Bootstrapper).GetTypeInfo().Assembly);
            assemblies.Add(typeof(ApplicationSettings).GetTypeInfo().Assembly);
#else
            assemblies.Add(GetType().Assembly);
            assemblies.Add(typeof(Bootstrapper).Assembly);
            assemblies.Add(typeof(ApplicationSettings).Assembly);
#endif
#if !WINFORMS && !TOUCH
            TryLoadAssembly(BindingAssemblyName, assemblies);
#endif
            return assemblies;
        }

        /// <summary>
        ///     Gets the type of main view model.
        /// </summary>
        protected override sealed Type GetMainViewModelType()
        {
            return typeof(TRootViewModel);
        }

        /// <summary>
        ///     Creates an instance of <see cref="IIocContainer" />.
        /// </summary>
        /// <returns>An instance of <see cref="IIocContainer" />.</returns>
        protected override IIocContainer CreateIocContainer()
        {
            return _iocContainer;
        }

        #endregion
    }
}