using System;
using System.Runtime.InteropServices;

namespace MugenMvvm.Bindings.Resources
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ResourceResolverResult : IEquatable<ResourceResolverResult>
    {
        public readonly bool IsResolved;
        public readonly object? Resource;

        public ResourceResolverResult(object? resource)
        {
            Resource = resource;
            IsResolved = true;
        }

        public bool Equals(ResourceResolverResult other) => IsResolved == other.IsResolved && Equals(Resource, other.Resource);

        public override bool Equals(object? obj) => obj is ResourceResolverResult other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(IsResolved, Resource);
    }
}