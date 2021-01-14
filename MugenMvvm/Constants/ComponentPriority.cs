namespace MugenMvvm.Constants
{
    public static class ComponentPriority
    {
        public const int Max = int.MaxValue - 10;
        public const int Min = int.MinValue + 10;
        public const int Cache = int.MaxValue - 300;
        public const int Decorator = int.MaxValue / 2;
        public const int PreInitializer = Decorator - 10000;
        public const int PostInitializer = int.MinValue / 2;
    }
}