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
    internal static class DesignTimeInitializer
    {
        #region Fields

        private static readonly string ToolkitAssemblyName;
        private static readonly object Locker;

        private static readonly Func<object, object> GetCurrentDomain;
        private static readonly Func<object, bool> GetIsDynamic;
        private static readonly Func<object, IEnumerable<Assembly>> GetAssembliesDelegate;
        private static readonly Func<Assembly, IEnumerable<AssemblyName>> GetReferencedAssemblies;

        private static readonly Func<object, string> GetLocation;
        private static readonly Func<object[], object> CreateFileInfo;
        private static readonly Func<object, DateTime> GetLastWriteTimeDelegate;

        private static readonly HashSet<Assembly> LoadedAssemblies;
        private static readonly Dictionary<string, IModule> LoadedModules;
        private static IModuleContext _lastContext;
        private static IDesignTimeManager _designTimeManager;

        private static bool _isWinRT;
        private static bool _loading;

        #endregion

        #region Constructors

        static DesignTimeInitializer()
        {
            ToolkitAssemblyName = typeof(DesignTimeInitializer).GetAssembly().FullName;
            Type appDomainType = Type.GetType("System.AppDomain", false);
            var fileInfoType = Type.GetType("System.IO.FileInfo", false);
            if (appDomainType == null || fileInfoType == null)
                return;
            var currentDomainProperty = appDomainType.GetPropertyEx("CurrentDomain");
            var getAssembliesMethod = appDomainType.GetMethodEx("GetAssemblies");
            var isDynamicProperty = typeof(Assembly).GetPropertyEx("IsDynamic");
            var getReferencedAssembliesMethod = typeof(Assembly).GetMethodEx("GetReferencedAssemblies");
            var locationProperty = typeof(Assembly).GetPropertyEx("Location");
            var lastWriteTimeProperty = fileInfoType.GetPropertyEx("LastWriteTime");
            var fileInfoConstructor = fileInfoType.GetConstructor(new[] { typeof(string) });
            if (currentDomainProperty == null || locationProperty == null || lastWriteTimeProperty == null ||
                getAssembliesMethod == null || getReferencedAssembliesMethod == null || fileInfoConstructor == null || isDynamicProperty == null)
                return;
            var reflectionManager = ServiceProvider.ReflectionManager;
            GetCurrentDomain = reflectionManager.GetMemberGetter<object>(currentDomainProperty);
            GetIsDynamic = reflectionManager.GetMemberGetter<bool>(isDynamicProperty);
            GetLocation = reflectionManager.GetMemberGetter<string>(locationProperty);
            GetLastWriteTimeDelegate = reflectionManager.GetMemberGetter<DateTime>(lastWriteTimeProperty);
            GetAssembliesDelegate = (Func<object, IEnumerable<Assembly>>)reflectionManager
                .GetMethodDelegate(typeof(Func<object, IEnumerable<Assembly>>), getAssembliesMethod);
            GetReferencedAssemblies = (Func<Assembly, IEnumerable<AssemblyName>>)reflectionManager
                .GetMethodDelegate(typeof(Func<Assembly, IEnumerable<AssemblyName>>), getReferencedAssembliesMethod);
            CreateFileInfo = reflectionManager.GetActivatorDelegate(fileInfoConstructor);
            Locker = new object();
            LoadedAssemblies = new HashSet<Assembly>();
            LoadedModules = new Dictionary<string, IModule>(StringComparer.Ordinal);
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

            //Trying to load all assemblies, WinRT designer has problems with loading assemblies with same name.
            if (_designTimeManager == null && managers.Count == 0)
            {
                assemblies.Clear();
                foreach (var assembly in LoadedAssemblies)
                {
                    if (CanLoadAssembly(true, assembly))
                        assemblies.Add(assembly);
                }
                for (int index = 0; index < assemblies.Count; index++)
                    TryAddManagers(managers, assemblies[index]);
                _isWinRT = managers.Count != 0;
            }

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
                    foreach (var module in LoadedModules.Values)
                    {
                        module.Unload(_lastContext);
                        module.TraceModule(false);
                    }
                    LoadedModules.Clear();
                }
                if (_designTimeManager != null)
                {
                    _designTimeManager.Dispose();
                    Tracer.Info("The design time manager was unloaded " + _designTimeManager);
                }

                _designTimeManager = designTimeManager;
                _designTimeManager.Initialize();
                Tracer.Info("The design time manager was loaded " + _designTimeManager);
                InitializeDesignTimeModules(GetAssemblies(true), true);
            }
            else
                InitializeDesignTimeModules(assemblies, false);
        }

        private static void InitializeDesignTimeModules(IList<Assembly> assemblies, bool allAssemblies)
        {
            var designTimeManager = _designTimeManager;
            if (assemblies.Count == 0 || designTimeManager == null || !designTimeManager.IsDesignMode)
                return;
            IList<IModule> modules = assemblies.GetModules(false);
            if (modules.Count == 0)
                return;

            var context = new ModuleContext(designTimeManager.Platform, LoadMode.Design, designTimeManager.IocContainer,
                designTimeManager.Context, allAssemblies ? assemblies : GetAssemblies(true));
            for (int i = 0; i < modules.Count; i++)
            {
                IModule module = modules[i];
                var fullName = module.GetType().AssemblyQualifiedName;
                IModule oldModule;
                if (LoadedModules.TryGetValue(fullName, out oldModule))
                {
                    LoadedModules.Remove(fullName);
                    oldModule.Unload(_lastContext);
                    oldModule.TraceModule(false);
                }
                if (module.Load(context))
                {
                    LoadedModules[fullName] = module;
                    module.TraceModule(true);
                }
            }
            _lastContext = context;
        }

        internal static IList<Assembly> GetAssemblies(bool ignoreLoaded)
        {
            var currentDomain = GetCurrentDomain(null);
            var assemblies = GetAssembliesDelegate(currentDomain);
            if (assemblies == null)
                return Empty.Array<Assembly>();

            Dictionary<string, List<Assembly>> dictionary = null;
            foreach (Assembly assembly in assemblies)
            {
                if (!CanLoadAssembly(ignoreLoaded, assembly))
                    continue;
                if (dictionary == null)
                    dictionary = new Dictionary<string, List<Assembly>>(StringComparer.Ordinal);
                List<Assembly> list;
                if (!dictionary.TryGetValue(assembly.FullName, out list))
                {
                    list = new List<Assembly>();
                    dictionary[assembly.FullName] = list;
                }
                list.Add(assembly);
            }
            if (dictionary == null)
                return Empty.Array<Assembly>();

            var result = new List<Assembly> { typeof(DesignTimeInitializer).GetAssembly() };
            foreach (var list in dictionary.Values)
            {
                //WinRT designer has problems with loading assemblies, and we should load all.
                if (_isWinRT)
                {
                    result.AddRange(list);
                    continue;
                }
                if (list.Count == 1)
                    result.Add(list[0]);
                else
                {
                    Assembly assembly = list[0];
                    DateTime value = GetLastWriteTime(assembly);
                    for (int i = 1; i < list.Count; i++)
                    {
                        var assm = list[i];
                        var time = GetLastWriteTime(assm);
                        if (time > value)
                        {
                            assembly = assm;
                            value = time;
                        }
                    }
                    result.Add(assembly);
                }
            }
            return result;
        }

        private static bool CanLoadAssembly(bool ignoreLoaded, Assembly assembly)
        {
            return (ignoreLoaded || LoadedAssemblies.Add(assembly)) && FilterAssembly(assembly);
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

        private static bool FilterAssembly(Assembly assembly)
        {
            if (!assembly.IsToolkitAssembly())
                return false;
            if (GetIsDynamic(assembly))
                return false;
            var assemblyNames = GetReferencedAssemblies(assembly);
            return assemblyNames != null && assemblyNames.Any(name => name.FullName == ToolkitAssemblyName);
        }

        private static DateTime GetLastWriteTime(Assembly assembly)
        {
            var value = GetLocation(assembly);
            if (string.IsNullOrEmpty(value))
                return DateTime.MinValue;
            var fileInfo = CreateFileInfo(new object[] { value });
            return GetLastWriteTimeDelegate(fileInfo);
        }

        #endregion
    }
}