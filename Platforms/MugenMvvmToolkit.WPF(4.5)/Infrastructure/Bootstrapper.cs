#region Copyright

// ****************************************************************************
// <copyright file="Bootstrapper.cs">
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
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
#if WPF
using System.Windows;
using Bootstrapper = MugenMvvmToolkit.WPF.Infrastructure.WpfBootstrapperBase;

namespace MugenMvvmToolkit.WPF.Infrastructure
#elif WINFORMS
using Bootstrapper = MugenMvvmToolkit.WinForms.Infrastructure.WinFormsBootstrapperBase;

namespace MugenMvvmToolkit.WinForms.Infrastructure
#elif WINDOWS_UWP
using Bootstrapper = MugenMvvmToolkit.UWP.Infrastructure.UwpBootstrapperBase;

namespace MugenMvvmToolkit.UWP.Infrastructure
#elif TOUCH
using UIKit;
using Bootstrapper = MugenMvvmToolkit.iOS.Infrastructure.TouchBootstrapperBase;

namespace MugenMvvmToolkit.iOS.Infrastructure
#elif XAMARIN_FORMS
using Bootstrapper = MugenMvvmToolkit.Xamarin.Forms.Infrastructure.XamarinFormsBootstrapperBase;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure
#endif
{
    public class Bootstrapper<T> : Bootstrapper
        where T : class
    {
        #region Nested types

        private sealed class DefaultApp : MvvmApplication
        {
            #region Methods

            public override Type GetStartViewModelType()
            {
                return typeof(T);
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly IIocContainer _iocContainer;
        private readonly IEnumerable<Assembly> _assemblies;
        private IMvvmApplication _application;

        #endregion

        #region Constructors

        static Bootstrapper()
        {
            if (!typeof(IViewModel).IsAssignableFrom(typeof(T)) && !typeof(IMvvmApplication).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException("The Bootstrapper<T> has invalid start type. The parameter T should be of type IViewModel or IMvvmApplication");
        }

#if WPF
        public Bootstrapper([NotNull] Application application, [NotNull] IIocContainer iocContainer, IEnumerable<Assembly> assemblies = null, PlatformInfo platform = null)
            : base(application, platform: platform)
#elif WINFORMS
        public Bootstrapper([NotNull] IIocContainer iocContainer, IEnumerable<Assembly> assemblies = null, PlatformInfo platform = null)
            : base(true, platform)
#elif WINDOWS_UWP
        public Bootstrapper([NotNull] IIocContainer iocContainer, IEnumerable<Assembly> assemblies = null, PlatformInfo platform = null)
            : base(platform)
#elif TOUCH
        public Bootstrapper([NotNull] UIWindow window, [NotNull] IIocContainer iocContainer, IEnumerable<Assembly> assemblies = null, PlatformInfo platform = null)
            : base(window, platform)
#elif XAMARIN_FORMS
        public Bootstrapper([NotNull]IPlatformService platformService, [NotNull] IIocContainer iocContainer, IEnumerable<Assembly> assemblies = null)
            : base(platformService)
#endif
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            _iocContainer = iocContainer;
            _assemblies = assemblies;
        }

        #endregion

        #region Overrides of BootstrapperBase

        protected override void UpdateAssemblies(HashSet<Assembly> assemblies)
        {
            base.UpdateAssemblies(assemblies);
            if (_assemblies != null)
                assemblies.AddRange(_assemblies);
            assemblies.Add(typeof(T).GetAssembly());
            if (_application != null)
                assemblies.Add(_application.GetType().GetAssembly());
        }

        protected override IMvvmApplication CreateApplication()
        {
            if (_application != null)
                return _application;
            if (typeof(IMvvmApplication).IsAssignableFrom(typeof(T)))
                return (IMvvmApplication)Activator.CreateInstance(typeof(T));
            return new DefaultApp();
        }

        protected override IIocContainer CreateIocContainer()
        {
            return _iocContainer;
        }

        #endregion

        #region Methods

        public void SetApplication<TApp>(TApp app)
            where TApp : class, T, IMvvmApplication
        {
            _application = app;
        }

        #endregion
    }
}
