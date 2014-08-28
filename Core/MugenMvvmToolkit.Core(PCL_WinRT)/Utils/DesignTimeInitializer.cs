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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Exceptions;

namespace MugenMvvmToolkit.Utils
{
    internal static class DesignTimeInitializer
    {
        #region Nested types

        private sealed class ModuleTypeComparer : IEqualityComparer<IModule>
        {
            #region Fields

            public static readonly ModuleTypeComparer Instance = new ModuleTypeComparer();

            #endregion

            #region Implementation of IEqualityComparer<in IModule>

            public bool Equals(IModule x, IModule y)
            {
                return x.GetType().Equals(y.GetType());
            }

            public int GetHashCode(IModule obj)
            {
                return obj.GetType().GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Fields

        private static bool _loading;
        private static readonly string ToolkitAssemblyName;
        private static readonly object Locker;
        private static readonly PropertyInfo CurrentDomainProperty;
        private static readonly PropertyInfo IsDynamicProperty;
        private static readonly MethodInfo GetAssembliesMethod;
        private static readonly MethodInfo GetReferencedAssembliesMethod;
        private static IDesignTimeManager _designTimeManager;

        private static readonly Dictionary<Assembly, HashSet<IModule>> LoadedModules;

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
            LoadedModules = new Dictionary<Assembly, HashSet<IModule>>();
        }

        #endregion

        #region Methods

        public static IDesignTimeManager GetDesignTimeManager()
        {
            if (Locker == null)
                return null;
            try
            {
                lock (Locker)
                {
                    if (_loading)
                        return _designTimeManager;
                    _loading = true;
                    IList<Assembly> assemblies = null;
                    if (_designTimeManager == null || _designTimeManager.IsDesignMode)
                    {
                        assemblies = GetAssemblies(true);
                        InitializeDesignTimeManager(assemblies);
                    }
                    if (_designTimeManager == null || _designTimeManager.IsDesignMode)
                    {
                        if (assemblies == null)
                            assemblies = GetAssemblies(true);
                        InitializeDesignTimeModules(assemblies);
                    }
                    return _designTimeManager;
                }
            }
            catch (Exception exception)
            {
                throw new DesignTimeException(exception);
            }
            finally
            {
                _loading = false;
            }
        }

        private static IList<Assembly> GetAssemblies(bool onlyToolkitReferenced)
        {
            var currentDomain = CurrentDomainProperty.GetValueEx<object>(null);
            var assemblies = (IEnumerable<Assembly>)GetAssembliesMethod.InvokeEx(currentDomain);
            if (assemblies == null)
                return EmptyValue<Assembly>.ListInstance;
            var result = new List<Assembly>();
            foreach (var assembly in assemblies)
            {
                if (FilterAssembly(assembly, onlyToolkitReferenced))
                    result.Add(assembly);
            }
            return result;
        }

        private static void InitializeDesignTimeManager(IList<Assembly> assemblies)
        {
            var managers = new List<IDesignTimeManager>();
            for (int index = 0; index < assemblies.Count; index++)
                TryAddManagers(managers, assemblies[index]);

            var oldManager = _designTimeManager;
            var manager = managers.OrderByDescending(timeManager => timeManager.Priority).FirstOrDefault();
            if (_designTimeManager == null)
                _designTimeManager = manager;
            else
            {
                if (manager == null || !_designTimeManager.GetType().Equals(manager.GetType()))
                    _designTimeManager = manager;
            }
            if (_designTimeManager != null && !ReferenceEquals(_designTimeManager, oldManager))
                _designTimeManager.Initialize();
        }

        private static void InitializeDesignTimeModules(IList<Assembly> assemblies)
        {
            var modules = MvvmUtils.GetModules(assemblies, false);
            var context = _designTimeManager == null
                ? new ModuleContext(PlatformInfo.Unknown, LoadMode.Design, null, null, assemblies)
                : new ModuleContext(_designTimeManager.Platform, LoadMode.Design, _designTimeManager.IocContainer,
                    _designTimeManager.Context, assemblies);

            for (int i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                var assembly = module.GetType().GetAssembly();
                HashSet<IModule> list;
                if (!LoadedModules.TryGetValue(assembly, out list))
                {
                    list = new HashSet<IModule>(ModuleTypeComparer.Instance);
                    LoadedModules.Add(assembly, list);
                }
                if (list.Add(module))
                    module.Load(context);
            }
        }

        private static void TryAddManagers(List<IDesignTimeManager> managers, Assembly assembly)
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
                    managers.Add((IDesignTimeManager)constructor.InvokeEx(EmptyValue<object>.ArrayInstance));
            }
        }

        private static bool FilterAssembly(Assembly assembly, bool onlyToolkitReferenced)
        {
            if (!MvvmUtils.NonFrameworkAssemblyFilter(assembly))
                return false;
            if (IsDynamicProperty.GetValueEx<bool>(assembly))
                return false;
            if (!onlyToolkitReferenced)
                return true;
            var assemblyNames = GetReferencedAssembliesMethod.InvokeEx(assembly, null) as IEnumerable<AssemblyName>;
            return assemblyNames != null && assemblyNames.Any(name => name.FullName == ToolkitAssemblyName);
        }

        #endregion
    }
}
