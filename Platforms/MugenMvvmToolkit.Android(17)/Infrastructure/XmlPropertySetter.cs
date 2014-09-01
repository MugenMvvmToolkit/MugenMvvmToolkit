#region Copyright
// ****************************************************************************
// <copyright file="XmlPropertySetter.cs">
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
using System.Linq.Expressions;
using Android.Content;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Infrastructure
{
    public struct XmlPropertySetter<TWrapper, TTarget>
    {
        #region Fields

        private readonly Context _context;
        private readonly TTarget _target;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="XmlPropertySetter{TWrapper,TTarget}" /> class.
        /// </summary>
        public XmlPropertySetter(TTarget target, Context context)
        {
            _target = target;
            _context = context;
        }

        #endregion

        #region Methods

        public void SetBoolProperty(Expression<Func<TWrapper, object>> propertyName, string value)
        {
            SetBoolProperty(MvvmExtensions.GetPropertyName(propertyName), value);
        }

        public void SetBoolProperty(string propertyName, string value)
        {
            SetStringProperty(propertyName, value, s => bool.Parse(s));
        }

        public void SetEnumProperty<TEnum>(Expression<Func<TWrapper, object>> propertyName, string value)
            where TEnum : struct
        {
            SetStringProperty(propertyName, value, s => (TEnum)Enum.Parse(typeof(TEnum), s.Replace("|", ","), true));
        }

        public void SetStringProperty(Expression<Func<TWrapper, object>> propertyName, string value, Func<string, object> convertAction = null)
        {
            SetStringProperty(MvvmExtensions.GetPropertyName(propertyName), value, convertAction);
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
                    BindingServiceProvider.BindingProvider.CreateBindingFromString(_target, propertyName, ToBindingString(value));
                    return;
                }
                objectToSet = value;
            }
            var member = BindingServiceProvider.MemberProvider.GetBindingMember(typeof(TTarget), propertyName, false, true);
            member.SetValue(_target, new[] { convertAction == null ? objectToSet : convertAction(objectToSet) });
        }

        public void SetProperty(Expression<Func<TWrapper, object>> propertyName, string value)
        {
            SetProperty(MvvmExtensions.GetPropertyName(propertyName), value);
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
                    BindingServiceProvider.BindingProvider.CreateBindingFromString(_target, propertyName, ToBindingString(value));
                    return;
                }
                objectToSet = value;
            }
            var member = BindingServiceProvider.MemberProvider.GetBindingMember(typeof(TTarget), propertyName, false, true);
            member.SetValue(_target, new[] { objectToSet });
        }

        public void SetBinding(Expression<Func<TWrapper, object>> propertyName, string value, bool required)
        {
            SetBinding(MvvmExtensions.GetPropertyName(propertyName), value, required);
        }

        public void SetBinding(string propertyName, string value, bool required)
        {
            if (value == null)
            {
                if (!required)
                    return;
                value = string.Empty;
            }
            BindingServiceProvider.BindingProvider.CreateBindingFromString(_target, propertyName, ToBindingString(value));
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