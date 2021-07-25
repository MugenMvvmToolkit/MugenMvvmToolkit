namespace MugenMvvm.Metadata
{
    public static class CollectionMetadata
    {
        public static readonly object ReloadItem = new();
        public static int DiffUtilAsyncThreshold = 300;
        public static int DiffUtilMaxThreshold = 5000;
        public static int BindableCollectionAdapterBatchDelay = 75;
        public static int BindableCollectionAdapterBatchThreshold = 25;
        public static int FlattenCollectionDecoratorBatchThreshold = 30;
    }
}