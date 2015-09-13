#region Copyright

// ****************************************************************************
// <copyright file="BootstrapperBase.cs">
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

using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class BootstrapperBase
    {
        #region Fields

        private const int InitializedState = 1;
        private static int _state;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the current <see cref="BootstrapperBase" />.
        /// </summary>
        public static BootstrapperBase Current { get; protected set; }

        /// <summary>
        ///     Gets the initialized state of the current bootstrapper.
        /// </summary>
        public bool IsInitialized
        {
            get { return _state == InitializedState; }
        }

        /// <summary>
        ///     Gets or sets the initialization context.
        /// </summary>
        public IDataContext InitializationContext { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes the current bootstrapper.
        /// </summary>
        public void Initialize()
        {
            if (Interlocked.Exchange(ref _state, InitializedState) == InitializedState)
            {
                var current = Current;
                if (!ReferenceEquals(current, this))
                    Tracer.Error(ExceptionManager.ObjectInitialized(typeof(BootstrapperBase).Name, Current).Message);
                return;
            }
            Current = this;
            InitializeInternal();
        }

        /// <summary>
        ///     Initializes the current bootstrapper.
        /// </summary>
        protected abstract void InitializeInternal();

        /// <summary>
        ///     Creates an instance of <see cref="IMvvmApplication" />.
        /// </summary>
        [NotNull]
        protected abstract IMvvmApplication CreateApplication();

        /// <summary>
        ///     Creates an instance of <see cref="IIocContainer" />.
        /// </summary>
        /// <returns>An instance of <see cref="IIocContainer" />.</returns>
        [NotNull]
        protected abstract IIocContainer CreateIocContainer();

        /// <summary>
        ///     Tries to load assembly by full name.
        /// </summary>
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