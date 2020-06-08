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
            public static readonly BindablePropertyDescriptor<object, object?> Enabled = nameof(Enabled);
            public static readonly BindableMethodDescriptor<object> ElementSource = nameof(ElementSource);
            public static readonly BindableMethodDescriptor<object> RelativeSource = nameof(RelativeSource);
            public static readonly BindableMethodDescriptor<object> FindByName = "FindElementByName";

            #endregion
        }

        #endregion
    }
}