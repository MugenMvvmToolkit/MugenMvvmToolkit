namespace MugenMvvm.Enums
{
    public class GroupHeaderChangedAction : EnumBase<GroupHeaderChangedAction, int>
    {
        public static readonly GroupHeaderChangedAction ItemAdded = new(1);
        public static readonly GroupHeaderChangedAction ItemChanged = new(2);
        public static readonly GroupHeaderChangedAction ItemRemoved = new(3);
        public static readonly GroupHeaderChangedAction Clear = new(4);

        public GroupHeaderChangedAction(int value, string? name = null) : base(value, name)
        {
        }
    }
}