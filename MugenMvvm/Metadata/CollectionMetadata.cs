namespace MugenMvvm.Metadata
{
    public static class CollectionMetadata
    {
        public static readonly object ReloadItem = new();
        public static int DiffUtilAsyncLimit = 300;
        public static int DiffUtilMaxLimit = 5000;
        public static int BindableCollectionAdapterBatchDelay = 100;
        public static int BindableCollectionAdapterBatchLimit = 10;
        public static int FlattenCollectionDecoratorBatchLimit = 30;
    }
}