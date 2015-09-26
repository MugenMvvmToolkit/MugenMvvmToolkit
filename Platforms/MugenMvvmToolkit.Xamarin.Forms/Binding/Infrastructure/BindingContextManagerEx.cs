#region Copyright

// ****************************************************************************
// <copyright file="BindingContextManagerEx.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Infrastructure
{
    public class BindingContextManagerEx : BindingContextManager
    {
        #region Fields

        private const string DataContextName = "BindingContext";

        #endregion

        #region Overrides of BindingContextManager

        protected override IBindingMemberInfo GetExplicitDataContextMember(object source)
        {
            IBindingMemberInfo member = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(source.GetType(), DataContextName, true, false);
            if (member != null && member.Type.Equals(typeof(object)) && member.CanRead && member.CanWrite)
                return member;
            return null;
        }

        #endregion
    }
}
