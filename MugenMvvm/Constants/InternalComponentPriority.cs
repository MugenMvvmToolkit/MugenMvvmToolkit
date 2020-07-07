namespace MugenMvvm.Constants
{
    public static class InternalComponentPriority
    {
        #region Fields

        public const int WeakReferenceProvider = 0;
        public const int DelegateProvider = 0;
        public const int Tracer = 0;
        public const int ValueHolderAttachedValueProvider = 2;
        public const int MetadataOwnerAttachedValueProvider = 1;
        public const int AttachedValueProvider = 0;
        public const int StaticTypeAttachedValueProvider = 3;

        #endregion
    }
}