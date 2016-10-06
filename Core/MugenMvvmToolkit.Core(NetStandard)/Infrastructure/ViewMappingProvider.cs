#region Copyright

// ****************************************************************************
// <copyright file="ViewMappingProvider.cs">
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

        public ViewMappingProvider([NotNull] IEnumerable<Assembly> assemblies)
            : this(assemblies, null, null)
        {
        }

        public ViewMappingProvider([NotNull] IEnumerable<Assembly> assemblies, IList<string> viewPostfix, IList<string> viewModelPostfix)
        {
            Should.NotBeNull(assemblies, nameof(assemblies));
            _assemblies = assemblies;
            _viewPostfix = viewPostfix ?? new[]
            {
                "ActivityView", "ViewActivity", "FragmentView", "ViewFragment", "WindowView", "ViewController", "PageView", "FormView", "ModalView",
                "Form", "View", "V", "Activity", "Fragment", "Page", "Window", "Controller"
            };
            _viewModelPostfix = viewModelPostfix ?? new[] { "ViewModel", "Vm" };
            _viewTypeToMapping = new Dictionary<Type, List<IViewMappingItem>>();
            _viewModelToMapping = new Dictionary<Type, Dictionary<string, IViewMappingItem>>();
            IsSupportedUriNavigation = true;
        }

        #endregion

        #region Properties

        public bool IsSupportedUriNavigation { get; set; }

        protected IList<string> ViewPostfix => _viewPostfix;

        protected IList<string> ViewModelPostfix => _viewModelPostfix;

        #endregion

        #region Methods

        public void AddMapping(IViewMappingItem mappingItem)
        {
            AddMapping(mappingItem, false, true);
        }

        private void AddMapping(IViewMappingItem mappingItem, bool throwOnError, bool rewrite = false)
        {
            var builder = ServiceProvider.BootstrapCodeBuilder;
            if (builder != null)
            {
                string newUri = mappingItem.Uri == ViewMappingItem.Empty ? "null" : $"new {typeof(Uri).FullName}(\"{mappingItem.Uri}\")";
                builder.Append(typeof(ViewMappingProvider).Name,
   $"mappingProvider.{nameof(AddMapping)}(new {typeof(ViewMappingItem).FullName}(typeof({mappingItem.ViewModelType.GetPrettyName()}), typeof({mappingItem.ViewType.GetPrettyName()}), \"{mappingItem.Name}\", {newUri}));");
            }
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
                if (!rewrite)
                    return;
            }
            value[name] = mappingItem;
            if (Tracer.TraceInformation)
                Tracer.Info("The view mapping to view model was created: ({0} ---> {1}), name: {2}",
                    mappingItem.ViewModelType, mappingItem.ViewType, mappingItem.Name);
        }


        protected virtual void InitializeMapping(IEnumerable<Type> types)
        {
            var vTypes = new Dictionary<string, HashSet<Type>>();
            var vmTypes = new Dictionary<string, HashSet<Type>>();
            foreach (var type in types)
            {
#if NET_STANDARD
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
                    .ToArrayEx(o => (ViewModelAttribute)o);
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
                    else if (IsViewType(type))
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
                    vmTypes.Remove(vType.Key);
                    foreach (Type viewModelType in list)
                    {
                        //NOTE: ignore if view is already has mapping.
                        AddMapping(new ViewMappingItem(viewModelType, originalViewType, null,
                            GetUri(originalViewType, viewModelType, null, UriKind.Relative)), false);
                    }
                }
            }

            //Trying to use base class for view models.
            foreach (var keyValuePair in vmTypes)
            {
                foreach (var vmType in keyValuePair.Value)
                {
                    var classes = GetBaseClasses(vmType);
                    for (int i = 0; i < classes.Count; i++)
                    {
                        var baseType = classes[i];
                        List<string> names;
                        if (!TryGetNames(baseType, _viewModelPostfix, out names))
                            continue;
                        bool added = false;

                        for (int j = 0; j < names.Count; j++)
                        {
                            HashSet<Type> viewTypes;
                            if (!vTypes.TryGetValue(names[j], out viewTypes))
                                continue;

                            foreach (var viewType in viewTypes)
                                AddMapping(new ViewMappingItem(vmType, viewType, null,
                                    GetUri(viewType, vmType, null, UriKind.Relative)), false);
                            added = true;
                            break;
                        }
                        if (added)
                            break;
                    }
                }
            }
        }

        protected virtual bool IsViewType(Type type)
        {
            return true;
        }

        protected virtual Uri GetUri(Type viewType, Type viewModelType, string url, UriKind uriKind)
        {
            if (!string.IsNullOrEmpty(url))
                return new Uri(url, uriKind);
            if (!IsSupportedUriNavigation)
                return ViewMappingItem.Empty;
            Assembly assembly = viewType.GetAssembly();
            string name = assembly.GetAssemblyName().Name;
            string uri = viewType.FullName.Replace(name, string.Empty).Replace(".", "/");
            return new Uri($"/{name};component{uri}.xaml", uriKind);
        }

        private static bool TryGetNames(Type type, IList<string> postFixes, out List<string> names)
        {
            names = new List<string>();
            for (int i = 0; i < postFixes.Count; i++)
            {
                var postFix = postFixes[i];
                var typeName = type.Name;
#if NET_STANDARD
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
            if (_assemblies == null)
                return;
            if (ApplicationSettings.ViewMappingProviderDisableAutoRegistration)
            {
                _assemblies = null;
                return;
            }
            lock (_viewModelToMapping)
            {
                var assemblies = _assemblies;
                _assemblies = null;

                var builder = ServiceProvider.BootstrapCodeBuilder;
                if (builder != null)
                {
                    builder.AppendStatic(typeof(ViewMappingProvider).Name, $"{typeof(ApplicationSettings).FullName}.{nameof(ApplicationSettings.ViewMappingProviderDisableAutoRegistration)} = true;");
                    builder.Append(typeof(ViewMappingProvider).Name, $"var mappingProvider = ({typeof(ViewMappingProvider).FullName}) {typeof(ServiceProvider).FullName}.Get<{typeof(IViewMappingProvider).FullName}>();");
                }
                InitializeMapping(assemblies.Where(assembly => assembly.IsToolkitAssembly()).SelectMany(assembly => assembly.SafeGetTypes(true)));
            }
        }

        private static List<Type> GetBaseClasses(Type type)
        {
#if NET_STANDARD
            type = type.GetTypeInfo().BaseType;
#else
            type = type.BaseType;
#endif
            var types = new List<Type>();
            while (type != null && typeof(IViewModel).IsAssignableFrom(type))
            {
                types.Add(type);
#if NET_STANDARD
                type = type.GetTypeInfo().BaseType;
#else
                type = type.BaseType;
#endif
            }
            return types;
        }

        #endregion

        #region Implementation of INavigableViewMappingProvider

        public IEnumerable<IViewMappingItem> ViewMappings
        {
            get
            {
                EnsureInitialized();
                return _viewModelToMapping.Values.SelectMany(items => items.Values);
            }
        }

        public IList<IViewMappingItem> FindMappingsForView(Type viewType, bool throwOnError)
        {
            Should.NotBeNull(viewType, nameof(viewType));
            EnsureInitialized();
            List<IViewMappingItem> item;
            if (!_viewTypeToMapping.TryGetValue(viewType, out item) && throwOnError)
                throw ExceptionManager.ViewModelNotFound(viewType);
            if (item == null)
                return Empty.Array<IViewMappingItem>();
            return item.ToArrayEx();
        }

        public IViewMappingItem FindMappingForViewModel(Type viewModelType, string viewName, bool throwOnError)
        {
            Should.BeOfType<IViewModel>(viewModelType, "viewModelType");
            EnsureInitialized();
            viewName = viewName ?? string.Empty;
            Dictionary<string, IViewMappingItem> value;
            if (!_viewModelToMapping.TryGetValue(viewModelType, out value))
            {
#if NET_STANDARD
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
