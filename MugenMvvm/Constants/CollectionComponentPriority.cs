namespace MugenMvvm.Constants
{
    public static class CollectionComponentPriority
    {
        public const int OrderDecorator = 0;
        public const int FilterDecorator = 1;
        public const int DecoratorManager = 10;
        public const int HeaderFooterDecorator = ComponentPriority.PostInitializer + 1000;
    }
}