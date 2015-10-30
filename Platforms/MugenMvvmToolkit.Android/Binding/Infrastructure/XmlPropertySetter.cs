#region Copyright

// ****************************************************************************
// <copyright file="XmlPropertySetter.cs">
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

using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Android.Content;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Builders;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    [StructLayout(LayoutKind.Auto)]
    public struct XmlPropertySetter<TWrapper, TTarget>
        where TTarget : class
    {
        #region Fields

        private readonly Context _context;
        private readonly BindingSet _bindingSet;
        private readonly TTarget _target;

        #endregion

        #region Constructors

        public XmlPropertySetter([NotNull]TTarget target, [NotNull] Context context, [NotNull] BindingSet bindingSet)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNull(context, "context");
            Should.NotBeNull(bindingSet, "bindingSet");
            _target = target;
            _context = context;
            _bindingSet = bindingSet;
        }

        #endregion

        #region Properties

        public BindingSet BindingSet
        {
            get { return _bindingSet; }
        }

        #endregion

        #region Methods

        public void SetBoolProperty(Func<Expression<Func<TWrapper, object>>> propertyName, string value)
        {
            SetBoolProperty(propertyName.GetMemberName(), value);
        }

        public void SetBoolProperty(string propertyName, string value)
        {
            SetStringProperty(propertyName, value, s => Empty.BooleanToObject(bool.Parse(s)));
        }

        public void SetEnumProperty<TEnum>(Func<Expression<Func<TWrapper, object>>> propertyName, string value)
            where TEnum : struct
        {
            SetStringProperty(propertyName, value, s => (TEnum)Enum.Parse(typeof(TEnum), s.Replace("|", ","), true));
        }

        public void SetStringProperty(Func<Expression<Func<TWrapper, object>>> propertyName, string value, Func<string, object> convertAction = null)
        {
            SetStringProperty(propertyName.GetMemberName(), value, convertAction);
        }

        public void SetStringProperty(string propertyName, string value, Func<string, object> convertAction = null)
        {
            if (value == null)
                return;
            var objectToSet = TryGetResourceAsString(value);
            if (objectToSet == null)
            {
                if (value.StartsWith("{"))
                {
                    _bindingSet.BindFromExpression(_target, propertyName, ToBindingString(value));
                    return;
                }
                objectToSet = value;
            }
            var member = BindingServiceProvider.MemberProvider.GetBindingMember(typeof(TTarget), propertyName, false, true);
            member.SetSingleValue(_target, convertAction == null ? objectToSet : convertAction(objectToSet));
        }

        public void SetProperty(Func<Expression<Func<TWrapper, object>>> propertyName, string value)
        {
            SetProperty(propertyName.GetMemberName(), value);
        }

        public void SetProperty(string propertyName, string value)
        {
            if (value == null)
                return;
            object objectToSet = TryGetResourceIdentifier(value);
            if (objectToSet == null)
            {
                if (value.StartsWith("{"))
                {
                    _bindingSet.BindFromExpression(_target, propertyName, ToBindingString(value));
                    return;
                }
                objectToSet = value;
            }
            var member = BindingServiceProvider.MemberProvider.GetBindingMember(typeof(TTarget), propertyName, false, true);
            member.SetSingleValue(_target, objectToSet);
        }

        public void SetBinding(Func<Expression<Func<TWrapper, object>>> propertyName, string value, bool required)
        {
            SetBinding(propertyName.GetMemberName(), value, required);
        }

        public void SetBinding(string propertyName, string value, bool required)
        {
            AddBinding(_bindingSet, _target, propertyName, value, required);
        }

        public void Apply()
        {
            _bindingSet.Apply();
        }

        private string TryGetResourceAsString(string value)
        {
            var resourceIdentifier = TryGetResourceIdentifier(value);
            if (resourceIdentifier == null)
                return null;
            return _context.GetString(resourceIdentifier.Value);
        }

        private int? TryGetResourceIdentifier(string value)
        {
            if (value == null)
                return null;
            if (value.StartsWith("/@"))
                value = value.Substring(1);
            else if (!value.StartsWith("@"))
                return null;
            var result = _context.Resources.GetIdentifier(value, null, _context.PackageName);
            if (result != 0)
                return result;
            result = _context.Resources.GetIdentifier(value.ToLowerInvariant(), null, _context.PackageName);
            if (result == 0)
                return null;
            return result;
        }

        internal static void AddBinding(BindingSet bindingSet, object target, string propertyName, string value, bool required)
        {
            if (value == null)
            {
                if (!required)
                    return;
                value = string.Empty;
            }
            bindingSet.BindFromExpression(target, propertyName, ToBindingString(value));
        }

        private static string ToBindingString(string expression)
        {
            if (expression == "{}")
                return string.Empty;
            if (!expression.EndsWith("}"))
                throw new ArgumentException(
                    string.Format("The binding string should start with '{{' symbol and end with '}}' symbol, invalid string: '{0}'", expression),
                    expression);
            return expression.Substring(1, expression.Length - 2);
        }

        #endregion
    }
}
