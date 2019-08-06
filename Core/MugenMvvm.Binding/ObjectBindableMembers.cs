using MugenMvvm.Binding.Members;

namespace MugenMvvm.Binding
{
    public abstract class ObjectBindableMembers
    {
        #region Fields

        public static readonly BindableMember<object, object?> Root = "R";
        public static readonly BindableMember<object, object?> Parent = nameof(Parent);
        public static readonly BindableMember<object, object?> DataContext = nameof(DataContext);
        public static readonly BindableMember<object, object?> FindByNameMethod = "#FindByName";

        #endregion
    }
}