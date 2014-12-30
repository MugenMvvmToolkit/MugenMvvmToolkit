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
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Exceptions;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the base class for the design time manager.
    /// </summary>
    public class DesignTimeManagerBase : DisposableObject, IDesignTimeManager
    {
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="DesignTimeManagerBase" /> class.
        /// </summary>
        public DesignTimeManagerBase()
        {
            _locker = new object();
            _platform = PlatformExtensions.GetPlatformInfo();
            _priority = GetType() == typeof(DesignTimeManagerBase) ? int.MinValue : 0;
        }

        #endregion

        #region Implementation of IDesignTimeManager

        /// <summary>
        ///     Gets the value indicating whether the control is in design mode (running under Blend or Visual Studio).
        /// </summary>
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

        /// <summary>
        ///     Gets the load-priority.
        /// </summary>
        public virtual int Priority
        {
            get { return _priority; }
        }

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        public PlatformInfo Platform
        {
            get { return _platform; }
        }

        /// <summary>
        ///     Gets the design time <see cref="IDesignTimeManager.IocContainer" />, if any.
        /// </summary>
        public IIocContainer IocContainer
        {
            get { return _iocContainer; }
        }

        /// <summary>
        ///     Gets the design context.
        /// </summary>
        public IDataContext Context
        {
            get { return _context; }
        }

        /// <summary>
        ///     Initializes the current design time manager.
        /// </summary>
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
                if (IocContainer == null)
                    ApplicationSettings.Platform = _platform;
                else
                    ServiceProvider.Initialize(IocContainer, _platform);
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

        /// <summary>
        ///     Initializes the view model in design mode.
        /// </summary>
        public void InitializeViewModel(IViewModel viewModel)
        {
            if (!IsDesignMode)
                return;
            Should.NotBeNull(viewModel, "viewModel");
            SynchronizationContext context = SynchronizationContext.Current;
            if (context == null)
                Task.Factory.StartNew(() => InitializeViewModelInternal(viewModel));
            else
                context.Post(state => InitializeViewModelInternal(viewModel), null);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates an instance of <see cref="IIocContainer" />.
        /// </summary>
        /// <returns>An instance of <see cref="IIocContainer" />.</returns>
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

        /// <summary>
        ///     Gets the design context.
        /// </summary>
        [CanBeNull]
        protected virtual IDataContext GetContext()
        {
            return DataContext.Empty;
        }

        /// <summary>
        ///     Occurs after the manager is fully loaded.
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        /// <summary>
        ///     Initializes the view model in design mode.
        /// </summary>
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
#elif SILVERLIGHT
                return DesignerProperties.IsInDesignTool;
#elif WPF
                DependencyProperty prop = DesignerProperties.IsInDesignModeProperty;
                return (bool)DependencyPropertyDescriptor
                    .FromProperty(prop, typeof(FrameworkElement))
                    .Metadata
                    .DefaultValue || IsVsRunning();
#elif NETFX_CORE || WINDOWSCOMMON
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

        #region Overrides of DisposableObject

        /// <summary>
        ///     Releases resources held by the object.
        /// </summary>
        protected override void OnDispose(bool disposing)
        {
            if (disposing && IocContainer != null)
                IocContainer.Dispose();
            base.OnDispose(disposing);
        }

        #endregion
    }
}