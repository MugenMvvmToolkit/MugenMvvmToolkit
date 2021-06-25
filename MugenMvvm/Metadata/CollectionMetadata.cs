﻿namespace MugenMvvm.Metadata
{
    public static class CollectionMetadata
    {
        public static int DiffUtilAsyncLimit = 100;
        public static int DiffUtilMaxLimit = 2000;
        public static int BindableCollectionAdapterBatchDelay = 50;
        public static int BindableCollectionAdapterBatchLimit = 10;
        public static int FlattenCollectionDecoratorBatchLimit = 30;

        public static readonly object ReloadItem = new(); //todo scroll to item
    }
}