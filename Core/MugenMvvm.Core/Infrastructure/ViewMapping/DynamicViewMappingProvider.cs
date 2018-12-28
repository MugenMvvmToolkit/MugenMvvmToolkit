using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Infrastructure.ViewMapping
{
    public class DynamicViewMappingProvider : StaticViewMappingProvider
    {
        #region Fields

        private IEnumerable<Assembly>? _assemblies;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public DynamicViewMappingProvider(IEnumerable<Assembly> assemblies)
            : this(assemblies, null, null)
        {
        }

        [Preserve(Conditional = true)]
        public DynamicViewMappingProvider(IEnumerable<Assembly> assemblies, IReadOnlyList<string>? viewPostfix, IReadOnlyList<string>? viewModelPostfix)
        {
            Should.NotBeNull(assemblies, nameof(assemblies));
            _assemblies = assemblies;
            ViewPostfix = viewPostfix ?? new[]
            {
                "ActivityView", "ViewActivity", "FragmentView", "ViewFragment", "WindowView", "ViewController", "PageView", "FormView", "ModalView",
                "Form", "View", "V", "Activity", "Fragment", "Page", "Window", "Controller"
            };
            ViewModelPostfix = viewModelPostfix ?? new[] { "ViewModel", "Vm" };
        }

        #endregion

        #region Properties

        public bool IsSupportedUriNavigation { get; set; }

        protected IReadOnlyList<string> ViewPostfix { get; }

        protected IReadOnlyList<string> ViewModelPostfix { get; }

        private bool IsDesignMode => false;//todo fix

        #endregion

        #region Methods

        protected override void EnsureInitialized()
        {
            if (_assemblies == null)
                return;

            lock (Locker)
            {
                if (_assemblies == null)
                    return;

                var assemblies = _assemblies;
                _assemblies = null;
                InitializeMapping(assemblies.SelectMany(assembly => assembly.GetTypesUnified(!IsDesignMode)));
            }
        }

        protected virtual void InitializeMapping(IEnumerable<Type> types)
        {
            var vTypes = new Dictionary<string, HashSet<Type>>();
            var vmTypes = new Dictionary<string, HashSet<Type>>();
            foreach (var type in types)
            {
                if (type.IsAbstractUnified() || type.IsInterfaceUnified())
                    continue;
                var viewModelAttributes = type
                    .GetCustomAttributesUnified(typeof(ViewModelAttribute), true)
                    .OfType<ViewModelAttribute>()
                    .ToList();

                if (viewModelAttributes.Count == 0)
                {
                    if (typeof(IViewModel).IsAssignableFromUnified(type))
                    {
                        TryGetNames(type, ViewModelPostfix, out var names);
                        foreach (var name in names)
                        {
                            if (!vmTypes.TryGetValue(name, out var viewModels))
                            {
                                viewModels = new HashSet<Type>();
                                vmTypes[name] = viewModels;
                            }
                            viewModels.Add(type);
                        }
                    }
                    else if (IsViewType(type))
                    {
                        if (TryGetNames(type, ViewPostfix, out var names))
                        {
                            foreach (var name in names)
                            {
                                if (!vTypes.TryGetValue(name, out var views))
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
                    for (int i = 0; i < viewModelAttributes.Count; i++)
                    {
                        var attribute = viewModelAttributes[i];
                        AddMapping(new ViewMappingItem(attribute.ViewModelType, type, attribute.Name, GetUri(type, attribute.ViewModelType, attribute.Uri), attribute.UriKind), true);
                    }
                }
            }

            foreach (var vType in vTypes)
            {
                foreach (var originalViewType in vType.Value)
                {
                    if (!vmTypes.TryGetValue(vType.Key, out var list))
                        continue;
                    vmTypes.Remove(vType.Key);
                    foreach (Type viewModelType in list)
                    {
                        //NOTE: ignore if view already has mapping.
                        AddMapping(new ViewMappingItem(viewModelType, originalViewType, null, GetUri(originalViewType, viewModelType, null), UriKind.Relative), false);
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
                        if (!TryGetNames(baseType, ViewModelPostfix, out var names))
                            continue;
                        bool added = false;

                        for (int j = 0; j < names.Count; j++)
                        {
                            if (!vTypes.TryGetValue(names[j], out var viewTypes))
                                continue;

                            foreach (var viewType in viewTypes)
                                AddMapping(new ViewMappingItem(vmType, viewType, null, GetUri(viewType, vmType, null), UriKind.Relative), false);
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

        protected virtual string? GetUri(Type viewType, Type viewModelType, string? url)
        {
            if (!string.IsNullOrEmpty(url))
                return url;
            if (!IsSupportedUriNavigation)
                return null;
            Assembly assembly = viewType.GetAssemblyUnified();
            string name = assembly.GetAssemblyNameUnified().Name;
            string uri = viewType.FullName.Replace(name, string.Empty).Replace(".", "/");
            return $"/{name};component{uri}.xaml";
        }

        private static bool TryGetNames(Type type, IReadOnlyList<string> postFixes, out List<string> names)
        {
            names = new List<string>();
            for (int i = 0; i < postFixes.Count; i++)
            {
                var postFix = postFixes[i];
                var typeName = type.Name;
                if (type.IsGenericTypeDefinitionUnified())
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

        private static List<Type> GetBaseClasses(Type type)
        {
            type = type.GetBaseTypeUnified();
            var types = new List<Type>();
            while (type != null && typeof(IViewModel).IsAssignableFromUnified(type))
            {
                types.Add(type);
                type = type.GetBaseTypeUnified();
            }
            return types;
        }

        #endregion
    }
}