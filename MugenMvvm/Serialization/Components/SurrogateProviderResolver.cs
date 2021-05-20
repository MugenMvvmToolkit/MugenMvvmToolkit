using System;
using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Serialization.Components
{
    public sealed class SurrogateProviderResolver : ISurrogateProviderResolverComponent, IHasPriority, IHasCache
    {
        private readonly Dictionary<Type, ISurrogateProvider?> _cache;
        private readonly Dictionary<Type, ISurrogateProvider> _surrogateProviders;

        public SurrogateProviderResolver()
        {
            _surrogateProviders = new Dictionary<Type, ISurrogateProvider>(InternalEqualityComparer.Type);
            _cache = new Dictionary<Type, ISurrogateProvider?>(InternalEqualityComparer.Type);
        }

        public int Priority { get; set; } = SerializationComponentPriority.SurrogateProvider;

        public void Add<TFrom, TSurrogate>(DelegateSurrogateProvider<TFrom, TSurrogate> surrogateProvider)
            where TFrom : class
            where TSurrogate : class
        {
            Should.NotBeNull(surrogateProvider, nameof(surrogateProvider));
            Add(surrogateProvider.FromType, surrogateProvider);
        }

        public void Add(Type type, ISurrogateProvider surrogateProvider)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(surrogateProvider, nameof(surrogateProvider));
            _surrogateProviders[type] = surrogateProvider;
            _cache.Clear();
        }

        public void Remove(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            _surrogateProviders.Remove(type);
            _cache.Clear();
        }

        public void Invalidate(object sender, object? state = null, IReadOnlyMetadataContext? metadata = null) => _cache.Clear();

        public ISurrogateProvider? TryGetSurrogateProvider(ISerializer serializer, Type type, ISerializationContext? serializationContext)
        {
            if (!_cache.TryGetValue(type, out var value))
            {
                value = TryGetSurrogateProvider(type);
                _cache[type] = value;
            }

            return value;
        }

        private ISurrogateProvider? TryGetSurrogateProvider(Type type)
        {
            if (!_surrogateProviders.TryGetValue(type, out var value))
            {
                foreach (var provider in _surrogateProviders)
                {
                    if (provider.Key.IsAssignableFrom(type))
                        return provider.Value;
                }
            }

            return value;
        }
    }
}