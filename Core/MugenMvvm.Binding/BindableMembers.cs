using MugenMvvm.Binding.Members.Descriptors;

namespace MugenMvvm.Binding
{
    public static class BindableMembers
    {
        #region Nested types

        public abstract class Object
        {
            #region Fields

            public static readonly BindablePropertyDescriptor<object, object?> Root = "R";
            public static readonly BindablePropertyDescriptor<object, object?> Parent = nameof(Parent);
            public static readonly BindablePropertyDescriptor<object, object?> DataContext = nameof(DataContext);
            public static readonly BindableMethodDescriptor<object> FindByNameMethod = "#FindByName";

            #endregion
        }

        #endregion
    }
}