#region Copyright

// ****************************************************************************
// <copyright file="BootstrapperBase.cs">
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
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public abstract class BootstrapperBase
    {
        #region Fields

        private const int InitializedState = 1;
        private int _state;
        private int _initializationThreadId;
        private readonly ManualResetEvent _initializedEvent;

        #endregion

        #region Constructors

        protected BootstrapperBase(bool isDesignMode)
        {
            _initializedEvent = new ManualResetEvent(false);
            ServiceProvider.IsDesignMode = isDesignMode;
        }

        #endregion

        #region Properties

        public bool IsInitialized => _state == InitializedState;

        public static BootstrapperBase Current { get; protected set; }

        public IDataContext InitializationContext { get; set; }

        protected abstract PlatformInfo Platform { get; }

        protected IMvvmApplication MvvmApplication { get; set; }

        protected IIocContainer IocContainer { get; set; }

        protected bool IsDesignMode => ServiceProvider.IsDesignMode;

        #endregion

        #region Methods

        public void Initialize()
        {
#if NET4
            var managedThreadId = Thread.CurrentThread.ManagedThreadId;
#else
            var managedThreadId = Environment.CurrentManagedThreadId;
#endif
            if (Interlocked.Exchange(ref _state, InitializedState) == InitializedState)
            {
                if (_initializationThreadId != managedThreadId)
                    _initializedEvent.WaitOne();
                return;
            }
            _initializationThreadId = managedThreadId;
            Current = this;
            InitializeInternal();
            _initializedEvent.Set();
        }

        [NotNull]
        protected abstract IMvvmApplication CreateApplication();

        [NotNull]
        protected abstract IIocContainer CreateIocContainer();

        protected virtual void InitializeInternal()
        {
            MvvmApplication = CreateApplication();
            IocContainer = CreateIocContainer();
            if (IsDesignMode)
            {
                InitializationContext = InitializationContext.ToNonReadOnly();
                InitializationContext.AddOrUpdate(InitializationConstants.IsDesignMode, true);
            }
            MvvmApplication.Initialize(Platform, IocContainer, GetAssemblies(), InitializationContext ?? DataContext.Empty);
        }

        [NotNull]
        protected virtual IList<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly>
            {
                GetType().GetAssembly(),
                typeof(BootstrapperBase).GetAssembly()
            };
            TryLoadAssemblyByType("BindingServiceProvider", "MugenMvvmToolkit.Binding", assemblies);
            if (MvvmApplication != null)
                assemblies.Add(MvvmApplication.GetType().GetAssembly());
            UpdateAssemblies(assemblies);
            return assemblies.ToArrayEx();
        }

        protected virtual void UpdateAssemblies(HashSet<Assembly> assemblies)
        {
        }

        protected static void TryLoadAssemblyByType(string typeName, string namespaceAssemblyName, ICollection<Assembly> assemblies)
        {
            TryLoadAssemblyByType(namespaceAssemblyName + "." + typeName + ", " + namespaceAssemblyName, assemblies);
        }

        protected static void TryLoadAssemblyByType(string fullTypeName, ICollection<Assembly> assemblies)
        {
            try
            {
                //faster than Assembly.Load
                var type = Type.GetType(fullTypeName, false);
                if (type != null)
                    assemblies.Add(type.GetAssembly());
            }
            catch
            {
            }
        }

        #endregion
    }
}