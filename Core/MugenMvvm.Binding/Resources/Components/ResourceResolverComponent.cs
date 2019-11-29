﻿using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Interfaces.Resources.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Resources.Components
{
    public sealed class ResourceResolverComponent : IResourceResolverComponent, IHasPriority
    {
        #region Constructors

        public ResourceResolverComponent()
        {
            Resources = new Dictionary<string, IResourceValue>();
        }

        #endregion

        #region Properties

        public IDictionary<string, IResourceValue> Resources { get; }

        public int Priority { get; set; } = ResourceComponentPriority.ConverterResolver;

        #endregion

        #region Implementation of interfaces

        public IResourceValue? TryGetResourceValue(string name, IReadOnlyMetadataContext? metadata)
        {
            Resources.TryGetValue(name, out var value);
            return value;
        }

        #endregion

        #region Methods

        public void AddResource(string name, IResourceValue resource)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(resource, nameof(resource));
            Resources[name] = resource;
        }

        #endregion
    }
}