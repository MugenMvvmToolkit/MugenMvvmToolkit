#region Copyright

// ****************************************************************************
// <copyright file="DesignTimeManagerBase.cs">
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
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Exceptions;

#if WPF
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
namespace MugenMvvmToolkit.WPF.Infrastructure
#elif WINFORMS
using System.Diagnostics;
namespace MugenMvvmToolkit.WinForms.Infrastructure
#elif SILVERLIGHT
using System.ComponentModel;

namespace MugenMvvmToolkit.Silverlight.Infrastructure
#elif WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Infrastructure
#elif WINDOWS_PHONE
using System.ComponentModel;

namespace MugenMvvmToolkit.WinPhone.Infrastructure
#endif
{
    public class DesignTimeManagerBase : IDesignTimeManager
    {
        #region Nested Types

        private sealed class DesignApp : MvvmApplication
        {
            #region Constructors

            public DesignApp()
                : base(LoadMode.Design)
            {
            }

            #endregion

            #region Methods

            public override Type GetStartViewModelType()
            {
                return typeof(IViewModel);
            }

            #endregion
        }

        #endregion

        #region Fields

        private static bool? _isDesignModeStatic;

        private readonly int _priority;
        private readonly object _locker;
        private bool _isInitialized;
        private IIocContainer _iocContainer;
        private readonly PlatformInfo _platform;
        private IDataContext _context;

        #endregion

        #region Constructors

        public DesignTimeManagerBase()
        {
            _locker = new object();
            _platform = PlatformExtensions.GetPlatformInfo();
            _priority = GetType() == typeof(DesignTimeManagerBase) ? int.MinValue : 0;
        }

        #endregion

        #region Implementation of IDesignTimeManager

        public virtual bool IsDesignMode
        {
            get
            {
                if (_isDesignModeStatic.HasValue)
                    return _isDesignModeStatic.Value;
                _isDesignModeStatic = GetIsDesignMode();
                return _isDesignModeStatic.Value;
            }
        }

        public virtual int Priority
        {
            get { return _priority; }
        }

        public PlatformInfo Platform
        {
            get { return _platform; }
        }

        public IIocContainer IocContainer
        {
            get { return _iocContainer; }
        }

        public IDataContext Context
        {
            get { return _context; }
        }

        public void Initialize()
        {
            if (_isInitialized || !IsDesignMode)
                return;
            bool hasException = false;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(_locker, ref lockTaken);
                if (_isInitialized)
                    return;
                _iocContainer = CreateIocContainer();
                _context = GetContext();
                if (_iocContainer != null)
                {
                    var application = CreateApplication();
                    application.Initialize(_platform, _iocContainer, ReflectionExtensions.GetDesignAssemblies(), _context);
                }
                OnInitialized();
            }
            catch (Exception exception)
            {
                hasException = true;
                throw new DesignTimeException(exception);
            }
            finally
            {
                if (!hasException)
                    _isInitialized = true;
                if (lockTaken)
                    Monitor.Exit(_locker);
            }
        }

        public void InitializeViewModel(IViewModel viewModel)
        {
            if (!IsDesignMode)
                return;
            Should.NotBeNull(viewModel, nameof(viewModel));
            SynchronizationContext context = SynchronizationContext.Current;
            if (context == null)
                Task.Factory.StartNew(() => InitializeViewModelInternal(viewModel));
            else
                context.Post(state => InitializeViewModelInternal(viewModel), null);
        }

        public virtual void Dispose()
        {
            if (IocContainer != null)
                IocContainer.Dispose();
        }

        #endregion

        #region Methods

        [CanBeNull]
        protected virtual IIocContainer CreateIocContainer()
        {
            foreach (var assembly in ReflectionExtensions.GetDesignAssemblies())
            {
                foreach (var type in assembly.SafeGetTypes(false))
                {
                    if (!typeof(IIocContainer).IsAssignableFrom(type) || !type.IsPublicNonAbstractClass())
                        continue;
                    var constructor = type.GetConstructor(Empty.Array<Type>());
                    if (constructor != null && constructor.IsPublic)
                        return (IIocContainer)constructor.InvokeEx();
                }

            }
            return null;
        }

        protected virtual IMvvmApplication CreateApplication()
        {
            return new DesignApp();
        }

        [CanBeNull]
        protected virtual IDataContext GetContext()
        {
            return DataContext.Empty;
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual void InitializeViewModelInternal([NotNull] IViewModel viewModel)
        {
            if (IocContainer == null || viewModel.IsInitialized)
                return;
            IViewModelProvider service;
            if (IocContainer.TryGet(out service))
                service.InitializeViewModel(viewModel, Context ?? DataContext.Empty);
        }

        private static bool GetIsDesignMode()
        {
            try
            {
#if WINFORMS
                return System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime || IsVsRunning();
#elif SILVERLIGHT || WINDOWS_PHONE
                return DesignerProperties.IsInDesignTool;
#elif WPF
                DependencyProperty prop = DesignerProperties.IsInDesignModeProperty;
                return (bool)DependencyPropertyDescriptor
                    .FromProperty(prop, typeof(FrameworkElement))
                    .Metadata
                    .DefaultValue || IsVsRunning();
#elif WINDOWSCOMMON
                return Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#endif
            }
            catch (Exception)
            {
                return false;
            }
        }

#if WPF || WINFORMS
        private static bool IsVsRunning()
        {
            using (var process = Process.GetCurrentProcess())
            {
                var processName = process.ProcessName;
                return processName.StartsWith("devenv", StringComparison.OrdinalIgnoreCase) ||
                       processName.StartsWith("xdesproc", StringComparison.OrdinalIgnoreCase);
            }
        }
#endif
        #endregion
    }
}
