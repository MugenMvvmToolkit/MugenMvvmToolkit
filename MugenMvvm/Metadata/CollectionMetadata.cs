namespace MugenMvvm.Metadata
{
    public static class CollectionMetadata
    {
        public static readonly object ReloadItem = new();

        public static int DiffUtilAsyncThreshold { get; set; } = 300;

        public static int DiffUtilMaxThreshold { get; set; } = 5000;

        public static bool BindableCollectionAdapterInitializeAsync { get; set; } = true;

        public static int BindableCollectionAdapterBatchDelay { get; set; } = 75;

        public static int BindableCollectionAdapterBatchThreshold { get; set; } = 25;

        public static int FlattenCollectionDecoratorBatchThreshold { get; set; } = 30;
    }
}