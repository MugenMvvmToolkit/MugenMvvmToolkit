namespace MugenMvvm.Constants
{
    public static class CollectionComponentPriority
    {
        public const int DecoratorManager = ComponentPriority.PreInitializer + 1000;
        public const int BindableAdapter = ComponentPriority.PreInitializer;
        public const int ConverterDecorator = 40;
        public const int FilterDecorator = 30;
        public const int GroupHeaderDecorator = 20;
        public const int SortingDecorator = 10;
        public const int LimitDecorator = 0;
        public const int HeaderFooterDecorator = ComponentPriority.PostInitializer + 1000;
        public const int FlattenCollectionDecorator = ComponentPriority.PostInitializer;
    }
}