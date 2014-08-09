#region Copyright
// ****************************************************************************
// <copyright file="AutoCompleteItem.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Reflection;

namespace MugenMvvmToolkit.Binding.Models
{
    internal sealed class AutoCompleteItem
    {
        #region Fields

        private readonly string _displayName;
        private readonly MemberTypes _type;
        private readonly string _value;

        #endregion

        #region Constructors

        public AutoCompleteItem(MemberInfo member)
            : this(member.Name, member.Name, member.MemberType)
        {
        }

        public AutoCompleteItem(string displayName, string value, MemberTypes? type = null)
        {
            Should.NotBeNull(displayName, "displayName");
            Should.NotBeNull(value, "value");
            _displayName = type.HasValue ? string.Format("{0} ({1})", displayName, type.Value) : displayName;
            _value = value;
            _type = type.GetValueOrDefault(MemberTypes.Custom);
        }

        #endregion

        #region Properties

        public string DisplayName
        {
            get { return _displayName; }
        }

        public string Value
        {
            get { return _value; }
        }

        public MemberTypes Type
        {
            get { return _type; }
        }

        #endregion
    }
}