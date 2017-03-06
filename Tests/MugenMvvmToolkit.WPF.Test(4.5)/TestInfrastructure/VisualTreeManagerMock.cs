#region Copyright

// ****************************************************************************
// <copyright file="VisualTreeManagerMock.cs">
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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class VisualTreeManagerMock : IVisualTreeManager
    {
        #region Properties

        public Func<Type, IBindingMemberInfo> GetParentMember { get; set; }

        public Func<Type, IBindingMemberInfo> GetRootMember { get; set; }

        public Func<object, object> FindParent { get; set; }

        public Func<object, string, object> FindByName { get; set; }

        public Func<object, string, uint, object> FindRelativeSource { get; set; }

        #endregion

        #region Implementation of ITargetTreeManager

        IBindingMemberInfo IVisualTreeManager.GetRootMember(Type type)
        {
            if (GetRootMember == null)
                return null;
            return GetRootMember(type);
        }

        IBindingMemberInfo IVisualTreeManager.GetParentMember(Type type)
        {
            if (GetParentMember == null)
                return null;
            return GetParentMember(type);
        }

        object IVisualTreeManager.GetParent(object target)
        {
            if (FindParent == null)
                return null;
            return FindParent(target);
        }

        object IVisualTreeManager.FindByName(object target, string elementName)
        {
            if (FindByName == null)
                return null;
            return FindByName(target, elementName);
        }

        object IVisualTreeManager.FindRelativeSource(object target, string typeName, uint level)
        {
            if (FindRelativeSource == null)
                return null;
            return FindRelativeSource(target, typeName, level);
        }

        #endregion
    }
}
