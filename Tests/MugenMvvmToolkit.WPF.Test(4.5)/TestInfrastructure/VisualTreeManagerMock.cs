using System;
using System.Reflection;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class VisualTreeManagerMock : IVisualTreeManager
    {
        #region Properties

        public Func<Type, IBindingMemberInfo> GetParentMember { get; set; }

        public Func<object, object> FindParent { get; set; }

        public Func<object, string, object> FindByName { get; set; }

        public Func<object, string, uint, object> FindRelativeSource { get; set; }

        #endregion

        #region Implementation of ITargetTreeManager

        /// <summary>
        ///     Gets the name of parent member, if any.
        /// </summary>
        IBindingMemberInfo IVisualTreeManager.GetParentMember(Type type)
        {
            if (GetParentMember == null)
                return null;
            return GetParentMember(type);
        }

        object IVisualTreeManager.FindParent(object target)
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