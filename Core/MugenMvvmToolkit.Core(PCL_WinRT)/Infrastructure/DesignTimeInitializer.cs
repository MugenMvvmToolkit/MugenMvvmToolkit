#region Copyright

// ****************************************************************************
// <copyright file="DesignTimeInitializer.cs">
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
using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Exceptions;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the static class to work in design mode.
    /// </summary>
    public static class DesignTimeInitializer
    {
        #region Fields

        private static bool _loading;
        private static readonly string ToolkitAssemblyName;
        private static readonly object Locker;
        private static readonly PropertyInfo CurrentDomainProperty;
        private static readonly PropertyInfo IsDynamicProperty;
        private static readonly MethodInfo GetAssembliesMethod;
        private static readonly MethodInfo GetReferencedAssembliesMethod;
        private static IDesignTimeManager _designTimeManager;

        private static readonly HashSet<Assembly> LoadedAssemblies;
        private static readonly HashSet<Assembly> FilteredAssemblies;
        private static readonly List<IModule> LoadedModules;
        private static IModuleContext _lastContext;

        #endregion

        #region Constructors

        static DesignTimeInitializer()
        {
            ToolkitAssemblyName = typeof(DesignTimeInitializer).GetAssembly().FullName;
            Type appDomainType = Type.GetType("System.AppDomain", false);
            if (appDomainType == null)
                return;
            CurrentDomainProperty = appDomainType.GetPropertyEx("CurrentDomain");
            GetAssembliesMethod = appDomainType.GetMethodEx("GetAssemblies");
            IsDynamicProperty = typeof(Assembly).GetPropertyEx("IsDynamic");
            GetReferencedAssembliesMethod = typeof(Assembly).GetMethodEx("GetReferencedAssemblies");
            if (CurrentDomainProperty == null || GetAssembliesMethod == null || GetReferencedAssembliesMethod == null ||
                IsDynamicProperty == null)
                return;
            Locker = new object();
            LoadedAssemblies = new HashSet<Assembly>();
            FilteredAssemblies = new HashSet<Assembly>();
            LoadedModules = new List<IModule>();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets an instance of <see cref="IDesignTimeManager" />.
        /// </summary>
        [CanBeNull]
        public static IDesignTimeManager GetDesignTimeManager()
        {
            if (Locker == null)
                return null;
            if (_loading)
                return _designTimeManager;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(Locker, ref lockTaken);
                _loading = true;
                Initialize();
                return _designTimeManager;
            }
            catch (Exception exception)
            {
                throw new DesignTimeException(exception);
            }
            finally
            {
                _loading = false;
                if (lockTaken)
                    Monitor.Exit(Locker);
            }
        }

        private static void Initialize()
        {
            IList<Assembly> assemblies = GetAssemblies(false);
            if (assemblies.Count == 0)
                return;

            var managers = new List<ConstructorInfo>();
            for (int index = 0; index < assemblies.Count; index++)
                TryAddManagers(managers, assemblies[index]);
            bool isNew = false;
            IDesignTimeManager designTimeManager = null;
            if (_designTimeManager == null)
            {
                designTimeManager = GeMaxPriorityManager(managers);
                isNew = designTimeManager != null;
            }
            else
            {
                if (managers.Count != 1 || managers[0].DeclaringType != _designTimeManager.GetType())
                {
                    designTimeManager = GeMaxPriorityManager(managers);
                    isNew = designTimeManager != null && designTimeManager.GetType() != _designTimeManager.GetType();
                }
            }
            if (isNew)
            {
                if (_lastContext != null)
                {
                    for (int i = 0; i < LoadedModules.Count; i++)
                        LoadedModules[i].Unload(_lastContext);
                    LoadedModules.Clear();
                }
                if (_designTimeManager != null)
                    _designTimeManager.Dispose();

                _designTimeManager = designTimeManager;
                _designTimeManager.Initialize();
                InitializeDesignTimeModules(GetAssemblies(true));
            }
            else
                InitializeDesignTimeModules(assemblies);
        }

        private static IDesignTimeManager GeMaxPriorityManager(List<ConstructorInfo> managers)
        {
            IDesignTimeManager maxManager = null;
            for (int i = 0; i < managers.Count; i++)
            {
                var m = (IDesignTimeManager)managers[i].InvokeEx(Empty.Array<object>());
                if (maxManager == null || m.Priority > maxManager.Priority)
                {
                    if (maxManager != null)
                        maxManager.Dispose();
                    maxManager = m;
                }
            }
            return maxManager;
        }

        private static void InitializeDesignTimeModules(IList<Assembly> assemblies)
        {
            if (assemblies.Count == 0 || (_designTimeManager != null && !_designTimeManager.IsDesignMode))
                return;
            IList<IModule> modules = assemblies.GetModules(false);
            if (modules.Count == 0)
                return;

            _lastContext = _designTimeManager == null
                ? new ModuleContext(PlatformInfo.Unknown, LoadMode.Design, null, null, FilteredAssemblies.ToArrayFast())
                : new ModuleContext(_designTimeManager.Platform, LoadMode.Design, _designTimeManager.IocContainer,
                    _designTimeManager.Context, FilteredAssemblies.ToArrayFast());

            for (int i = 0; i < modules.Count; i++)
            {
                IModule module = modules[i];
                if (module.Load(_lastContext))
                    LoadedModules.Add(module);
            }
        }

        private static void TryAddManagers(List<ConstructorInfo> managers, Assembly assembly)
        {
            foreach (Type type in assembly.SafeGetTypes(false))
            {
                if (!typeof(IDesignTimeManager).IsAssignableFrom(type) || !type.IsPublicNonAbstractClass())
                    continue;
                ConstructorInfo constructor = type.GetConstructor(Empty.Array<Type>());
                if (constructor != null)
                    managers.Add(constructor);
            }
        }

        internal static IList<Assembly> GetAssemblies(bool ignoreLoaded)
        {
            var currentDomain = CurrentDomainProperty.GetValueEx<object>(null);
            var assemblies = (IEnumerable<Assembly>)GetAssembliesMethod.InvokeEx(currentDomain);
            if (assemblies == null)
                return Empty.Array<Assembly>();
            var result = new List<Assembly>();
            foreach (Assembly assembly in assemblies)
            {
                if ((ignoreLoaded || LoadedAssemblies.Add(assembly)) && FilterAssembly(assembly))
                {
                    FilteredAssemblies.Add(assembly);
                    result.Add(assembly);
                }
            }
            return result;
        }

        private static bool FilterAssembly(Assembly assembly)
        {
            if (!assembly.IsNonFrameworkAssembly())
                return false;
            if (IsDynamicProperty.GetValueEx<bool>(assembly))
                return false;
            var assemblyNames = GetReferencedAssembliesMethod.InvokeEx(assembly, null) as IEnumerable<AssemblyName>;
            return assemblyNames != null && assemblyNames.Any(name => name.FullName == ToolkitAssemblyName);
        }

        #endregion
    }
}