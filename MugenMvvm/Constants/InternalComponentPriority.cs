namespace MugenMvvm.Constants
{
    public static class InternalComponentPriority
    {
        public const int WeakReferenceProvider = 0;
        public const int DelegateProvider = 0;
        public const int DelegateProviderCache = ComponentPriority.Cache;
        public const int ValueHolderWeakReferenceCache = ComponentPriority.Cache + 1;

        public const int Logger = 0;
        public const int ValueHolderAttachedValueProvider = 2;
        public const int MetadataOwnerAttachedValueProvider = 1;
        public const int WeakTableAttachedValueProvider = -1;
        public const int StaticTypeAttachedValueProvider = 3;
    }
}