using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding
{
    public static class AttachedMembersBase
    {
        #region Nested types

        public abstract class Object //todo licence
        {
            #region Fields

            public static readonly BindingMemberDescriptor<object, object> DataContext;
            public static readonly BindingMemberDescriptor<object, object> Parent;
            public static readonly BindingMemberDescriptor<object, object> Root;
            public static readonly BindingMemberDescriptor<object, object> CommandParameter;
            public static readonly BindingMemberDescriptor<object, IEnumerable<object>> Errors;
            public static readonly BindingMemberDescriptor<object, bool> HasErrors;
            public static readonly BindingMemberDescriptor<object, bool> IsFlatTree;
            public static readonly BindingMemberDescriptor<object, bool> IsFlatContext;

            #endregion

            #region Constructors

            static Object()
            {
                DataContext = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.DataContext);
                Parent = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.Parent);
                Root = new BindingMemberDescriptor<object, object>(nameof(Root));
                CommandParameter = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.CommandParameter);
                Errors = new BindingMemberDescriptor<object, IEnumerable<object>>(AttachedMemberConstants.ErrorsPropertyMember);
                HasErrors = new BindingMemberDescriptor<object, bool>(nameof(HasErrors));
                IsFlatTree = new BindingMemberDescriptor<object, bool>(nameof(IsFlatTree));
                IsFlatContext = new BindingMemberDescriptor<object, bool>(nameof(IsFlatContext));
            }

            #endregion
        }

        #endregion
    }
}