#region Copyright

// ****************************************************************************
// <copyright file="DataBindingExtension.cs">
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
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Models.Exceptions;
// ReSharper disable CheckNamespace
#if WPF
using System.ComponentModel;
using MugenMvvmToolkit.WPF.Binding.Models;

namespace MugenMvvmToolkit.WPF.MarkupExtensions
#elif SILVERLIGHT
using MugenMvvmToolkit.Silverlight.Binding.Models;

namespace MugenMvvmToolkit.Silverlight.MarkupExtensions
#endif

// ReSharper restore CheckNamespace
{
    public partial class DataBindingExtension : MarkupExtension
    {
        #region Fields

        private static Type _sharedDpType;
        private string _targetMemberName;

        #endregion

        #region Constructors

        public DataBindingExtension()
        {
            _targetMemberName = string.Empty;
        }

#if WPF
        public DataBindingExtension(string path)
            : this()
        {
            Path = path;
        }
#endif
        #endregion

        #region Overrides of MarkupExtension

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = serviceProvider.GetService<IProvideValueTarget>();
            if (target == null)
                return GetEmptyValue();
            var targetObject = target.TargetObject;
            var targetProperty = target.TargetProperty;
            if (targetObject == null || targetProperty == null)
                return GetEmptyValue();
            if (!(targetObject is DependencyObject))
            {
                var type = targetObject.GetType();
                if (_sharedDpType == type || "System.Windows.SharedDp".Equals(type.FullName, StringComparison.Ordinal))
                {
                    if (_sharedDpType == null)
                        _sharedDpType = type;
                    return this;
                }
            }
#if WPF
            if (targetObject is Setter || targetObject is DataTrigger || targetObject is Condition)
#else
            if (targetObject is Setter)
#endif
                return this;

            if (_targetMemberName == string.Empty)
                _targetMemberName = GetMemberName(targetObject, targetProperty);
            if (_targetMemberName == null)
                return GetEmptyValue();

            IDataBinding binding = HasValue
                ? CreateBindingBuilder(targetObject, _targetMemberName).Build()
                : CreateBinding(targetObject, _targetMemberName);

            if (ServiceProvider.DesignTimeManager.IsDesignMode && binding is InvalidDataBinding)
                throw new DesignTimeException(((InvalidDataBinding)binding).Exception);
            return GetDefaultValue(targetObject, targetProperty, binding, _targetMemberName);
        }

        #endregion

        #region Methods

        protected virtual string GetMemberName(object targetObject, object targetProperty)
        {
#if WPF
            var depProp = targetProperty as DependencyProperty;
            if (depProp != null)
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(depProp, targetObject.GetType());
                if (descriptor != null && descriptor.IsAttached)
                    return RegisterAttachedProperty(depProp, targetObject);
                return depProp.Name;
            }
            var methodInfo = targetProperty as MethodInfo;
            if (methodInfo != null && methodInfo.IsStatic && methodInfo.Name.StartsWith("Add", StringComparison.Ordinal) &&
                methodInfo.Name.EndsWith("Handler", StringComparison.Ordinal))
                return methodInfo.Name.Substring(3, methodInfo.Name.Length - 10);
#endif
            var member = (MemberInfo)targetProperty;
#if !WPF
            if (member.MemberType == MemberTypes.Method)
                return RegisterAttachedProperty((MethodInfo)member, targetObject);
#endif
            return member.Name;
        }

#if WPF
        private static string RegisterAttachedProperty(DependencyProperty property, object target)
        {
            var targetType = target.GetType();
            var path = property.Name + "Property";
            var member = BindingServiceProvider
                                        .MemberProvider
                                        .GetBindingMember(targetType, path, false, false);
            if (member == null)
            {
                BindingServiceProvider
                               .MemberProvider
                               .Register(targetType,
                                   new DependencyPropertyBindingMember(property, path, property.PropertyType,
                                       property.ReadOnly, null, null), true);
            }
            return path;
        }
#else
        private static string RegisterAttachedProperty(MethodInfo method, object target)
        {
            if (!(target is DependencyObject))
                return method.Name;

            Type declaringType = method.DeclaringType;
            if (declaringType == null)
                return null;

            var targetType = target.GetType();
            string name = method.Name.Replace("Get", string.Empty) + "Property";
            var memberInfo = BindingServiceProvider
                                            .MemberProvider
                                            .GetBindingMember(targetType, name, false, false);
            if (memberInfo != null)
                return name;

            var fullName = $"_attached_{declaringType.FullName.Replace(".", "_")}_{name}";
            memberInfo = BindingServiceProvider
                                        .MemberProvider
                                        .GetBindingMember(targetType, fullName, false, false);

            if (memberInfo != null)
                return fullName;

            FieldInfo fieldInfo = declaringType.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (fieldInfo == null)
                return null;

            BindingServiceProvider
                           .MemberProvider
                           .Register(method.GetParameters()[0].ParameterType,
                               new DependencyPropertyBindingMember((DependencyProperty)fieldInfo.GetValue(null), fullName, method.ReturnType, false, method, null), true);
            return fullName;
        }
#endif

        #endregion
    }
}
