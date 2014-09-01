#region Copyright
// ****************************************************************************
// <copyright file="ViewMappingProvider.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the <see cref="IViewMappingProvider" /> that uses attributes to fill mappings.
    /// </summary>
    public class ViewMappingProvider : IViewMappingProvider
    {
        #region Fields

        private readonly Dictionary<Type, Dictionary<string, IViewMappingItem>> _viewModelToMapping;
        private readonly Dictionary<Type, List<IViewMappingItem>> _viewTypeToMapping;
        private readonly IList<string> _viewPostfix;
        private readonly IList<string> _viewModelPostfix;
        private IEnumerable<Assembly> _assemblies;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewMappingProvider" /> class.
        /// </summary>
        public ViewMappingProvider([NotNull] IEnumerable<Assembly> assemblies)
            : this(assemblies, null, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewMappingProvider" /> class.
        /// </summary>
        public ViewMappingProvider([NotNull] IEnumerable<Assembly> assemblies, IList<string> viewPostfix, IList<string> viewModelPostfix)
        {
            Should.NotBeNull(assemblies, "assemblies");
            _assemblies = assemblies;
            _viewPostfix = viewPostfix ?? new[]
            {
                "ActivityView", "FragmentView", "WindowView", "PageView", "FormView", 
                "Form", "View", "V", "Activity", "Fragment", "Page", "Window"
            };
            _viewModelPostfix = viewModelPostfix ?? new[] { "ViewModel", "Vm" };
            _viewTypeToMapping = new Dictionary<Type, List<IViewMappingItem>>();
            _viewModelToMapping = new Dictionary<Type, Dictionary<string, IViewMappingItem>>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the view postfixes.
        /// </summary>
        protected IList<string> ViewPostfix
        {
            get { return _viewPostfix; }
        }

        /// <summary>
        /// Gets the view model postfixes.
        /// </summary>
        protected IList<string> ViewModelPostfix
        {
            get { return _viewModelPostfix; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes view mappings.
        /// </summary>
        protected virtual void InitializeMapping(IEnumerable<Type> types)
        {
            var vTypes = new Dictionary<string, HashSet<Type>>();
            var vmTypes = new Dictionary<string, HashSet<Type>>();
            foreach (var type in types)
            {
#if PCL_WINRT
                var typeInfo = type.GetTypeInfo();
                if (typeInfo.IsAbstract || typeInfo.IsInterface)
                    continue;
                ViewModelAttribute[] viewModelAttributes = typeInfo
                    .GetCustomAttributes<ViewModelAttribute>(true)
                    .ToArray();
#else
                if (type.IsAbstract || type.IsInterface)
                    continue;
                ViewModelAttribute[] viewModelAttributes = type
                    .GetCustomAttributes(typeof(ViewModelAttribute), true)
                    .ToArrayFast(o => (ViewModelAttribute)o);
#endif

                if (viewModelAttributes.Length == 0)
                {
                    if (typeof(IViewModel).IsAssignableFrom(type))
                    {
                        List<string> names;
                        TryGetNames(type, _viewModelPostfix, out names);
                        foreach (var name in names)
                        {
                            HashSet<Type> viewModels;
                            if (!vmTypes.TryGetValue(name, out viewModels))
                            {
                                viewModels = new HashSet<Type>();
                                vmTypes[name] = viewModels;
                            }
                            viewModels.Add(type);
                        }
                    }
                    else
                    {
                        List<string> names;
                        if (TryGetNames(type, _viewPostfix, out names))
                        {
                            foreach (var name in names)
                            {
                                HashSet<Type> views;
                                if (!vTypes.TryGetValue(name, out views))
                                {
                                    views = new HashSet<Type>();
                                    vTypes[name] = views;
                                }
                                views.Add(type);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < viewModelAttributes.Length; i++)
                    {
                        var attribute = viewModelAttributes[i];
                        AddMapping(new ViewMappingItem(attribute.ViewModelType, type, attribute.Name,
                            GetUri(type, attribute.ViewModelType, attribute.Uri, attribute.UriKind)), true);
                    }
                }
            }

            foreach (var vType in vTypes)
            {
                foreach (var originalViewType in vType.Value)
                {
                    HashSet<Type> list;
                    if (!vmTypes.TryGetValue(vType.Key, out list))
                        continue;
                    foreach (Type viewModelType in list)
                    {
                        //NOTE: ignore if view is already have mapping.
                        AddMapping(new ViewMappingItem(viewModelType, originalViewType, null,
                            GetUri(originalViewType, viewModelType, null, UriKind.Relative)), false);
                    }
                }
            }
        }

        /// <summary>
        ///     Creates an <see cref="Uri" /> for the specified type of view.
        /// </summary>
        /// <param name="viewType">The specified type of view.</param>
        /// <param name="viewModelType">The specified type of view model.</param>
        /// <param name="url">The specified url value.</param>
        /// <param name="uriKind">
        ///     The specified <see cref="UriKind" />.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="Uri" />.
        /// </returns>
        protected virtual Uri GetUri(Type viewType, Type viewModelType, string url, UriKind uriKind)
        {
            if (!string.IsNullOrEmpty(url))
                return new Uri(url, uriKind);
            Assembly assembly = viewType.GetAssembly();
            string name = assembly.GetAssemblyName().Name;
            string uri = viewType.FullName.Replace(name, string.Empty).Replace(".", "/");
            return new Uri(string.Format("/{0};component{1}.xaml", name, uri), uriKind);
        }

        /// <summary>
        /// Adds the view mapping to internal collection.
        /// </summary>
        protected void AddMapping(IViewMappingItem mappingItem, bool throwOnError)
        {
            List<IViewMappingItem> list;
            if (!_viewTypeToMapping.TryGetValue(mappingItem.ViewType, out list))
            {
                list = new List<IViewMappingItem>();
                _viewTypeToMapping[mappingItem.ViewType] = list;
            }
            list.Add(mappingItem);

            Dictionary<string, IViewMappingItem> value;
            if (!_viewModelToMapping.TryGetValue(mappingItem.ViewModelType, out value))
            {
                value = new Dictionary<string, IViewMappingItem>();
                _viewModelToMapping[mappingItem.ViewModelType] = value;
            }
            IViewMappingItem item;
            string name = mappingItem.Name ?? string.Empty;
            if (value.TryGetValue(name, out item))
            {
                if (throwOnError)
                    throw ExceptionManager.DuplicateViewMapping(item.ViewType, item.ViewModelType, item.Name);
                return;
            }
            value[name] = mappingItem;
            Tracer.Info("The view mapping to view model was created: ({0} ---> {1}), name: {2}",
                mappingItem.ViewModelType, mappingItem.ViewType, mappingItem.Name);
        }

        private static bool TryGetNames(Type type, IList<string> postFixes, out List<string> names)
        {
            names = new List<string>();
            for (int i = 0; i < postFixes.Count; i++)
            {
                var postFix = postFixes[i];
                var typeName = type.Name;
#if PCL_WINRT
                if (type.GetTypeInfo().IsGenericTypeDefinition)
#else
                if (type.IsGenericTypeDefinition)
#endif
                {
                    var index = typeName.IndexOf('`');
                    if (index > 0)
                        typeName = typeName.Substring(0, index);
                }

                if (typeName.EndsWith(postFix))
                {
                    var name = typeName.Substring(0, typeName.Length - postFix.Length);
                    names.Add(name);
                }
            }
            return names.Count != 0;
        }

        private void EnsureInitialized()
        {
            //NOTE to keep actual mapping in design mode.
            if (ServiceProvider.DesignTimeManager.IsDesignMode)
            {
                lock (_viewModelToMapping)
                {
                    _viewModelToMapping.Clear();
                    _viewTypeToMapping.Clear();
                    InitializeMapping(ReflectionExtensions.GetDesignAssemblies().SelectMany(assembly => assembly.SafeGetTypes(false)));
                    return;
                }
            }
            if (_assemblies == null)
                return;
            lock (_viewModelToMapping)
            {
                var assemblies = _assemblies;
                _assemblies = null;
                InitializeMapping(assemblies.SelectMany(assembly => assembly.SafeGetTypes(true)));
            }
        }

        #endregion

        #region Implementation of INavigableViewMappingProvider

        /// <summary>
        ///     Gets a series instances of <see cref="IViewMappingItem" />.
        /// </summary>
        public IEnumerable<IViewMappingItem> ViewMappings
        {
            get
            {
                EnsureInitialized();
                return _viewModelToMapping.Values.SelectMany(items => items.Values);
            }
        }

        /// <summary>
        ///     Finds the series of <see cref="IViewMappingItem" /> for the specified type of view.
        /// </summary>
        /// <param name="viewType">The specified type of view.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>
        ///     The series of <see cref="IViewMappingItem" />.
        /// </returns>
        public IList<IViewMappingItem> FindMappingsForView(Type viewType, bool throwOnError)
        {
            Should.NotBeNull(viewType, "viewType");
            EnsureInitialized();
            List<IViewMappingItem> item;
            if (!_viewTypeToMapping.TryGetValue(viewType, out item) && throwOnError)
                throw ExceptionManager.ViewModelNotFound(viewType);
            if (item == null)
                return Empty.Array<IViewMappingItem>();
            return item.ToArrayFast();
        }

        /// <summary>
        ///     Finds the <see cref="IViewMappingItem" /> for the specified type of view model.
        /// </summary>
        /// <param name="viewModelType">The specified type of view model.</param>
        /// <param name="viewName">The specified name of view, if any.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="IViewMappingItem" />.
        /// </returns>
        public IViewMappingItem FindMappingForViewModel(Type viewModelType, string viewName, bool throwOnError)
        {
            Should.BeOfType<IViewModel>(viewModelType, "viewModelType");
            EnsureInitialized();
            viewName = viewName ?? string.Empty;
            Dictionary<string, IViewMappingItem> value;
            if (!_viewModelToMapping.TryGetValue(viewModelType, out value))
            {
#if PCL_WINRT
                if (viewModelType.GetTypeInfo().IsGenericType)
#else
                if (viewModelType.IsGenericType)
#endif
                {
                    viewModelType = viewModelType.GetGenericTypeDefinition();
                    if (viewModelType != null)
                        _viewModelToMapping.TryGetValue(viewModelType, out value);
                }
            }
            if (value == null)
            {
                if (throwOnError)
                    throw ExceptionManager.ViewNotFound(viewModelType);
                return null;
            }
            IViewMappingItem item;
            if (!value.TryGetValue(viewName, out item))
            {
                if (viewName != string.Empty)
                    value.TryGetValue(string.Empty, out item);
            }
            if (item == null && throwOnError)
                throw ExceptionManager.ViewNotFound(viewModelType);
            return item;
        }

        #endregion
    }
}