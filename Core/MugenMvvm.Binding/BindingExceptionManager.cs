using System;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Members;

namespace MugenMvvm.Binding
{
    internal static class BindingExceptionManager
    {
        #region Methods

        internal static void ThrowBindingMemberMustBeWritable(IBindingMemberInfo member)
        {
            throw new InvalidOperationException(BindingMessageConstants.BindingMemberMustBeWritableFormat4.Format(member.Name, member.Type, member.MemberType, member.Member));
        }

        internal static void ThrowBindingMemberMustBeReadable(IBindingMemberInfo member)
        {
            throw new InvalidOperationException(BindingMessageConstants.BindingMemberMustBeReadableFormat4.Format(member.Name, member.Type, member.MemberType, member.Member));
        }

        #endregion
    }
}