#region Copyright
// ****************************************************************************
// <copyright file="VisualTreeManager.cs">
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
using System;
using System.Reflection;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the visual tree manager.
    /// </summary>
    public class VisualTreeManager : IVisualTreeManager
    {
        #region Implementation of ITargetTreeManager

        /// <summary>
        ///     Gets the parent member, if any.
        /// </summary>
        public virtual IBindingMemberInfo GetParentMember(Type type)
        {
            Should.NotBeNull(type, "type");
            return BindingProvider.Instance.MemberProvider.GetBindingMember(type, AttachedMemberConstants.Parent, false, false);
        }

        /// <summary>
        ///     Tries to find parent.
        /// </summary>
        public virtual object FindParent(object target)
        {
            Should.NotBeNull(target, "target");
            Type type = target.GetType();
            IBindingMemberInfo parentProp = GetParentMember(type);
            return parentProp == null ? null : parentProp.GetValue(target, null);
        }

        /// <summary>
        ///     Tries to find element by it's name.
        /// </summary>
        public virtual object FindByName(object target, string elementName)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNullOrWhitespace(elementName, "elementName");
            var member = BindingProvider
                .Instance
                .MemberProvider
                .GetBindingMember(target.GetType(), AttachedMemberConstants.FindByNameMethod, false, false);
            if (member == null)
                return null;
            return member.GetValue(target, new object[] { elementName });
        }

        /// <summary>
        ///     Tries to find relative source.
        /// </summary>
        public virtual object FindRelativeSource(object target, string typeName, uint level)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNullOrWhitespace(typeName, "typeName");
            object fullNameSource = null;
            object nameSource = null;
            uint fullNameLevel = 0;
            uint nameLevel = 0;

            target = FindParent(target);
            while (target != null)
            {
                bool shortNameEqual;
                bool fullNameEqual;
                TypeNameEqual(target.GetType(), typeName, out shortNameEqual, out fullNameEqual);
                if (shortNameEqual)
                {
                    nameSource = target;
                    nameLevel++;
                }
                if (fullNameEqual)
                {
                    fullNameSource = target;
                    fullNameLevel++;
                }

                if (fullNameSource != null && fullNameLevel == level)
                    return fullNameSource;
                if (nameSource != null && nameLevel == level)
                    return nameSource;

                target = FindParent(target);
            }           
            return null;
        }

        #endregion

        #region Methods

        private static void TypeNameEqual(Type type, string typeName, out bool shortNameEqual, out bool fullNameEqual)
        {
            shortNameEqual = false;
            fullNameEqual = false;
            while (type != null)
            {
                if (!shortNameEqual)
                {
                    if (type.Name == typeName)
                    {
                        shortNameEqual = true;
                        if (fullNameEqual)
                            break;
                    }
                }
                if (!fullNameEqual && type.FullName == typeName)
                {
                    fullNameEqual = true;
                    if (shortNameEqual)
                        break;
                }
#if PCL_WINRT
                type = type.GetTypeInfo().BaseType;
#else
                type = type.BaseType;
#endif

            }
        }

        #endregion
    }
}