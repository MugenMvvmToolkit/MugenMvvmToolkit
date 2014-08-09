#region Copyright
// ****************************************************************************
// <copyright file="DesignTimeManagerBase.cs">
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
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
    public class DesignTimeManagerBase : IDesignTimeManager
    {
        #region Fields

        private readonly int _priority;
        private readonly object _locker;
        private bool _isInitialized;
        private IIocContainer _iocContainer;
        private readonly bool _isDesignMode;
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
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            _isDesignMode = GetIsDesignMode();
            _platform = PlatformExtensions.GetPlatformInfo();
            _priority = GetType() == typeof(DesignTimeManagerBase) ? int.MinValue : 0;
        }

        #endregion

        #region Implementation of IDesignTimeManager

        /// <summary>
        ///     Gets the value indicating whether the control is in design mode (running under Blend or Visual Studio).
        /// </summary>
        public bool IsDesignMode
        {
            get { return _isDesignMode; }
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
            if (_isInitialized)
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
                if (IocContainer != null)
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
            Should.NotBeNull(viewModel, "viewModel");
            InitializeViewModelInternal(viewModel);
            var designViewModel = viewModel as IDesignViewModel;
            if (designViewModel != null)
                designViewModel.InitializeViewModel();
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
        ///     Gets the value indicating whether the control is in design mode (running under Blend or Visual Studio).
        /// </summary>
        protected virtual bool GetIsDesignMode()
        {
            return GetIsDesignModeStatic();
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
            if (IocContainer == null)
                return;
            IocContainer
                .Get<IViewModelProvider>()
                .InitializeViewModel(viewModel, Context ?? DataContext.Empty);
        }

        internal static bool GetIsDesignModeStatic()
        {
            try
            {
#if WINFORMS
                return System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;
#elif SILVERLIGHT
                return DesignerProperties.IsInDesignTool;
#elif WPF
                DependencyProperty prop = DesignerProperties.IsInDesignModeProperty;
                var isInDesignMode = (bool)DependencyPropertyDescriptor
                    .FromProperty(prop, typeof(FrameworkElement))
                    .Metadata
                    .DefaultValue;
                if (!isInDesignMode &&
                    Process.GetCurrentProcess().ProcessName.StartsWith("devenv", StringComparison.Ordinal))
                    isInDesignMode = true;
                return isInDesignMode;
#elif NETFX_CORE || WINDOWSCOMMON
                return Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#endif
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}