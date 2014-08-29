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
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Exceptions;

namespace MugenMvvmToolkit.Utils
{
    /// <summary>
    ///     Represents the static class to work in design mode.
    /// </summary>
    internal static class DesignTimeInitializer
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
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an instance of <see cref="IDesignTimeManager"/>.
        /// </summary>
        [CanBeNull]
        public static IDesignTimeManager GetDesignTimeManager()
        {
            if (Locker == null)
                return null;
            if (_loading)
                return _designTimeManager;
            bool cycle = false;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(Locker, ref lockTaken);
                if (_loading)
                {
                    cycle = true;
                    return _designTimeManager;
                }
                _loading = true;
                if (_designTimeManager == null || _designTimeManager.IsDesignMode)
                    InitializeDesignTimeManager();
                return _designTimeManager;
            }
            catch (Exception exception)
            {
                throw new DesignTimeException(exception);
            }
            finally
            {
                if (!cycle)
                    _loading = false;
                if (lockTaken)
                    Monitor.Exit(Locker);
            }
        }

        private static void InitializeDesignTimeManager()
        {
            var assemblies = GetAssemblies(false);
            if (assemblies.Count == 0)
                return;

            var managers = new List<ConstructorInfo>();
            for (int index = 0; index < assemblies.Count; index++)
                TryAddManagers(managers, assemblies[index]);
            bool isNew = false;
            if (_designTimeManager == null)
            {
                _designTimeManager = GeMaxPriorityManager(managers);
                isNew = _designTimeManager != null;
            }
            else
            {
                if (managers.Count != 1 || !managers[0].DeclaringType.Equals(_designTimeManager.GetType()))
                {
                    var manager = GeMaxPriorityManager(managers);
                    if (manager == null || !_designTimeManager.GetType().Equals(manager.GetType()))
                    {
                        _designTimeManager = manager;
                        isNew = true;
                    }
                }
            }
            if (isNew)
            {
                if (_designTimeManager != null)
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
                var m = (IDesignTimeManager)managers[i].InvokeEx(EmptyValue<object>.ArrayInstance);
                if (maxManager == null || m.Priority > maxManager.Priority)
                    maxManager = m;
            }
            return maxManager;
        }

        private static void InitializeDesignTimeModules(IList<Assembly> assemblies)
        {
            var modules = MvvmUtils.GetModules(assemblies, false);
            if (modules.Count == 0)
                return;

            var context = _designTimeManager == null
                ? new ModuleContext(PlatformInfo.Unknown, LoadMode.Design, null, null, assemblies)
                : new ModuleContext(_designTimeManager.Platform, LoadMode.Design, _designTimeManager.IocContainer,
                    _designTimeManager.Context, assemblies);

            for (int i = 0; i < modules.Count; i++)
                modules[i].Load(context);
        }

        private static void TryAddManagers(List<ConstructorInfo> managers, Assembly assembly)
        {
            foreach (var type in assembly.SafeGetTypes(false))
            {
#if PCL_WINRT
                var typeInfo = type.GetTypeInfo();
                if (!typeof(IDesignTimeManager).IsAssignableFrom(type) || typeInfo.IsAbstract || !typeInfo.IsClass)
                    continue;
#else
                if (!typeof(IDesignTimeManager).IsAssignableFrom(type) || type.IsAbstract || !type.IsClass)
                    continue;
#endif
                var constructor = type.GetConstructor(EmptyValue<Type>.ArrayInstance);
                if (constructor != null)
                    managers.Add(constructor);
            }
        }

        private static IList<Assembly> GetAssemblies(bool ignoreLoaded)
        {
            var currentDomain = CurrentDomainProperty.GetValueEx<object>(null);
            var assemblies = (IEnumerable<Assembly>)GetAssembliesMethod.InvokeEx(currentDomain);
            if (assemblies == null)
                return EmptyValue<Assembly>.ListInstance;
            var result = new List<Assembly>();
            foreach (var assembly in assemblies)
            {
                if ((ignoreLoaded || LoadedAssemblies.Add(assembly)) && FilterAssembly(assembly))
                    result.Add(assembly);
            }
            return result;
        }

        private static bool FilterAssembly(Assembly assembly)
        {
            if (!MvvmUtils.NonFrameworkAssemblyFilter(assembly))
                return false;
            if (IsDynamicProperty.GetValueEx<bool>(assembly))
                return false;
            var assemblyNames = GetReferencedAssembliesMethod.InvokeEx(assembly, null) as IEnumerable<AssemblyName>;
            return assemblyNames != null && assemblyNames.Any(name => name.FullName == ToolkitAssemblyName);
        }

        #endregion
    }
}
