#region Copyright
// ****************************************************************************
// <copyright file="TypeCache.cs">
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
using System.Reflection;

namespace MugenMvvmToolkit.Infrastructure
{
    public sealed class TypeCache<TType>
    {
        #region Fields

        /// <summary>
        ///     Gets the instance of cache.
        /// </summary>
        public static readonly TypeCache<TType> Instance;

        private readonly HashSet<Assembly> _cachedAssemblies;
        private readonly IDictionary<string, Type> _fullNameCache;

        private readonly IDictionary<string, Type> _ignoreCaseFullNameCache;
        private readonly IDictionary<string, Type> _ignoreCaseNameCache;
        private readonly object _locker;
        private readonly IDictionary<string, Type> _nameCache;

        #endregion

        #region Constructors

        static TypeCache()
        {
            Instance = new TypeCache<TType>();
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

        public static void Initialize(IList<Assembly> assemblies)
        {
            if (assemblies == null)
                return;
            for (int index = 0; index < assemblies.Count; index++)
                Instance.AddAssembly(assemblies[index]);
        }

        public Type GetTypeByName(string typeName, bool ignoreCase, bool throwOnError)
        {
            Type type;
            lock (_locker)
            {
                if (ignoreCase)
                {
                    if (!_ignoreCaseFullNameCache.TryGetValue(typeName, out type))
                        _ignoreCaseNameCache.TryGetValue(typeName, out type);
                }
                else
                {
                    if (!_fullNameCache.TryGetValue(typeName, out type))
                        _nameCache.TryGetValue(typeName, out type);
                }
            }
            if (type == null)
            {
                var message = string.Format("The type with name '{0}' was not found.", typeName);
                if (throwOnError)
                    throw new ArgumentException(message, "typeName");
                Tracer.Warn(message);
            }
            return type;
        }

        /// <summary>
        ///     Adds the assembly to scan.
        /// </summary>
        public void AddAssembly(Assembly assembly)
        {
            lock (_locker)
            {
                if (!_cachedAssemblies.Add(assembly))
                    return;
            }
            var types = assembly.GetTypes();
            for (int index = 0; index < types.Length; index++)
                Add(types[index]);
        }

        private void Add(Type type)
        {
            if (!typeof(TType).IsAssignableFrom(type))
                return;
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

        #endregion
    }
}