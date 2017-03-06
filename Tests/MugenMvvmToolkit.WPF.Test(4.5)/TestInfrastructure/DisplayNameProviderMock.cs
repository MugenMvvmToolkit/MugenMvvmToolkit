#region Copyright

// ****************************************************************************
// <copyright file="DisplayNameProviderMock.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

using System;
using System.Reflection;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class DisplayNameProviderMock : IDisplayNameProvider
    {
        #region Properties

        public MemberInfo Member { get; set; }

        public Func<MemberInfo, string> GetNameDelegate { get; set; }

        #endregion

        #region Implementation of IDisplayNameProvider

        Func<string> IDisplayNameProvider.GetDisplayNameAccessor(MemberInfo memberInfo)
        {
            Member = memberInfo;
            if (GetNameDelegate == null)
                return () => memberInfo.Name;
            var nameDelegate = GetNameDelegate;
            return () => nameDelegate(memberInfo);
        }

        #endregion
    }
}
