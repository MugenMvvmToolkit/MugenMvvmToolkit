#region Copyright

// ****************************************************************************
// <copyright file="AutoCompleteItem.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

namespace MugenMvvmToolkit.WinForms.Binding.Models
{
    internal sealed class AutoCompleteItem
    {
        #region Fields

        public readonly string DisplayName;
        public readonly MemberTypes MemberType;
        public readonly Type Type;
        public readonly string Value;

        #endregion

        #region Constructors

        public AutoCompleteItem(FieldInfo field)
            : this(field.Name, field.Name, field.MemberType, field.FieldType)
        {
        }

        public AutoCompleteItem(PropertyInfo property)
            : this(property.Name, property.Name, property.MemberType, property.PropertyType)
        {
        }

        public AutoCompleteItem(EventInfo eventInfo)
            : this(eventInfo.Name, eventInfo.Name, eventInfo.MemberType, eventInfo.EventHandlerType)
        {
        }

        public AutoCompleteItem(string displayName, string value, MemberTypes? memberType = null, Type type = null)
        {
            Should.NotBeNull(displayName, nameof(displayName));
            Should.NotBeNull(value, nameof(value));
            Type = type ?? typeof(object);
            DisplayName = memberType.HasValue
                ? $"{displayName} ({Type.Name} - {(memberType.Value == MemberTypes.Custom ? "Attached" : memberType.Value.ToString())})"
                : displayName;
            Value = value;
            MemberType = memberType.GetValueOrDefault(MemberTypes.Custom);
        }

        #endregion

        #region Overrides of ValueType

        public override string ToString()
        {
            return DisplayName;
        }

        #endregion
    }
}
