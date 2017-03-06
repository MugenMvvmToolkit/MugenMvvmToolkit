#region Copyright

// ****************************************************************************
// <copyright file="DisplayNameAttribute.cs">
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

// ReSharper disable once CheckNamespace
namespace System.ComponentModel
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
    public class DisplayNameAttribute : Attribute
    {
        public DisplayNameAttribute()
            : this(string.Empty)
        {
        }

        public DisplayNameAttribute(string displayName)
        {
            DisplayNameValue = displayName;
        }

        public virtual string DisplayName => DisplayNameValue;

        protected string DisplayNameValue { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            var displayNameAttribute = obj as DisplayNameAttribute;
            if (displayNameAttribute != null)
                return displayNameAttribute.DisplayName == DisplayName;
            return false;
        }

        public override int GetHashCode()
        {
            return DisplayName.GetHashCode();
        }
    }
}
