namespace MugenMvvm.Enums
{
    public class BatchUpdateType : EnumBase<BatchUpdateType, int>
    {
        public static readonly BatchUpdateType Source = new(1);
        public static readonly BatchUpdateType Decorators = new(2);

        public BatchUpdateType(int value, string? name = null) : base(value, name)
        {
        }
    }
}