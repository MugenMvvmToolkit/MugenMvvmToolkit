namespace MugenMvvm.Constants
{
    public static class CollectionComponentPriority
    {
        public const int SortingDecorator = 0;
        public const int GroupHeaderDecorator = 9;
        public const int FilterDecorator = 10;
        public const int DecoratorManager = 100;
        public const int HeaderFooterDecorator = ComponentPriority.PostInitializer + 1000;
        public const int FlattenCollectionDecorator = HeaderFooterDecorator - 1;
        public const int ItemObserverDecorator = ComponentPriority.Min;
    }
}