#region Copyright

// ****************************************************************************
// <copyright file="XmlPropertySetter.cs">
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
using System.Collections.Generic;
using Android.Content;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    public sealed class XmlPropertySetter : List<IBindingBuilder>
    {
        #region Fields

        private readonly Context _context;
        private readonly object _target;

        #endregion

        #region Constructors

        public XmlPropertySetter([NotNull]object target, [NotNull] Context context)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(context, nameof(context));
            _target = target;
            _context = context;
        }

        #endregion

        #region Methods

        public void SetBoolProperty(string propertyName, string value)
        {
            SetStringProperty(propertyName, value, s => Empty.BooleanToObject(bool.Parse(s)));
        }

        public void SetEnumProperty<TEnum>(string propertyName, string value)
            where TEnum : struct
        {
            SetStringProperty(propertyName, value, s => (TEnum)Enum.Parse(typeof(TEnum), s.Replace("|", ","), true));
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
                    AddRange(Bind(_target, propertyName, ToBindingString(value)));
                    return;
                }
                objectToSet = value;
            }
            var member = BindingServiceProvider.MemberProvider.GetBindingMember(_target.GetType(), propertyName, false, true);
            member.SetSingleValue(_target, convertAction == null ? objectToSet : convertAction(objectToSet));
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
                    AddRange(Bind(_target, propertyName, ToBindingString(value)));
                    return;
                }
                objectToSet = value;
            }
            var member = BindingServiceProvider.MemberProvider.GetBindingMember(_target.GetType(), propertyName, false, true);
            member.SetSingleValue(_target, objectToSet);
        }

        public void SetBinding(string propertyName, string value, bool required)
        {
            AddBinding(this, _target, propertyName, value, required);
        }

        public void Bind(object target, string expression)
        {
            AddRange(BindingServiceProvider.BindingProvider.CreateBuildersFromString(target, expression));
        }

        public void Apply()
        {
            for (int i = 0; i < Count; i++)
                this[i].Build();
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

        internal static void AddBinding(XmlPropertySetter bindingSet, object target, string propertyName, string value, bool required)
        {
            if (value == null)
            {
                if (!required)
                    return;
                value = string.Empty;
            }
            bindingSet.AddRange(Bind(target, propertyName, ToBindingString(value)));
        }

        private static IList<IBindingBuilder> Bind(object target, string targetPath, string bindingExpression)
        {
            return BindingServiceProvider.BindingProvider.CreateBuildersFromString(target, targetPath + " " + bindingExpression + ";");
        }

        private static string ToBindingString(string expression)
        {
            if (expression == "{}")
                return string.Empty;
            if (!expression.EndsWith("}"))
                throw new ArgumentException($"The binding string should start with '{{' symbol and end with '}}' symbol, invalid string: '{expression}'", expression);
            return expression.Substring(1, expression.Length - 2);
        }

        #endregion
    }
}
