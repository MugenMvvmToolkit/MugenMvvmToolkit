namespace MugenMvvm.Metadata
{
    public static class CollectionMetadata
    {
        public static readonly object TrueFilterArgs = new();
        public static readonly object FalseFilterArgs = new();

        public static readonly object ReloadItem = new();

        public static bool RaiseItemChangedCheckDuplicates { get; set; }

        public static bool RaiseItemChangedAsync { get; set; }

        public static int RaiseItemChangedLockTimeout { get; set; }

        public static int RaiseItemChangedDelay { get; set; } = 10;

        public static int RaiseItemChangedResetThreshold { get; set; } = 30;

        public static int DiffUtilAsyncThreshold { get; set; } = 300;

        public static int DiffUtilMaxThreshold { get; set; } = 5000;

        public static int BindableCollectionAdapterLockTimeout { get; set; } = 80;

        public static int BindableCollectionAdapterBatchDelay { get; set; } = 75;

        public static int BindableCollectionAdapterBatchThreshold { get; set; } = 25;

        public static int FlattenCollectionDecoratorBatchThreshold { get; set; } = 30;
    }
}