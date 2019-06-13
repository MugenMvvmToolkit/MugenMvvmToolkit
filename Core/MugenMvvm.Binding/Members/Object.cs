using MugenMvvm.Binding.Infrastructure.Members;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public abstract class ObjectBindableMembers
    {
        #region Fields

        public static readonly BindingMemberDescriptor<object, object?> Root = "R";
        public static readonly BindingMemberDescriptor<object, object?> Parent = nameof(Parent);
        public static readonly BindingMemberDescriptor<object, object?> DataContext = nameof(DataContext);
        public static readonly BindingMemberDescriptor<object, object?> FindByNameMethod = "#FindByName";

        #endregion
    }
}