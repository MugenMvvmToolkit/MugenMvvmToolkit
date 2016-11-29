#region Copyright

// ****************************************************************************
// <copyright file="TypeCache.cs">
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

using System;
using System.Collections.Generic;
using System.Reflection;
using Android.Runtime;
using Java.Lang;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Attributes;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Android.Infrastructure
{
    public sealed class TypeCache<TType>
    {
        #region Fields

        public static readonly TypeCache<TType> Instance;
        // ReSharper disable once StaticMemberInGenericType
        private static HashSet<string> _usedTypes;

        private readonly HashSet<Assembly> _cachedAssemblies;
        private readonly object _locker;

        private readonly IDictionary<string, Type> _fullNameCache;
        private readonly IDictionary<string, Type> _ignoreCaseFullNameCache;

        private readonly IDictionary<string, Type> _ignoreCaseNameCache;
        private readonly IDictionary<string, Type> _nameCache;

        #endregion

        #region Constructors

        static TypeCache()
        {
            Instance = new TypeCache<TType>();
            if (AndroidBootstrapperBase.ViewAssemblies != null)
                Initialize(AndroidBootstrapperBase.ViewAssemblies);
        }

        private TypeCache()
        {
            _locker = new object();
            _ignoreCaseFullNameCache = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            _ignoreCaseNameCache = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            _fullNameCache = new Dictionary<string, Type>(StringComparer.InvariantCulture);
            _nameCache = new Dictionary<string, Type>(StringComparer.InvariantCulture);
            _cachedAssemblies = new HashSet<Assembly>();
        }

        #endregion

        #region Methods

        public static void Initialize([CanBeNull] IList<Assembly> assemblies)
        {
            if (assemblies == null)
                return;
            for (int index = 0; index < assemblies.Count; index++)
                Instance.AddAssembly(assemblies[index]);
            if (!PlatformExtensions.TypeCacheOnlyUsedTypeToBootstrapCodeBuilder)
            {
                var codeBuilder = ServiceProvider.BootstrapCodeBuilder;
                if (codeBuilder != null)
                {
                    foreach (var type in Instance._fullNameCache)
                        WriteTypeUsage(codeBuilder, type.Key, type.Value, true);
                    foreach (var type in Instance._nameCache)
                        WriteTypeUsage(codeBuilder, type.Key, type.Value, false);
                }
            }
        }

        public Type GetTypeByName(string typeName, bool ignoreCase, bool throwOnError)
        {
            if (_cachedAssemblies.Count == 0)
                Initialize(AndroidBootstrapperBase.ViewAssemblies);
            Type type;
            lock (_locker)
            {
                if (ignoreCase)
                {
                    if (!_ignoreCaseFullNameCache.TryGetValue(typeName, out type))
                    {
                        if (_ignoreCaseNameCache.TryGetValue(typeName, out type) && PlatformExtensions.TypeCacheOnlyUsedTypeToBootstrapCodeBuilder)
                            WriteTypeUsage(ServiceProvider.BootstrapCodeBuilder, typeName, type, false);
                    }
                    else if (PlatformExtensions.TypeCacheOnlyUsedTypeToBootstrapCodeBuilder)
                        WriteTypeUsage(ServiceProvider.BootstrapCodeBuilder, typeName, type, true);
                }
                else
                {
                    if (!_fullNameCache.TryGetValue(typeName, out type))
                    {
                        if (_nameCache.TryGetValue(typeName, out type) && PlatformExtensions.TypeCacheOnlyUsedTypeToBootstrapCodeBuilder)
                            WriteTypeUsage(ServiceProvider.BootstrapCodeBuilder, typeName, type, false);
                    }
                    else if (PlatformExtensions.TypeCacheOnlyUsedTypeToBootstrapCodeBuilder)
                        WriteTypeUsage(ServiceProvider.BootstrapCodeBuilder, typeName, type, true);
                }
            }
            if (type == null)
            {
                if (throwOnError)
                    throw new ArgumentException("The type with name '" + typeName + "' was not found.", nameof(typeName));
            }
            return type;
        }

        public void AddAssembly(Assembly assembly)
        {
            lock (_locker)
            {
                if (!_cachedAssemblies.Add(assembly))
                    return;
                var types = assembly.GetTypes();
                for (int index = 0; index < types.Length; index++)
                    Add(types[index]);
            }
        }

        public void AddType(Type type, string alias = null)
        {
            lock (_locker)
                Add(type, alias);
        }

        public void AddFullName(string name, Type type)
        {
            //only for initialization
            // ReSharper disable InconsistentlySynchronizedField
            _fullNameCache[name] = type;
            _ignoreCaseFullNameCache[name] = type;
            // ReSharper restore InconsistentlySynchronizedField
        }

        public void AddName(string name, Type type)
        {
            //only for initialization
            // ReSharper disable InconsistentlySynchronizedField
            _nameCache[name] = type;
            _ignoreCaseNameCache[name] = type;
            // ReSharper restore InconsistentlySynchronizedField
        }

        private void Add(Type type, string alias = null)
        {
            if (!typeof(TType).IsAssignableFrom(type) || type.IsGenericTypeDefinition || !type.IsPublic)
                return;
            if (typeof(IJavaObject).IsAssignableFrom(type))
            {
                try
                {
                    var name = Class.FromType(type).Name;
                    _fullNameCache[name] = type;
                    _ignoreCaseFullNameCache[name] = type;
                }
                catch
                {
                    return;
                }
            }

            foreach (var attribute in type.GetCustomAttributes<TypeNameAliasAttribute>(false))
            {
                _nameCache[attribute.Alias] = type;
                _ignoreCaseNameCache[attribute.Alias] = type;
            }
            if (alias != null)
            {
                _nameCache[alias] = type;
                _ignoreCaseNameCache[alias] = type;
            }

            if (!string.IsNullOrEmpty(type.FullName))
            {
                _fullNameCache[type.FullName] = type;
                _ignoreCaseFullNameCache[type.FullName] = type;
            }
            if (!string.IsNullOrEmpty(type.Name))
            {
                _nameCache[type.Name] = type;
                _ignoreCaseNameCache[type.Name] = type;
            }
        }

        private static void WriteTypeUsage(IBootstrapCodeBuilder builder, string name, Type type, bool isFullName)
        {
            if (builder != null)
            {
                if (_usedTypes == null)
                    _usedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (_usedTypes.Add(name))
                    builder.Append("TypeCache",
                          $"{typeof(TypeCache<TType>).GetPrettyName()}.{nameof(Instance)}.{(isFullName ? nameof(AddFullName) : nameof(AddName))}(\"{name}\", typeof({type.GetPrettyName()}));");
            }
        }

        #endregion
    }
}