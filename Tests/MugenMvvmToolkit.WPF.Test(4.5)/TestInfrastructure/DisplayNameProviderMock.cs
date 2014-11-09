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

        /// <summary>
        ///     Gets a display name for the specified type using the specified member.
        /// </summary>
        /// <param name="memberInfo">The specified member.</param>
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