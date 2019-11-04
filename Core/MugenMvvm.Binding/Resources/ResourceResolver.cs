using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Resources
{
    public class ResourceResolver : ComponentOwnerBase<IResourceResolver>, IResourceResolver, IComponentOwnerAddedCallback<IComponent<IResourceResolver>>,
        IComponentOwnerRemovedCallback<IComponent<IResourceResolver>>
    {
        #region Fields

        protected IBindingValueConverterResolverComponent[] ConverterResolvers;
        protected IResourceResolverComponent[] ResourceResolvers;
        protected ITypeResolverComponent[] TypeResolvers;

        #endregion

        #region Constructors

        public ResourceResolver(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
            TypeResolvers = Default.EmptyArray<ITypeResolverComponent>();
            ResourceResolvers = Default.EmptyArray<IResourceResolverComponent>();
            ConverterResolvers = Default.EmptyArray<IBindingValueConverterResolverComponent>();
            Converters = new Dictionary<string, IBindingValueConverter>();
            Resources = new Dictionary<string, IResourceValue>();
            Types = new Dictionary<string, Type>
            {
                {"object", typeof(object)},
                {"bool", typeof(bool)},
                {"char", typeof(char)},
                {"string", typeof(string)},
                {"sbyte", typeof(sbyte)},
                {"byte", typeof(byte)},
                {"short", typeof(short)},
                {"ushort", typeof(ushort)},
                {"int", typeof(int)},
                {"uint", typeof(uint)},
                {"long", typeof(long)},
                {"ulong", typeof(ulong)},
                {"float", typeof(float)},
                {"double", typeof(double)},
                {"decimal", typeof(decimal)}
            };
            AddType(typeof(object));
            AddType(typeof(bool));
            AddType(typeof(char));
            AddType(typeof(string));
            AddType(typeof(sbyte));
            AddType(typeof(byte));
            AddType(typeof(short));
            AddType(typeof(ushort));
            AddType(typeof(int));
            AddType(typeof(uint));
            AddType(typeof(long));
            AddType(typeof(ulong));
            AddType(typeof(float));
            AddType(typeof(double));
            AddType(typeof(decimal));
            AddType(typeof(DateTime));
            AddType(typeof(TimeSpan));
            AddType(typeof(Guid));
            AddType(typeof(Math));
            AddType(typeof(Convert));
            AddType(typeof(Enumerable));
            AddType(typeof(Environment));
        }

        #endregion

        #region Properties

        public Dictionary<string, Type> Types { get; }

        public Dictionary<string, IBindingValueConverter> Converters { get; }

        public Dictionary<string, IResourceValue> Resources { get; }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback<IComponent<IResourceResolver>>.OnComponentAdded(IComponentCollection<IComponent<IResourceResolver>> collection,
            IComponent<IResourceResolver> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentAdded(collection, component, metadata);
        }

        void IComponentOwnerRemovedCallback<IComponent<IResourceResolver>>.OnComponentRemoved(IComponentCollection<IComponent<IResourceResolver>> collection,
            IComponent<IResourceResolver> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentRemoved(collection, component, metadata);
        }

        public IResourceValue? TryGetResourceValue(string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(name, nameof(name));
            return TryGetResourceValueInternal(name, metadata);
        }

        public IBindingValueConverter? TryGetConverter(string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(name, nameof(name));
            return TryGetConverterInternal(name, metadata);
        }

        public Type? TryGetType(string name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(name, nameof(name));
            return TryGetTypeInternal(name, metadata);
        }

        #endregion

        #region Methods

        public void AddType(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            Types[type.Name] = type;
            var fullName = type.FullName;
            if (fullName != null)
                Types[fullName] = type;
        }

        protected virtual void OnComponentAdded(IComponentCollection<IComponent<IResourceResolver>> collection, IComponent<IResourceResolver> component,
            IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref ConverterResolvers, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref ResourceResolvers, collection, component);
            MugenExtensions.ComponentTrackerOnAdded(ref TypeResolvers, collection, component);
        }

        protected virtual void OnComponentRemoved(IComponentCollection<IComponent<IResourceResolver>> collection, IComponent<IResourceResolver> component,
            IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref ConverterResolvers, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref ResourceResolvers, component);
            MugenExtensions.ComponentTrackerOnRemoved(ref TypeResolvers, component);
        }

        protected virtual IResourceValue? TryGetResourceValueInternal(string name, IReadOnlyMetadataContext? metadata)
        {
            var resolvers = ResourceResolvers;
            for (var i = 0; i < resolvers.Length; i++)
            {
                var value = resolvers[i].TryGetResourceValue(name, metadata);
                if (value != null)
                    return value;
            }

            if (Resources.TryGetValue(name, out var v))
                return v;
            return null;
        }

        protected virtual IBindingValueConverter? TryGetConverterInternal(string name, IReadOnlyMetadataContext? metadata)
        {
            var resolvers = ConverterResolvers;
            for (var i = 0; i < resolvers.Length; i++)
            {
                var converter = resolvers[i].TryGetConverter(name, metadata);
                if (converter != null)
                    return converter;
            }

            if (Converters.TryGetValue(name, out var v))
                return v;
            return null;
        }

        protected virtual Type? TryGetTypeInternal(string name, IReadOnlyMetadataContext? metadata)
        {
            var resolvers = TypeResolvers;
            for (var i = 0; i < resolvers.Length; i++)
            {
                var type = resolvers[i].TryGetType(name, metadata);
                if (type != null)
                    return type;
            }

            if (Types.TryGetValue(name, out var v))
                return v;
            return null;
        }

        #endregion
    }
}