#region Copyright

// ****************************************************************************
// <copyright file="BootstrapperBase.cs">
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

using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public abstract class BootstrapperBase
    {
        #region Fields

        private const int InitializedState = 1;
        private static int _state;
        private static readonly ManualResetEvent InitializedEvent;

        #endregion

        #region Constructors

        static BootstrapperBase()
        {
            InitializedEvent = new ManualResetEvent(false);
        }

        #endregion

        #region Properties

        public bool IsInitialized => _state == InitializedState;

        public static BootstrapperBase Current { get; protected set; }

        public IDataContext InitializationContext { get; set; }

        #endregion

        #region Methods

        public void Initialize()
        {
            if (Interlocked.Exchange(ref _state, InitializedState) == InitializedState)
            {
                InitializedEvent.WaitOne();
                var current = Current;
                if (!ReferenceEquals(current, this))
                    Tracer.Error(ExceptionManager.ObjectInitialized(typeof(BootstrapperBase).Name, Current).Message);
                return;
            }
            Current = this;
            InitializeInternal();
            InitializedEvent.Set();
        }

        protected abstract void InitializeInternal();

        [NotNull]
        protected abstract IMvvmApplication CreateApplication();

        [NotNull]
        protected abstract IIocContainer CreateIocContainer();

        protected static Assembly TryLoadAssembly(string assemblyName, ICollection<Assembly> assemblies)
        {
            try
            {
#if PCL_WINRT
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
#else
                var assembly = Assembly.Load(assemblyName);
#endif

                if (assembly != null && assemblies != null)
                    assemblies.Add(assembly);
                return assembly;
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                return null;
            }
        }

        #endregion
    }
}