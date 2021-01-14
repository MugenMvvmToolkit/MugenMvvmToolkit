using System.Runtime.InteropServices;

namespace MugenMvvm.Bindings.Resources
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ResourceResolverResult
    {
        public readonly bool IsResolved;
        public readonly object? Resource;

        public ResourceResolverResult(object? resource)
        {
            Resource = resource;
            IsResolved = true;
        }
    }
}